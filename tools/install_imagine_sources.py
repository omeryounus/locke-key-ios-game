#!/usr/bin/env python3
"""Copy Grok Imagine session images into tools/art_sources/ for import."""

from __future__ import annotations

import json
import shutil
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SESSION_IMAGES = Path(
    "/home/agentv/.grok/sessions/%2Fhome%2Fagentv/"
    "019f4048-c7a0-7142-b679-11c8eb03f471/images"
)
OUT = ROOT / "tools" / "art_sources"

# Session image id -> art_sources filename (Imagine pass 2026-07-09)
MAPPING = {
    "28.jpg": "1.jpg",
    "30.jpg": "2.jpg",
    "27.jpg": "3.jpg",
    "29.jpg": "4.jpg",
    "34.jpg": "5.jpg",
    "33.jpg": "6.jpg",
    "35.jpg": "7.jpg",
    "32.jpg": "8.jpg",
    "31.jpg": "9.jpg",
    "37.jpg": "10.jpg",
    "36.jpg": "11.jpg",
    "41.jpg": "12.jpg",
    "38.jpg": "13.jpg",
    "40.jpg": "14.jpg",
    "39.jpg": "15.jpg",
    "42.jpg": "16.jpg",
    "44.jpg": "17_key_slots.jpg",
    "43.jpg": "18_vfx_ghost_phase.jpg",
    "45.jpg": "19_vfx_echo_fog.jpg",
    "46.jpg": "20_vfx_memory.jpg",
    "51.jpg": "21_memory_panel_1.jpg",
    "47.jpg": "22_memory_panel_2.jpg",
    "48.jpg": "23_memory_panel_3.jpg",
    "49.jpg": "24_foyer_bg_far.jpg",
    "50.jpg": "25_library_bg_far.jpg",
}


def main() -> None:
    if not SESSION_IMAGES.is_dir():
        raise FileNotFoundError(f"Session images not found: {SESSION_IMAGES}")

    OUT.mkdir(parents=True, exist_ok=True)
    installed: dict[str, str] = {}

    for src_name, dest_name in MAPPING.items():
        src = SESSION_IMAGES / src_name
        if not src.exists():
            raise FileNotFoundError(f"Missing generated image: {src}")
        dest = OUT / dest_name
        shutil.copy2(src, dest)
        installed[dest_name] = src_name
        print(f"Installed {dest_name} <= {src_name}")

    manifest = ROOT / "tools" / "imagine_install_map.json"
    manifest.write_text(
        json.dumps(
            {
                "session_images": str(SESSION_IMAGES),
                "installed_at": "2026-07-09",
                "mapping": installed,
                "style_reference": "26.jpg",
            },
            indent=2,
        )
        + "\n"
    )
    print(f"Wrote {manifest.relative_to(ROOT)}")


if __name__ == "__main__":
    main()