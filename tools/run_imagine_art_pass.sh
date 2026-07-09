#!/usr/bin/env bash
# Regenerate all Chapter 1 art from Grok Imagine sources in tools/art_sources/.
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"

echo "==> Installing Imagine JPEG sources (if mapping present)"
if [[ -f "${ROOT}/tools/install_imagine_sources.py" ]]; then
  python3 "${ROOT}/tools/install_imagine_sources.py" || true
fi

echo "==> Running Chapter 1 art import pipeline"
bash "${ROOT}/tools/run_chapter1_art_pass.sh"
python3 "${ROOT}/tools/import_extended_art.py"

echo "Imagine art pass complete."