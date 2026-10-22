# Phase 13 alternative licensing and execution routes

Research date: 2026-10-22. Tested on this GitHub branch: source and shell configuration only; Unity Editor **NOT EXECUTED**.

## Decision matrix

1. Local Unity Editor with Unity Hub activated Personal license: no added software subscription; compatible with Unity Personal's supported activation pathway. Preferred first Unity native gate. Requires computer and supported Unity Editor/Android modules.
2. Local headless batchmode on that activated machine: same license. Script `tools/run-local-unity-playmode.sh` runs the Unity PlayMode test runner, saves log and XML, fails if tests are missing, skipped, failed or never recorded. **NOT EXECUTED**.
3. GameCI GitHub-hosted runner: existing Phase13 workflow supports a Unity Hub-generated `.ulf` license and credentials in GitHub Actions secrets; runner usage subject to platform quotas, especially for Android builds. License gate already tested; Editor job previously SKIPPED due empty secrets.
4. Self-hosted GitHub Actions runner: if already licensed machine available, invoke local script there; avoid adding unverified secrets and do not expose a runner to untrusted public pull requests. **NOT CONFIGURED**.
5. Legacy `.alf` manual-activation automation, Puppeteer website manipulation or developer-tools license-option workaround: excluded because Unity Personal manual activation is unsupported.

## Exact legitimate activation

Sign into Unity Hub, Settings / Licenses / Add / Get a free personal license (wording may vary with Hub release). Current Unity Hub docs say signing in can activate Personal automatically. Confirm license actually active on the execution machine. For GameCI GitHub-hosted activation, use supported Hub-created Unity license `.ulf` and current GameCI steps for the `UNITY_LICENSE`, `UNITY_EMAIL`, `UNITY_PASSWORD` secrets. Never commit secrets or Unity account data. Availability and portability of `.ulf` depend on Unity and GameCI's current rules.

## Local native execution

Install the Unity Editor version specified by `ProjectSettings/ProjectVersion.txt` and Unity Test Framework, with project opened once in Hub. Set `UNITY_EDITOR` to that installed binary. Run:

```bash
cd phase12
UNITY_EDITOR=/absolute/path/to/Unity ./tools/run-local-unity-playmode.sh
```

Expected outcome: a nonzero exit on missing Unity binary, failing NUnit tests, or missing/failed XML results. A PASS requires actual Unity Editor execution and PlayMode results. Inspect `phase13-evidence/unity-playmode.log` and `phase13-evidence/playmode-results.xml`. This command is not executed by GitHub Actions.

## Android build gate

Only after Unity import and PlayMode tests pass: install Android Build Support / SDK / NDK / OpenJDK on the same activated machine. In Editor, run **QUEST-EAGLEEYE / Configure Android ARM64 IL2CPP (No Build)**. Execute a real Android build through supported Unity BuildPipeline/Editor and capture success logs and APK hash. Do not infer Android compatibility from .NET desktop tests.

## Verified / outstanding

- .NET 8 Phase 11 compiled synthetic receiver: PASS (prior GitHub Actions run 39765420722).
- GitHub Unity credentials-presence gate: EXECUTED, no secrets, native steps SKIPPED (run 39766742911).
- Local Unity script: CREATED, NOT EXECUTED.
- Unity native test runner, Editor compile, Android ARM64 IL2CPP, Quest and TOPDON devices: NOT TESTED.

Author: Joseph Ribal Nasr. Publication qualification decision remains DEVELOP FURTHER. No main merge or Zenodo upload.
