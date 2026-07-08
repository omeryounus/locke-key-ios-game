#!/usr/bin/env bash
set -euo pipefail

# Linux-friendly verification. Catches most asset/meta issues without macOS.
# For script compile + import, install Unity Linux editor or use Docker (see README).

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
UNITY_VERSION="${UNITY_VERSION:-6000.5.2f1}"
MODE="${1:-static}"

echo "==> Static checks (Linux)"
python3 "${ROOT}/tools/verify_static.py"

if [[ "${MODE}" == "static" ]]; then
  echo
  echo "Linux static verification passed."
  echo "Note: iOS builds still require macOS. For compile/import checks, run:"
  echo "  tools/verify_linux.sh unity"
  exit 0
fi

if [[ "${MODE}" != "unity" ]]; then
  echo "Usage: tools/verify_linux.sh [static|unity]"
  exit 2
fi

UNITY_PATH="${UNITY_PATH:-}"
if [[ -z "${UNITY_PATH}" ]]; then
  for candidate in \
    "${HOME}/Unity/Hub/Editor/${UNITY_VERSION}/Editor/Unity" \
    "${HOME}/.local/share/Unity/Hub/Editor/${UNITY_VERSION}/Editor/Unity" \
    "/opt/unity/Editor/Unity"; do
    if [[ -x "${candidate}" ]]; then
      UNITY_PATH="${candidate}"
      break
    fi
  done
fi

if [[ -z "${UNITY_PATH}" || ! -x "${UNITY_PATH}" ]]; then
  echo
  echo "Unity Linux editor not found."
  echo "Options:"
  echo "  1) Install Unity 6000.5.2f1 Linux editor via Unity Hub, then rerun:"
  echo "     tools/verify_linux.sh unity"
  echo "  2) Use GameCI Docker (requires Docker + Unity license):"
  echo "     tools/verify_linux.sh docker"
  echo "  3) Static-only checks (already passed):"
  echo "     tools/verify_linux.sh static"
  exit 1
fi

mkdir -p "${ROOT}/Builds"
LOG="${ROOT}/Builds/verify-linux.log"

echo "==> Unity project verification (Linux editor)"
"${UNITY_PATH}" \
  -batchmode \
  -nographics \
  -quit \
  -projectPath "${ROOT}" \
  -executeMethod ProjectVerifyMenu.VerifyProject \
  -logFile "${LOG}"

echo "Linux Unity verification passed."
echo "Log: ${LOG}"