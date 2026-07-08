#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
UNITY_VERSION="${UNITY_VERSION:-6000.5.2f1}"
MODE="${1:-project}"

if [[ "$(uname -s)" == "Darwin" ]]; then
  UNITY_PATH="${UNITY_PATH:-/Applications/Unity/Hub/Editor/${UNITY_VERSION}/Unity.app/Contents/MacOS/Unity}"
else
  UNITY_PATH="${UNITY_PATH:-}"
fi

echo "==> Static checks"
python3 "${ROOT}/tools/verify_static.py"

if [[ -z "${UNITY_PATH}" || ! -x "${UNITY_PATH}" ]]; then
  echo
  echo "Unity editor not found. Static checks passed, but Unity verification is required."
  echo "On macOS, run:"
  echo "  export UNITY_PATH=\"/Applications/Unity/Hub/Editor/${UNITY_VERSION}/Unity.app/Contents/MacOS/Unity\""
  echo "  tools/verify_unity.sh project   # compile + asset import"
  echo "  tools/verify_unity.sh ios       # full iOS Xcode project build"
  exit 1
fi

case "${MODE}" in
  project)
    METHOD="ProjectVerifyMenu.VerifyProject"
    LOG="${ROOT}/Builds/verify-project.log"
    ;;
  ios)
    METHOD="ProjectVerifyMenu.VerifyIOSBuild"
    LOG="${ROOT}/Builds/verify-ios.log"
    ;;
  *)
    echo "Usage: tools/verify_unity.sh [project|ios]"
    exit 2
    ;;
esac

mkdir -p "${ROOT}/Builds"

echo "==> Unity ${MODE} verification"
"${UNITY_PATH}" \
  -batchmode \
  -nographics \
  -quit \
  -projectPath "${ROOT}" \
  -executeMethod "${METHOD}" \
  -logFile "${LOG}"

echo "Unity ${MODE} verification passed."
echo "Log: ${LOG}"