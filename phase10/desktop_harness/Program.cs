using System;using System.IO;using System.Net.Sockets;using System.Threading.Tasks;using System.Reflection;using System.Threading;using QuestEagleEye;using UnityEngine;using UnityEngine.UI;
class Program {
 static int Main(string[] args) { try {
  if(args.Length!=2) throw new Exception("Usage: dotnet run -- decode <fixture_file> | tcp <port>");
  if(args[0]=="receiver") {
   var receiver=new QeeThermalReceiver { host="127.0.0.1", port=int.Parse(args[1]), output=new RawImage(), statusText=new Text(), staleAfterSeconds=0.3f, reconnectMinSeconds=0.1f, reconnectMaxSeconds=0.2f };
   var update=typeof(QeeThermalReceiver).GetMethod("Update",BindingFlags.Instance|BindingFlags.NonPublic);
   var destroy=typeof(QeeThermalReceiver).GetMethod("OnDestroy",BindingFlags.Instance|BindingFlags.NonPublic);
   receiver.Connect(); int observed=0, applied=0; bool staleObserved=false;
   try {
    for(int i=0;i<120;i++) {
     Thread.Sleep(30); update.Invoke(receiver,null);
     if(receiver.output.enabled) observed++;
     if(receiver.output.texture is Texture2D tex) {
      applied=Math.Max(applied,tex.applyCount);
      if(tex.lastPixels==null || tex.lastPixels.Length!=tex.width*tex.height) throw new Exception("Texture pixel buffer invalid");
     }
     if(observed>0 && !receiver.output.enabled) staleObserved=true;
    }
   } finally { destroy.Invoke(receiver,null); }
   if(receiver.output.enabled) throw new Exception("Destroy left texture enabled");
   if(receiver.output.texture!=null) throw new Exception("Destroy retained texture reference");
   if(observed==0) throw new Exception("Receiver never displayed valid frame");
   if(applied==0) throw new Exception("Texture Apply was never executed");
   if(!staleObserved) throw new Exception("Receiver never hid disconnected or stale frame");
   Console.WriteLine($"RECEIVER_PASS:{observed}:applies={applied}:stale={staleObserved}"); return 0;
  }

  if(args[0]=="phase11") {
   var receiver=new QeeThermalReceiver {host="127.0.0.1",port=int.Parse(args[1]),output=new RawImage(),statusText=new Text(),staleAfterSeconds=0.20f,reconnectMinSeconds=0.08f,reconnectMaxSeconds=0.16f};
   var t=typeof(QeeThermalReceiver);
   var update=t.GetMethod("Update",BindingFlags.Instance|BindingFlags.NonPublic);
   var destroy=t.GetMethod("OnDestroy",BindingFlags.Instance|BindingFlags.NonPublic);
   var session=t.GetField("sessionId",BindingFlags.Instance|BindingFlags.NonPublic);
   var q=t.GetField("queue",BindingFlags.Instance|BindingFlags.NonPublic);
   var qlock=t.GetField("queueLock",BindingFlags.Instance|BindingFlags.NonPublic);
   var w=t.GetField("worker",BindingFlags.Instance|BindingFlags.NonPublic);
   receiver.Connect();
   int live=0, hiddenAfterLive=0, sessionChanges=0, maxQueued=0, applies=0, prior=-1; bool sawFirstColor=false,sawSecondColor=false;
   try {
    for(int i=0;i<220;i++) {
     Thread.Sleep(20);
     lock(qlock.GetValue(receiver)) { var waiting=(System.Collections.ICollection)q.GetValue(receiver);maxQueued=Math.Max(maxQueued,waiting.Count);if(waiting.Count>2)throw new Exception("Queue exceeded bound before update"); }
     update.Invoke(receiver,null);
     int current=(int)session.GetValue(receiver);
     if(current!=prior && prior>=0)sessionChanges++;
     prior=current;
     lock(qlock.GetValue(receiver)) { var queue=(System.Collections.ICollection)q.GetValue(receiver);maxQueued=Math.Max(maxQueued,queue.Count);if(queue.Count>2) throw new Exception("Queue overflow"); }
     if(receiver.output.enabled)live++;
     else if(live>0)hiddenAfterLive++;
     if(receiver.output.texture is Texture2D texture) {
       applies=Math.Max(applies,texture.applyCount);
       if(texture.lastPixels!=null && texture.lastPixels.Length>0){
        if(texture.lastPixels[0].r<20)sawFirstColor=true;
        if(texture.lastPixels[0].r>150)sawSecondColor=true;
       }
     }
    }
    if(live==0 || applies==0)throw new Exception("No rendered TCP frames");
    if(hiddenAfterLive==0)throw new Exception("Disconnect or staleness never hid display");
    if(sessionChanges<2 || !sawFirstColor || !sawSecondColor)throw new Exception($"Two distinct connected sessions not displayed: first={sawFirstColor} second={sawSecondColor}"); 
    if(receiver.DroppedFrames==0)throw new Exception("Stress stream did not exercise queue overflow/drop policy");
    receiver.Disconnect();
    update.Invoke(receiver,null);
    if(receiver.output.enabled)throw new Exception("Disconnect did not hide output");
    var oldWorker=(Task)w.GetValue(receiver);
    if(oldWorker!=null && !oldWorker.Wait(3000))throw new Exception("Cancellation worker did not complete");
    if((System.Collections.ICollection)q.GetValue(receiver) is var remaining && remaining.Count>2)throw new Exception("Queue exceeded bound after cancellation");
   } finally { destroy.Invoke(receiver,null); }
   if(receiver.output.texture!=null||receiver.output.enabled)throw new Exception("Texture not released on destroy");
   Console.WriteLine($"PHASE11_PASS:live={live}:hidden={hiddenAfterLive}:sessions={sessionChanges}:maxqueue={maxQueued}:applies={applies}:dropped={receiver.DroppedFrames}:second={sawSecondColor}");
   return 0;
  }
  if(args[0]=="decode") {using var s=File.OpenRead(args[1]);var f=QeeWireProtocol.ReadFrame(s);if(s.Position!=s.Length) throw new Exception("Trailing stream data");Console.WriteLine($"PASS:{f.Metadata.frame_id}:{f.Pixels.Length}");return 0;}
  if(args[0]=="tcp") {using var client=new TcpClient("127.0.0.1",int.Parse(args[1]));using var stream=client.GetStream();int count=0;try {while(true){QeeWireProtocol.ReadFrame(stream);count++;}}catch(EndOfStreamException){} if(count!=8)throw new Exception($"Expected 8 received frames; got {count}");Console.WriteLine($"TCP_PASS:{count}");return 0;}
  throw new Exception("unknown mode");
 } catch(Exception e) {Console.Error.WriteLine(e);return 1;} }
}