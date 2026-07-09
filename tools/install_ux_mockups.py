#!/usr/bin/env python3
"""Install hi-fi UX reference mockups into Assets/ArtSource/ux/."""

from __future__ import annotations

import json
import shutil
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
GROK = ROOT / "Assets/GrokWireframes"
WF = ROOT / "Assets/ArtSource/wireframes"
UX = ROOT / "Assets/ArtSource/ux"

# Full Grok export filename → canonical UX reference name
GROK_UX_FILES = {
    "grok-b5ed4fa4-968c-417e-bc5a-b477d5e17be7.jpg": "ux_s0_splash.jpg",
    "grok-bd7aee8a-5f73-4dd5-9769-bbd8281ee63b.jpg": "ux_s1_story_reel.jpg",
    "grok-96927911-50bd-4af0-9235-7920042867d3.jpg": "ux_s2_chapter_map.jpg",
    "grok-68d49b26-b5c0-477f-820d-a0198138db2e.jpg": "ux_s3_foyer_hud.jpg",
    "grok-03e3ef1b-0ce3-49d2-9b53-1ed32dd18356.jpg": "ux_s4_key_discovery.jpg",
    "grok-a2728751-dab6-4218-847e-37abb2cf38b9.jpg": "ux_s5_lock_puzzle.jpg",
    "grok-691aff9d-e6d1-45c7-a322-bfe6c5b8c9b8.jpg": "ux_s6_key_ring.jpg",
    "grok-ee6c462b-7745-43e6-aff9-c00a5f62fa85.jpg": "ux_design_system_board.jpg",
    "grok-5fc5a0fc-9abc-453e-b87a-724bb3f2bc2d.jpg": "ux_landscape_device_frame.jpg",
}

# Short UUID prefix → filename (for manifest / manual drops)
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

# Wireframe fallback if Grok hi-fi missing
WIREFRAME_FALLBACK = {
    "wireframe_main_scene_keyhouse.jpg": "ux_s0_splash.jpg",
    "wireframe_story_strip.jpg": "ux_s1_story_reel.jpg",
    "wireframe_chapter_map.jpg": "ux_s2_chapter_map.jpg",
    "wireframe_foyer_detailed.jpg": "ux_s3_foyer_hud.jpg",
    "wireframe_key_discovery.jpg": "ux_s4_key_discovery.jpg",
    "wireframe_lock_3state.jpg": "ux_s5_lock_puzzle.jpg",
    "wireframe_key_ring.jpg": "ux_s6_key_ring.jpg",
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


def install_file(src: Path, dest_name: str, installed: dict[str, str]) -> bool:
    dest = UX / dest_name
    shutil.copy2(src, dest)
    (Path(str(dest) + ".meta")).write_text(jpg_meta(GUIDS[dest_name]))
    installed[dest_name] = str(src.relative_to(ROOT))
    print(f"ux/{dest_name} <= {src.relative_to(ROOT)}")
    return True


def main() -> None:
    UX.mkdir(parents=True, exist_ok=True)
    (UX / ".meta").write_text(folder_meta(GUIDS["Assets/ArtSource/ux"]))

    installed: dict[str, str] = {}
    dests_done: set[str] = set()

    for grok_name, dest_name in GROK_UX_FILES.items():
        src = GROK / grok_name
        if not src.exists():
            print(f"skip missing grok: {grok_name}")
            continue
        install_file(src, dest_name, installed)
        dests_done.add(dest_name)

    for wf_name, dest_name in WIREFRAME_FALLBACK.items():
        if dest_name in dests_done:
            continue
        src = WF / wf_name
        if not src.exists():
            print(f"skip missing wireframe fallback: {wf_name}")
            continue
        install_file(src, dest_name, installed)
        dests_done.add(dest_name)

    missing = set(GUIDS) - {"Assets/ArtSource/ux"} - dests_done
    if missing:
        print("warning: missing UX files:", ", ".join(sorted(missing)))

    manifest = {
        "note": "Reference only — implement UI in code per LockeKeyUITheme / LockeUIComponents",
        "source": "Assets/GrokWireframes/grok-<uuid>.jpg",
        "uuid_rename_map": UUID_MAP,
        "grok_files": GROK_UX_FILES,
        "installed": installed,
        "screens": {
            "S0": "Assets/ArtSource/ux/ux_s0_splash.jpg",
            "S1": "Assets/ArtSource/ux/ux_s1_story_reel.jpg",
            "S2": "Assets/ArtSource/ux/ux_s2_chapter_map.jpg",
            "S3": "Assets/ArtSource/ux/ux_s3_foyer_hud.jpg",
            "S4": "Assets/ArtSource/ux/ux_s4_key_discovery.jpg",
            "S5": "Assets/ArtSource/ux/ux_s5_lock_puzzle.jpg",
            "S6": "Assets/ArtSource/ux/ux_s6_key_ring.jpg",
            "kit": "Assets/ArtSource/ux/ux_design_system_board.jpg",
            "layout": "Assets/ArtSource/ux/ux_landscape_device_frame.jpg",
        },
    }
    out = ROOT / "tools/ux_reference_manifest.json"
    out.write_text(json.dumps(manifest, indent=2) + "\n")
    print(f"Wrote {out.relative_to(ROOT)}")


if __name__ == "__main__":
    main()