#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace QuestEagleEye.Editor {
 public static class Phase12SceneCreator {
  [MenuItem("QUEST-EAGLEEYE/Create Synthetic Receiver Scene")]
  public static void CreateScene() {
    if(!AssetDatabase.IsValidFolder("Assets/Scenes"))AssetDatabase.CreateFolder("Assets","Scenes");
    var scene=EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects,NewSceneMode.Single);
    var go=new GameObject("QUEST-EAGLEEYE Synthetic Receiver");
    go.AddComponent<Phase12SceneBootstrap>();
    EditorSceneManager.MarkSceneDirty(scene);
    if(!EditorSceneManager.SaveScene(scene,"Assets/Scenes/QeeSyntheticReceiver.unity"))throw new System.Exception("Could not save Unity scene");
    UnityEngine.Debug.Log("Created scene. Source is synthetic only. Configure port then press Play.");
  }
 }
}
#endif
