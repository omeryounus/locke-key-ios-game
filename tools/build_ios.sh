#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
UNITY_VERSION="${UNITY_VERSION:-6000.5.2f1}"

if [[ "$(uname -s)" == "Darwin" ]]; then
  UNITY_PATH="${UNITY_PATH:-/Applications/Unity/Hub/Editor/${UNITY_VERSION}/Unity.app/Contents/MacOS/Unity}"
else
  UNITY_PATH="${UNITY_PATH:-}"
fi

if [[ -z "${UNITY_PATH}" || ! -x "${UNITY_PATH}" ]]; then
  echo "Unity editor not found."
  echo "Set UNITY_PATH to your Unity executable, e.g.:"
  echo "  export UNITY_PATH=\"/Applications/Unity/Hub/Editor/${UNITY_VERSION}/Unity.app/Contents/MacOS/Unity\""
  echo "Then rerun: tools/build_ios.sh"
  exit 1
fi

mkdir -p "${ROOT}/Builds/iOS"

"${UNITY_PATH}" \
  -batchmode \
  -nographics \
  -quit \
  -projectPath "${ROOT}" \
  -executeMethod IOSBuildMenu.BuildIOS \
  -logFile "${ROOT}/Builds/iOS/unity-build.log"

echo "iOS Xcode project generated at ${ROOT}/Builds/iOS"
echo "Open the .xcodeproj on macOS and run on device or simulator."