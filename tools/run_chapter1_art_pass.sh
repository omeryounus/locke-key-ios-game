#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"

echo "==> Importing base sprites"
python3 "${ROOT}/tools/import_art_assets.py"

echo "==> Importing UI/VFX/memory/zone backgrounds"
python3 "${ROOT}/tools/import_ui_vfx_assets.py"

echo "==> Importing sprite sheets + room layers"
python3 "${ROOT}/tools/import_sprite_sheets.py"

echo "==> Patching scene sprites"
python3 "${ROOT}/tools/patch_chapter1_sprites.py"

echo "Chapter 1 art replacement pass complete."