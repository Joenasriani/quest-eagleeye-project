# QUEST-EAGLEEYE Phase 12 | Unity native validation candidate

Author: Joseph Ribal Nasr, Independent Researcher. Not tested on Quest 3, TOPDON TC001, Unity Editor or Android.

## Source provenance

The two code files `Assets/Scripts/QeeWireProtocol.cs` and `Assets/Scripts/QeeThermalReceiver.cs` were copied from `phase10/Assets/Scripts/` on the isolated branch after the verified .NET 8 Phase 11 GitHub Actions run #39765420722. Do not replace them with desktop Unity stubs in the Editor.

## Import / execution procedure

1. Install the selected Unity 2022.3 LTS Editor with Android Build Support, OpenJDK, SDK and NDK (if Android builds are planned). Version in ProjectSettings/ProjectVersion.txt is a project target, not an executed validation.
2. Open folder `phase12/` as a Unity project via Unity Hub. Allow Unity to resolve package dependencies. Record actual Editor version and import errors.
3. Select **QUEST-EAGLEEYE → Create Synthetic Receiver Scene** to generate and save `Assets/Scenes/QeeSyntheticReceiver.unity`; inspect the generated Canvas and status label. Do not claim scene creation succeeded until executed in the Editor.
4. Open **Window → General → Test Runner → PlayMode**, run `QeeReceiverPlayModeTests` against localhost. Capture full runner XML, Unity Editor logs, package versions and scene.
5. For Android settings choose **QUEST-EAGLEEYE → Configure Android ARM64 IL2CPP (No Build)**. Then execute `QuestEagleEye.Editor.QuestEagleEyeBuildGate.CheckConfiguration` in an Editor batchmode process or via script. Configuration alone is NOT an Android APK build.
6. Actually build using Unity Editor Android Build Support with IL2CPP. Save builder log, package-lock.json, device targets, APK SHA-256; do not label PASS without artifact and successful build.
7. Actual Quest 3 execution is separate. Displaying synthetic textures does not establish thermal sensor acquisition or real-world spatial registration.

## Gates and evidence status

- D1: .NET 8 compiled synthetic desktop receiver (Phase 11) PASS; independent of Unity.
- U1: Unity Editor compilation and package resolution NOT TESTED.
- U2: Unity-native PlayMode texture + stale/disconnect test NOT TESTED.
- U3: Generated, saved scene NOT TESTED.
- A1: Android ARM64/IL2CPP configuration NOT TESTED.
- A2: Android APK build NOT TESTED.
- Q1: Physical Quest 3 runtime NOT TESTED.
- T1: TOPDON TC001 acquisition NOT TESTED.

## Limitations

The PlayMode test is a candidate. The timeout uses a frame-count loop, whose wall-clock duration depends on Editor frame rate. It uses one TCP connection; full reconnect and long-lived concurrency stress remain covered by the compiled desktop test, NOT Unity PlayMode. No radiometric measurement, calibrated 3D thermal registration, field safety or operational performance is established.
