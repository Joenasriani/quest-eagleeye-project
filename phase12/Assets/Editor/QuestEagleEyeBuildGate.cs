#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System;
namespace QuestEagleEye.Editor {
 public static class QuestEagleEyeBuildGate {
  [MenuItem("QUEST-EAGLEEYE/Configure Android ARM64 IL2CPP (No Build)")]
  public static void Configure() {
    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android,BuildTarget.Android);
    PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);
    PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
    PlayerSettings.SetApiCompatibilityLevel(NamedBuildTarget.Android,ApiCompatibilityLevel.NET_Standard);
    Debug.Log("Configured Android IL2CPP ARM64; no APK built, no device tested.");
  }
  public static void CheckConfiguration() {
    if(EditorUserBuildSettings.activeBuildTarget!=BuildTarget.Android)throw new Exception("Android target not selected");
    if(PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android)!=ScriptingImplementation.IL2CPP)throw new Exception("IL2CPP not set");
    if((PlayerSettings.Android.targetArchitectures & AndroidArchitecture.ARM64)==0)throw new Exception("ARM64 absent");
    Debug.Log("QEE_ANDROID_CONFIG_CHECK_PASS (not an APK build)");
  }
 }
}
#endif
