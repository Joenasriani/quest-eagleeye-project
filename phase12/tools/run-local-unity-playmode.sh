#!/usr/bin/env bash
# Runs ONLY against a previously Unity-Hub-activated, locally installed Unity Editor.
# No license generation, bypass, automatic secrets upload, or external publication.
set -Eeuo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
UNITY_EDITOR="${UNITY_EDITOR:-}"
if [[ -z "$UNITY_EDITOR" || ! -x "$UNITY_EDITOR" ]]; then
  echo "BLOCKED: Set UNITY_EDITOR to an executable Unity Editor binary activated via Unity Hub." >&2
  exit 3
fi
mkdir -p "$ROOT/phase13-evidence"
# Prevent a stale XML from a previous run from being accepted.
rm -f "$ROOT/phase13-evidence/playmode-results.xml"
set +e
"$UNITY_EDITOR" -batchmode -nographics -projectPath "$ROOT" -runTests -testPlatform PlayMode -testResults "$ROOT/phase13-evidence/playmode-results.xml" -logFile "$ROOT/phase13-evidence/unity-playmode.log" -quit
status=$?
set -e
if [[ $status -ne 0 ]]; then
  echo "FAILED: Unity exited with $status. Examine phase13-evidence/unity-playmode.log." >&2
  exit "$status"
fi
if [[ ! -s "$ROOT/phase13-evidence/playmode-results.xml" ]]; then
  echo "FAILED: Unity returned success but no nonempty PlayMode results XML exists." >&2
  exit 4
fi
python3 - "$ROOT/phase13-evidence/playmode-results.xml" <<'PY'
import sys,xml.etree.ElementTree as ET
root=ET.parse(sys.argv[1]).getroot()
tests=int(root.attrib.get('total',root.attrib.get('testcasecount','0')))
failures=int(root.attrib.get('failed','0'))
skips=int(root.attrib.get('skipped','0'))
passed=int(root.attrib.get('passed','0'))
if tests<2 or failures or passed<2 or skips:
 raise SystemExit(f"FAILED: total={tests} passed={passed} failed={failures} skipped={skips}")
print(f"UNITY_NATIVE_PLAYMODE_PASS total={tests} passed={passed}")
PY
