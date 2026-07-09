#!/usr/bin/env python3
"""Stage UX reference mockups under Assets/ArtSource/ux/ (design targets, not Resources)."""

from __future__ import annotations

import json
import shutil
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
WF = ROOT / "Assets/ArtSource/wireframes"
UX = ROOT / "Assets/ArtSource/ux"

# wireframe → ux filename (hi-fi targets; wireframes used as placeholders until replaced)
MAPPING = {
    "wireframe_main_scene_keyhouse.jpg": "ux_s0_splash.jpg",
    "wireframe_story_strip.jpg": "ux_s1_story_reel.jpg",
    "wireframe_chapter_map.jpg": "ux_s2_chapter_map.jpg",
    "wireframe_foyer_detailed.jpg": "ux_s3_foyer_hud.jpg",
    "wireframe_key_discovery.jpg": "ux_s4_key_discovery.jpg",
    "wireframe_lock_3state.jpg": "ux_s5_lock_puzzle.jpg",
    "wireframe_key_ring.jpg": "ux_s6_key_ring.jpg",
}

# Optional hi-fi asset UUID → filename (drop files here when downloaded)
UUID_MAP = {
    "b5ed4fa4": "ux_s0_splash.jpg",
    "bd7aee8a": "ux_s1_story_reel.jpg",
    "96927911": "ux_s2_chapter_map.jpg",
    "68d49b26": "ux_s3_foyer_hud.jpg",
    "03e3ef1b": "ux_s4_key_discovery.jpg",
    "a2728751": "ux_s5_lock_puzzle.jpg",
    "691aff9d": "ux_s6_key_ring.jpg",
    "ee6c462b": "ux_design_system_board.jpg",
    "5fc5a0fc": "ux_landscape_device_frame.jpg",
}


def jpg_meta(guid: str) -> str:
    return f"""fileFormatVersion: 2
guid: {guid}
DefaultImporter:
  externalObjects: {{}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""


def folder_meta(guid: str) -> str:
    return f"""fileFormatVersion: 2
guid: {guid}
folderAsset: yes
DefaultImporter:
  externalObjects: {{}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""


GUIDS = {
    "Assets/ArtSource/ux": "c0000000000000200000000000000020",
    "ux_s0_splash.jpg": "c0000000000000210000000000000021",
    "ux_s1_story_reel.jpg": "c0000000000000220000000000000022",
    "ux_s2_chapter_map.jpg": "c0000000000000230000000000000023",
    "ux_s3_foyer_hud.jpg": "c0000000000000240000000000000024",
    "ux_s4_key_discovery.jpg": "c0000000000000250000000000000025",
    "ux_s5_lock_puzzle.jpg": "c0000000000000260000000000000026",
    "ux_s6_key_ring.jpg": "c0000000000000270000000000000027",
    "ux_design_system_board.jpg": "c0000000000000280000000000000028",
    "ux_landscape_device_frame.jpg": "c0000000000000290000000000000029",
}


def main() -> None:
    UX.mkdir(parents=True, exist_ok=True)
    (UX / ".meta").write_text(folder_meta(GUIDS["Assets/ArtSource/ux"]))

    installed = {}
    extras = [
        ("wireframe_chapter_map.jpg", "ux_design_system_board.jpg"),
        ("wireframe_foyer_detailed.jpg", "ux_landscape_device_frame.jpg"),
    ]

    for src_name, dest_name in list(MAPPING.items()) + extras:
        src = WF / src_name
        dest = UX / dest_name
        if not src.exists():
            print(f"skip missing wireframe: {src_name}")
            continue
        shutil.copy2(src, dest)
        guid = GUIDS.get(dest_name, "c00000000000002a000000000000002a")
        (Path(str(dest) + ".meta")).write_text(jpg_meta(guid))
        installed[dest_name] = src_name
        print(f"ux/{dest_name} <= wireframes/{src_name}")

    manifest = {
        "note": "Reference only — implement UI in code per LockeKeyUITheme / LockeUIComponents",
        "uuid_rename_map": UUID_MAP,
        "wireframe_placeholders": installed,
        "screens": {
            "S0": "Assets/ArtSource/ux/ux_s0_splash.jpg",
            "S1": "Assets/ArtSource/ux/ux_s1_story_reel.jpg",
            "S2": "Assets/ArtSource/ux/ux_s2_chapter_map.jpg",
            "S3": "Assets/ArtSource/ux/ux_s3_foyer_hud.jpg",
            "S4": "Assets/ArtSource/ux/ux_s4_key_discovery.jpg",
            "S5": "Assets/ArtSource/ux/ux_s5_lock_puzzle.jpg",
            "S6": "Assets/ArtSource/ux/ux_s6_key_ring.jpg",
        },
    }
    out = ROOT / "tools/ux_reference_manifest.json"
    out.write_text(json.dumps(manifest, indent=2) + "\n")
    print(f"Wrote {out.relative_to(ROOT)}")


if __name__ == "__main__":
    main()