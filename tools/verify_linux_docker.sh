#!/usr/bin/env bash
set -euo pipefail

# Optional: run Unity project verification in a GameCI container on Linux.
# Requires Docker, a Unity license, and network access to pull the image.

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
UNITY_VERSION="${UNITY_VERSION:-6000.5.2f1}"
IMAGE="${UNITY_DOCKER_IMAGE:-unityci/editor:${UNITY_VERSION}-linux-il2cpp-3}"

if ! command -v docker >/dev/null 2>&1; then
  echo "Docker is not installed."
  exit 1
fi

python3 "${ROOT}/tools/verify_static.py"

mkdir -p "${ROOT}/Builds"
LOG="${ROOT}/Builds/verify-docker.log"

echo "==> Pulling ${IMAGE} (first run may take a while)"
docker pull "${IMAGE}"

echo "==> Unity project verification (Docker)"
docker run --rm \
  -u "$(id -u):$(id -g)" \
  -e HOME=/tmp \
  -v "${ROOT}:/project" \
  -w /project \
  "${IMAGE}" \
  /opt/unity/Editor/Unity \
    -batchmode \
    -nographics \
    -quit \
    -projectPath /project \
    -executeMethod ProjectVerifyMenu.VerifyProject \
    -logFile /project/Builds/verify-docker.log

echo "Docker Unity verification passed."
echo "Log: ${LOG}"