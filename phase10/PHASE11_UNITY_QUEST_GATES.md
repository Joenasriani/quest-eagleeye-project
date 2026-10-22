# QUEST-EAGLEEYE Phase 11: Unity and Quest validation gates

Author: Joseph Ribal Nasr. Research status: proposed XR engineering prototype, synthetic stream only.

## Confirmed GitHub Actions desktop evidence

- Commit before gate documentation: `2d1299b3267600451572bae94ef4725a3300609d`
- GitHub Actions run: https://github.com/Joenasriani/quest-eagleeye-project/actions/runs/39765276650
- .NET 8 compiled actual `QeeWireProtocol.cs` and `QeeThermalReceiver.cs` with desktop Unity substitutes, 0 warnings, 0 errors.
- 8 compiled executable integration cases passed: valid frame, invalid magic, bad SHA-256, truncated frame, oversized envelope, TCP 8-frame delivery, basic receiver lifecycle, two-session reconnect plus cancellation and bounded-queue stress.
- Reconnect stress evidence: both synthetic pixel identities rendered; observed maximum queued frames 2; 449 frame drops; output hidden on staleness/disconnect; texture released on destroy. The synthetic stream and desktop substitutes do **not** verify Unity or headset rendering.

## Gate U1: Unity Editor import and C# compilation (NOT TESTED)

1. Select a documented compatible Unity LTS Editor and record exact version, packages, scripting backend, target platforms and operating system.
2. Import the actual receiver/decoder into an isolated Unity project. Use UnityEngine/UnityEngine.UI, not the desktop test substitutes.
3. Compile in the Editor; collect full Console messages and any assembly definition conflicts. Mandatory pass: no compiler errors, no unexplained warnings.
4. Keep the project's synthetic data warning visible. Do not imply radiometric sensing.

## Gate U2: Unity PlayMode data flow (NOT TESTED)

1. Create and save a minimal scene with canvas, RawImage, Text status, `QeeThermalReceiver`, camera and event system as needed.
2. Run the synthetic stream from a separately controlled host. Record source and Unity process timestamps without treating unsynchronized clocks as latency evidence.
3. Verify texture dimensions, actual GPU-backed image updates, stall indicator, unexpected stream closure, frame ordering, reconnect and memory cleanup through Editor logs and memory profiler.
4. Automate PlayMode tests and capture actual executed test results. Desktop stubs cannot substitute for Unity object lifecycle.

## Gate Q1: Quest Android build (NOT TESTED)

1. Record Unity, Android SDK/NDK/JDK, OpenXR/Meta XR package versions and deployment configuration.
2. Build an Android ARM64 APK with the intended scripting backend (normally IL2CPP for platform acceptance); collect build logs.
3. Confirm Android `INTERNET` permission, network security policy and headset-to-host routing; no public inbound exposure required.
4. Passing compilation on desktop is not proof of successful Android or IL2CPP compilation.

## Gate Q2: Physical Quest 3 execution (NOT TESTED)

1. Obtain author-approved hardware, sideload the build, record headset OS/runtime version and device logs.
2. Use *synthetic* stream only initially. Verify frame updates, connection loss/recovery, panel geometry, UI safety/comfort and sustained rendering without claiming thermal registration.
3. Capture device traces, resource usage and failure conditions; distinguish display of 2D panel imagery from registered 3D overlay.
4. Actual TOPDON TC001 capture and Meshtastic/GNSS integration are separate later hardware gates and remain NOT TESTED.

## Research and publication boundary

No military/operational equivalence is demonstrated. The synthetic receiver cannot measure Quest latency, registration precision, thermal radiometry or hardware feasibility. Final publication audit is deferred. This test branch is not a main-branch merge or a Zenodo deposit.
