#!/usr/bin/env bash
set -euo pipefail

# Chapter 1 device verification checklist for iOS slice stabilization.
# Run on macOS with Unity + Xcode for full coverage; static/compile steps work on Linux.

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
MODE="${1:-checklist}"
UNITY_VERSION="${UNITY_VERSION:-6000.5.2f1}"
FPS_TARGET="${FPS_TARGET:-60}"

pass() { echo "[PASS] $1"; }
fail() { echo "[FAIL] $1"; FAILURES=$((FAILURES + 1)); }
warn() { echo "[WARN] $1"; }

FAILURES=0

print_checklist() {
  cat <<'EOF'
Chapter 1 Device Verification Checklist
======================================
[ ] Unity compile (batchmode ProjectVerifyMenu.VerifyProject)
[ ] Chapter 1 Play Mode smoke test
    - Arrival: move tutorial unlocks controls
    - House key pickup + stuck door unlock
    - Bookshelf pushes reveal Ghost Key
    - Ghost Key phases through sealed door only after crossing passage
    - Echo: hide arch breaks pursuit; escape zone clears encounter
    - Save reload restores keys, beat, puzzles, and player position
[ ] iOS build (ProjectVerifyMenu.VerifyIOSBuild or tools/build_ios.sh)
[ ] TestFlight / physical device run
[ ] 60 FPS target check (Gameplay HUD + Echo encounter segment)
EOF
}

run_static() {
  echo "==> Static verification"
  python3 "${ROOT}/tools/verify_static.py"
  pass "Static asset/meta checks"
}

run_unity_compile() {
  echo "==> Unity compile verification"
  if [[ -x "${ROOT}/tools/verify_linux.sh" ]]; then
    if "${ROOT}/tools/verify_linux.sh" unity; then
      pass "Unity compile/import"
      return
    fi
  fi

  if [[ -x "${ROOT}/tools/verify_unity.sh" ]]; then
    if "${ROOT}/tools/verify_unity.sh"; then
      pass "Unity compile/import"
      return
    fi
  fi

  warn "Unity editor not available — run ProjectVerifyMenu.VerifyProject on a Unity machine"
}

run_smoke_notes() {
  echo "==> Play Mode smoke test (manual)"
  cat <<'EOF'
In Unity Play Mode with Chapter1 scene:
1. Enter Play Mode from Assets/_Project/Scenes/Chapter1/Chapter1.unity
2. Confirm arrival toast and delayed control reveal after first move
3. Collect house key, unlock stuck door (audio + haptic feedback)
4. Push bookshelf 3x, collect Ghost Key, use key at sealed door
5. Cross passage trigger before ghost phase ends — door stays solved
6. Hide behind arch during Echo; reach PassageZone to complete beat
7. Stop Play Mode, re-enter — save should restore progress
EOF
  warn "Play Mode smoke test requires manual confirmation in Unity Editor"
}

run_ios_build_notes() {
  echo "==> iOS build verification"
  if [[ "$(uname -s)" == "Darwin" ]] && [[ -x "${ROOT}/tools/build_ios.sh" ]]; then
    if "${ROOT}/tools/build_ios.sh"; then
      pass "iOS Xcode project build"
      return
    fi
    fail "iOS build script failed"
    return
  fi

  warn "iOS build requires macOS + Xcode. Run tools/build_ios.sh or ProjectVerifyMenu.VerifyIOSBuild"
}

run_fps_notes() {
  echo "==> FPS target (${FPS_TARGET})"
  cat <<EOF
On device, profile Chapter 1 Echo encounter (worst case):
- Target: ${FPS_TARGET} FPS sustained
- Unity Profiler: CPU/GPU < 16.6ms frame budget
- Disable deep profiler for realistic numbers
EOF
  warn "FPS check is manual on TestFlight/device build"
}

case "${MODE}" in
  checklist)
    print_checklist
    ;;
  static)
    run_static
    ;;
  compile)
    run_static
    run_unity_compile
    ;;
  smoke)
    run_smoke_notes
    ;;
  ios)
    run_ios_build_notes
    ;;
  fps)
    run_fps_notes
    ;;
  all)
    print_checklist
    echo
    run_static || true
    run_unity_compile || true
    run_smoke_notes
    run_ios_build_notes
    run_fps_notes
    ;;
  *)
    echo "Usage: tools/chapter1_device_verify.sh [checklist|static|compile|smoke|ios|fps|all]"
    exit 2
    ;;
esac

if [[ "${FAILURES}" -gt 0 ]]; then
  echo
  echo "${FAILURES} verification step(s) failed."
  exit 1
fi

echo
echo "Chapter 1 device verification script completed (${MODE})."