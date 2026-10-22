// Quest-compatible *candidate* Unity C# consumer. Desktop compilation and physical Quest runtime NOT VERIFIED.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace QuestEagleEye {
    public sealed class QeeThermalReceiver : MonoBehaviour {
        [Header("Synthetic-only reference service. Never use for safety-critical sensing.")]
        public string host = "127.0.0.1";
        public int port = 50831;
        public bool connectOnStart = false;
        public RawImage output;
        public Text statusText;
        public float staleAfterSeconds = 1f;
        public float reconnectMinSeconds = 0.5f;
        public float reconnectMaxSeconds = 8f;
        const int MaxQueuedFrames = 2;
        readonly Queue<QueuedFrame> queue = new Queue<QueuedFrame>();
        readonly object queueLock = new object();
        struct QueuedFrame { public int Session; public QeeFrame Frame; public QueuedFrame(int session, QeeFrame frame) {Session=session; Frame=frame;} }
        int sessionId;
        volatile bool socketConnected;
        readonly Stopwatch clock = Stopwatch.StartNew();
        readonly object workerLock = new object();
        bool destroyed;
        CancellationTokenSource cancellation;
        Task worker;
        Texture2D texture;
        Color32[] rgba;
        long lastFrameId = -1;
        long lastFrameReceivedTick;
        bool receivedAny;
        string networkStatus = "DISCONNECTED";
        int dropCount;
        int rejectedCount;
        public int DroppedFrames => dropCount;
        public int RejectedFrames => rejectedCount;

        void Start() { SetStatus("THERMAL_UNAVAILABLE · SYNTHETIC ONLY"); if(connectOnStart) Connect(); }
        public void Connect() {
            lock (workerLock) {
                if (destroyed || (worker != null && !worker.IsCompleted)) return;
                if (port < 1 || port > 65535) { SetStatus("INVALID_PORT"); return; }
                DisconnectCore(); // Invalidate previous session under workerLock.
                var previous = cancellation;
                cancellation = new CancellationTokenSource();
                var token = cancellation.Token;
                // Prior worker has completed; its cancellation registration is released.
                previous?.Dispose();
                worker = Task.Run(() => RunNetwork(token), token);
            }
        }
        async Task RunNetwork(CancellationToken token) {
            double delay=Math.Max(0.1f,reconnectMinSeconds);
            while (!token.IsCancellationRequested) {
                try {
                    using(var client=new TcpClient()) {
                        // Cancellation of ConnectAsync is implemented by closing the socket when disabled.
                        using(token.Register(()=>client.Close())) {
                            await client.ConnectAsync(host,port);
                            client.ReceiveTimeout=2000;
                            int connectionSession = Interlocked.Increment(ref sessionId);
                            ClearQueue();
                            socketConnected=true;
                            networkStatus="CONNECTED";
                            delay=Math.Max(0.1f,reconnectMinSeconds);
                            using(var stream=client.GetStream()) {
                                while(!token.IsCancellationRequested) {
                                    var frame=QeeWireProtocol.ReadFrame(stream);
                                    // Queue only frames from the currently active connection generation.
                                    if (connectionSession != Volatile.Read(ref sessionId)) break;
                                    lock (queueLock) {
                                        if (connectionSession != Volatile.Read(ref sessionId)) break;
                                        if (queue.Count >= MaxQueuedFrames) { queue.Dequeue(); IncrementSaturating(ref dropCount); }
                                        queue.Enqueue(new QueuedFrame(connectionSession, frame));
                                    }
                                }
                            }
                        }
                    }
                    InvalidateSession();
                } catch (Exception exc) when (exc is SocketException || exc is IOException || exc is InvalidDataException || exc is ObjectDisposedException || exc is ArgumentException || exc is InvalidOperationException) {
                    if (!token.IsCancellationRequested) { InvalidateSession(); IncrementSaturating(ref rejectedCount); }
                }
                if (!token.IsCancellationRequested) {
                    try {await Task.Delay(TimeSpan.FromSeconds(delay), token);}catch(OperationCanceledException) { break; }
                    delay=Math.Min(Math.Max(delay*2,0.1f), Math.Max(reconnectMaxSeconds,0.1f));
                }
            }
            InvalidateSession();
        }
        static void IncrementSaturating(ref int counter) { AddSaturating(ref counter, 1); }
        static void AddSaturating(ref int counter, long amount) {
            int oldValue, newValue;
            do {
                oldValue = Volatile.Read(ref counter);
                newValue = (int)Math.Min((long)int.MaxValue, (long)oldValue + Math.Max(0L, amount));
            } while (Interlocked.CompareExchange(ref counter, newValue, oldValue) != oldValue);
        }
        void ClearQueue() {
            lock (queueLock) queue.Clear();
        }
        void InvalidateSession() {
            Interlocked.Increment(ref sessionId);
            socketConnected=false;
            networkStatus="DISCONNECTED";
            ClearQueue();
        }
        int displayedSession=-1;
        void Update() {
            QeeFrame latest=null;
            int active=Volatile.Read(ref sessionId);
            if (displayedSession != active) { displayedSession=active; lastFrameId=-1; receivedAny=false; }
            lock (queueLock) {
                while(queue.Count > 0) {
                    var next=queue.Dequeue();
                    if(next.Session == active) latest=next.Frame;
                    else IncrementSaturating(ref dropCount);
                }
            }
            // A disconnect may have invalidated this session after queue draining.
            if (active != Volatile.Read(ref sessionId)) latest = null;
            if(latest!=null) {
                if(latest.Metadata.frame_id<=lastFrameId) {
                    IncrementSaturating(ref rejectedCount);
                } else {
                    if(lastFrameId>=0 && latest.Metadata.frame_id>lastFrameId+1)
                        AddSaturating(ref dropCount, Math.Min((long)int.MaxValue, latest.Metadata.frame_id - lastFrameId - 1));
                    lastFrameId=latest.Metadata.frame_id;
                    RenderFrame(latest);
                    lastFrameReceivedTick=clock.ElapsedTicks;
                    receivedAny=true;
                }
            }
            double age=receivedAny ? (clock.ElapsedTicks-lastFrameReceivedTick)/(double)Stopwatch.Frequency : double.PositiveInfinity;
            bool live=socketConnected && networkStatus=="CONNECTED" && active == Volatile.Read(ref sessionId) && age<=Math.Max(staleAfterSeconds,0.01f);
            if (destroyed) live = false;
            if(output!=null) output.enabled=live;
            SetStatus(live ? "SYNTHETIC · UNREGISTERED" : "THERMAL_UNAVAILABLE · SYNTHETIC ONLY");
        }
        void RenderFrame(QeeFrame frame) {
            int w=frame.Metadata.width,h=frame.Metadata.height;
            if(texture==null||texture.width!=w||texture.height!=h) {
                if(texture!=null) Destroy(texture);
                texture=new Texture2D(w,h,TextureFormat.RGBA32,false,false);
                texture.filterMode=FilterMode.Point;
                rgba=new Color32[w*h];
                if(output!=null) output.texture=texture;
            }
            // Raw uint16 pixel values are pseudo-counts, NOT calibrated degrees or sensor radiometry.
            for(int i=0;i<rgba.Length;i++) {
                ushort raw=(ushort)(frame.Pixels[2*i] | (frame.Pixels[2*i+1]<<8));
                byte gray=(byte)(raw>>8);
                rgba[i]=new Color32(gray,gray,gray,255);
            }
            texture.SetPixels32(rgba);texture.Apply(false,false);
        }
        void SetStatus(string text) {if(statusText!=null) statusText.text=text;}
        public void Disconnect() { lock (workerLock) DisconnectCore(); }
        void DisconnectCore() {
            InvalidateSession();
            try { cancellation?.Cancel(); } catch (ObjectDisposedException) { }
            // Cancellation closes the socket through the registered callback.
            receivedAny=false;lastFrameId=-1; displayedSession=-1;
            if(output!=null) output.enabled=false;
        }
        void OnDisable() {Disconnect();}
        void OnDestroy() {
            CancellationTokenSource oldTokenSource;
            Task oldWorker;
            lock (workerLock) {
                destroyed = true;
                DisconnectCore();
                oldTokenSource = cancellation;
                oldWorker = worker;
                cancellation = null;
                worker = null;
            }
            // Dispose after active worker and registrations finish.
            if (oldTokenSource != null) {
                if (oldWorker != null && !oldWorker.IsCompleted)
                    oldWorker.ContinueWith(_ => oldTokenSource.Dispose(), TaskScheduler.Default);
                else oldTokenSource.Dispose();
            }
            if (output != null) output.texture = null;
            if (texture != null) Destroy(texture);
        }
    }
}