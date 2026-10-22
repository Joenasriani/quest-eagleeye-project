using UnityEngine;
using UnityEngine.UI;
namespace QuestEagleEye {
 public sealed class Phase12SceneBootstrap : MonoBehaviour {
  void Awake() {
   var canvas = new GameObject("Synthetic Thermal Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
   var c = canvas.GetComponent<Canvas>(); c.renderMode = RenderMode.ScreenSpaceOverlay;
   var panel = new GameObject("Thermal Panel", typeof(RectTransform), typeof(RawImage));
   panel.transform.SetParent(canvas.transform, false);
   var rect = panel.GetComponent<RectTransform>(); rect.sizeDelta = new Vector2(640,480);
   var label = new GameObject("Safety Label", typeof(RectTransform), typeof(Text));
   label.transform.SetParent(canvas.transform,false);
   var text=label.GetComponent<Text>();text.text="THERMAL_UNAVAILABLE - SYNTHETIC ONLY";text.color=Color.white;text.font=Resources.GetBuiltinResource<Font>("Arial.ttf");
   var receiver=gameObject.AddComponent<QeeThermalReceiver>();
   receiver.output=panel.GetComponent<RawImage>();receiver.statusText=text;
   receiver.connectOnStart=false;
  }
 }
}
