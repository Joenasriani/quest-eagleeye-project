# QUEST-EAGLEEYE: Reproducible Unity Hub Test Bundle

Classification: **Unity project / PlayMode validation candidate**, not an empirical headset result.
Phase 0 verdict: **DEVELOP FURTHER**. Author Joseph Ribal Nasr.

## Acquire

1. Download the archive for branch `test/qee-dotnet8-phase10` from GitHub (Code → Download ZIP), extract it locally.
2. Open the nested `phase12` folder in Unity Hub as a project (do not open the repository root).
3. The target in `ProjectSettings/ProjectVersion.txt` is Unity 2022.3.62f1, **not a Unity-verified version**. Install appropriate 2022.3 LTS Editor using Unity Hub, with Android modules only if attempting the Android gate.
4. Activate Unity Personal legitimately through Unity Hub, subject to eligibility. Do not use legacy manual activation or commit license files.

## Execute native tests

- In Unity: Window → General → Test Runner → PlayMode → Run All.
- On a local Unix shell with an already activated Unity executable:
  `cd phase12 && UNITY_EDITOR=/path/to/Unity bash tools/run-local-unity-playmode.sh`.
- Logs: `phase12/phase13-evidence/unity-playmode.log`.
- Results: `phase12/phase13-evidence/playmode-results.xml`.
- Each run deletes the prior XML first. The script requires actual Unity exit success, a nonempty valid NUnit-style XML report, at least two successful tests, and zero failures/skips.
- If running interactively, record full Unity Console output, Test Runner results, Editor version and `Packages/packages-lock.json`. Do not mark PASS from the shell script's existence alone.

## Audit findings / corrections

1. **FIXED IN SOURCE**: PlayMode receiver poll was 350 frames, unsuitable as a wall-clock timeout across Editor speeds. Now uses `Time.realtimeSinceStartup` with a 12-second deadline.
2. **FIXED IN SOURCE**: Fixture listener and scene objects were not guaranteed cleanup when a test fails. Now guarded with `finally` and an explicit sender-completion deadline.
3. **FIXED IN SCRIPT**: A stale `playmode-results.xml` could produce a false pass after a tool returned success without writing a new report. Script deletes old XML before launch.
4. **OPEN**: Native Unity compilation and actual Unity test execution not yet performed; manual changes or further fixes may be necessary.
5. **OPEN**: One-session Unity PlayMode test does not prove full native reconnect cycle or sustained load. Synthetic C# desktop stress tests are separate evidence.
6. **OPEN**: Unity scene creation and Android IL2CPP BuildPipeline are not executed.

## Android verification gate

After successful Unity import and PlayMode test execution, configure **QUEST-EAGLEEYE → Configure Android ARM64 IL2CPP (No Build)**. A real APK build requires Android Build Support and a valid build scene. Record the Unity Editor build report and APK hash separately. Configuration source is not build evidence.

## Research integrity

No Unity Editor, Android APK, Quest 3 or TOPDON TC001 evidence is claimed here. Do not merge to main, publish or deposit to Zenodo.
