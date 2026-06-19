#!/usr/bin/env bash
#
# Verify that the Unity project imports and compiles with NO script errors,
# without opening the Editor GUI. Uses Unity batchmode.
#
# Usage:
#   UNITY="/path/to/Unity" ./Tools/verify_compile.sh
#
# Common Unity binary locations:
#   macOS  : /Applications/Unity/Hub/Editor/6000.0.32f1/Unity.app/Contents/MacOS/Unity
#   Windows: C:/Program Files/Unity/Hub/Editor/6000.0.32f1/Editor/Unity.exe
#   Linux  : ~/Unity/Hub/Editor/6000.0.32f1/Editor/Unity
#
# Exit codes: 0 = no compile errors, 1 = compile errors found, 2 = usage error.

set -euo pipefail

PROJECT="$(cd "$(dirname "$0")/.." && pwd)"
LOG="${LOG:-$PROJECT/Logs/compile.log}"
mkdir -p "$(dirname "$LOG")"

if [[ -z "${UNITY:-}" ]]; then
  echo "ERROR: set UNITY to your Unity editor binary, e.g.:"
  echo '  UNITY="/Applications/Unity/Hub/Editor/6000.0.32f1/Unity.app/Contents/MacOS/Unity" ./Tools/verify_compile.sh'
  exit 2
fi
if [[ ! -x "$UNITY" ]]; then
  echo "ERROR: UNITY ('$UNITY') is not an executable file."
  exit 2
fi

echo ">> Importing & compiling project (batchmode). This may take a few minutes..."
# -quit makes Unity exit after import; scripts are compiled during import.
"$UNITY" \
  -batchmode \
  -nographics \
  -quit \
  -projectPath "$PROJECT" \
  -logFile "$LOG" || true   # don't abort; we inspect the log ourselves

echo ">> Scanning log: $LOG"
if grep -nE "error CS[0-9]+" "$LOG"; then
  echo ""
  echo "❌ COMPILE ERRORS FOUND (see above / $LOG)"
  exit 1
fi

# Also surface any importer/assembly errors that aren't CS#### codes.
if grep -nE "Scripts have compiler errors|Compilation failed" "$LOG"; then
  echo ""
  echo "❌ COMPILATION FAILED (see $LOG)"
  exit 1
fi

echo "✅ No compile errors detected."
echo "   (Full log: $LOG)"
