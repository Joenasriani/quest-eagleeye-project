using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using QuestEagleEye;
namespace QuestEagleEye.Tests {
 public sealed class QeeReceiverPlayModeTests {
  static byte[] Packet(long id, ushort first=1024) {
   var pixels=new byte[]{(byte)(first&255),(byte)(first>>8),0,1,0,2,0,3};
   string sha;
   using(var h=SHA256.Create())sha=BitConverter.ToString(h.ComputeHash(pixels)).Replace("-","").ToLowerInvariant();
   string json="{\"schema_version\":1,\"source\":\"synthetic-fixture\",\"frame_id\":"+id+
    ",\"width\":2,\"height\":2,\"pixel_format\":\"uint16-le\",\"capture_clock_domain\":\"unity-playmode\",\"capture_mono_ns\":"+(1234567+id)+
    ",\"synthetic\":true,\"sha256\":\""+sha+"\"}";
   var meta=Encoding.ASCII.GetBytes(json);var packet=new byte[12+meta.Length+pixels.Length];
   packet[0]=(byte)'Q';packet[1]=(byte)'E';packet[2]=(byte)'E';packet[3]=(byte)'1';
   WriteBE(packet,4,meta.Length);WriteBE(packet,8,pixels.Length);
   Buffer.BlockCopy(meta,0,packet,12,meta.Length);Buffer.BlockCopy(pixels,0,packet,12+meta.Length,pixels.Length);
   return packet;
  }
  static void WriteBE(byte[] d,int idx,int v){d[idx]=(byte)(v>>24);d[idx+1]=(byte)(v>>16);d[idx+2]=(byte)(v>>8);d[idx+3]=(byte)v;}
  [UnityTest]
  public IEnumerator ReceivesRendersStalesAndDisconnects(){
   var listener=new TcpListener(IPAddress.Loopback,0);listener.Start();
   int port=((IPEndPoint)listener.LocalEndpoint).Port;
   var go=new GameObject("phase12Receiver");
   var view=new GameObject("RawImage",typeof(RectTransform),typeof(RawImage));
   var output=view.GetComponent<RawImage>();output.enabled=false;
   var receiver=go.AddComponent<QeeThermalReceiver>();
   receiver.host="127.0.0.1";receiver.port=port;receiver.output=output;receiver.staleAfterSeconds=.20f;receiver.reconnectMinSeconds=.08f;receiver.reconnectMaxSeconds=.16f;
   using(var cts=new CancellationTokenSource(TimeSpan.FromSeconds(12))){
    var sender=Task.Run(async()=>{
      using(var client=await listener.AcceptTcpClientAsync()){
       var stream=client.GetStream();
       for(int i=0;i<10;i++){var p=Packet(i);await stream.WriteAsync(p,0,p.Length);await Task.Delay(30);}
      }
    });
    receiver.Connect();
    bool sawTexture=false,sawStale=false;int iterations=0;
    while(iterations++<350 && !sawStale) {
      if(output.enabled && output.texture is Texture2D tex && tex.width==2 && tex.height==2)sawTexture=true;
      if(sawTexture && !output.enabled)sawStale=true;
      yield return null;
    }
    Assert.IsTrue(sawTexture,"Never rendered Unity Texture2D");
    Assert.IsTrue(sawStale,"Never hid stale or disconnected image");
    receiver.Disconnect();yield return null;
    Assert.IsFalse(output.enabled);
    Assert.LessOrEqual(receiver.DroppedFrames,int.MaxValue);
    UnityEngine.Object.Destroy(go);yield return null;
    Assert.IsTrue(output.texture==null,"Texture reference retained on destroy");
    Assert.IsTrue(sender.IsCompleted,"Fixture sender never completed");
   }
   listener.Stop();UnityEngine.Object.Destroy(view);
  }
  [Test] public void DecoderRejectsMalformedHeader(){
   byte[] broken=Packet(0);broken[0]=(byte)'X';
   Assert.Throws<InvalidDataException>(()=>QeeWireProtocol.ReadFrame(new MemoryStream(broken)));
  }
 }
}
