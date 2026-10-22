using System;
namespace UnityEngine {
 [AttributeUsage(AttributeTargets.Field)] public sealed class HeaderAttribute:Attribute { public HeaderAttribute(string x) {} }
 public class Object { public static int DestroyCount; public static void Destroy(Object x) { if (x != null) DestroyCount++; } }
 public class MonoBehaviour : Object {}
 public class Texture : Object {}
 public enum TextureFormat { RGBA32 } public enum FilterMode { Point }
 public struct Color32 { public byte r,g,b,a; public Color32(byte r,byte g,byte b,byte a) {this.r=r;this.g=g;this.b=b;this.a=a;} }
 public class Texture2D : Texture { public int width,height; public FilterMode filterMode; public Texture2D(int w,int h,TextureFormat f,bool mip,bool linear) {width=w;height=h;} public Color32[] lastPixels; public int applyCount; public void SetPixels32(Color32[] colors) { lastPixels=(Color32[])colors.Clone(); } public void Apply(bool a,bool b) { applyCount++; } }
 public static class Debug {}
 public static class JsonUtility { public static T FromJson<T>(string json) => System.Text.Json.JsonSerializer.Deserialize<T>(json, new System.Text.Json.JsonSerializerOptions { IncludeFields=true }); }
}
namespace UnityEngine.UI { public class RawImage { public UnityEngine.Texture texture; public bool enabled; } public class Text { public string text; } }