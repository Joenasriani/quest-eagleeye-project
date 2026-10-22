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
  if(args[0]=="decode") {using var s=File.OpenRead(args[1]);var f=QeeWireProtocol.ReadFrame(s);if(s.Position!=s.Length) throw new Exception("Trailing stream data");Console.WriteLine($"PASS:{f.Metadata.frame_id}:{f.Pixels.Length}");return 0;}
  if(args[0]=="tcp") {using var client=new TcpClient("127.0.0.1",int.Parse(args[1]));using var stream=client.GetStream();int count=0;try {while(true){QeeWireProtocol.ReadFrame(stream);count++;}}catch(EndOfStreamException){} if(count!=8)throw new Exception($"Expected 8 received frames; got {count}");Console.WriteLine($"TCP_PASS:{count}");return 0;}
  throw new Exception("unknown mode");
 } catch(Exception e) {Console.Error.WriteLine(e);return 1;} }
}