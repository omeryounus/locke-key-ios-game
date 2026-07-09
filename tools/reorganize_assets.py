#!/usr/bin/env python3
"""
Reorganize Grok wireframe assets into correct Unity Resources/Art subfolders.

Source: Assets/GrokWireframes/  (already renamed)
Destinations:
  Assets/_Project/Resources/Art/Keys/
  Assets/_Project/Resources/Art/Backgrounds/
  Assets/_Project/Resources/Art/Storyboard/
  Assets/_Project/Resources/Art/UI/
  Assets/ArtSource/wireframes/           (NOT in Resources)
"""

import shutil
import os
from pathlib import Path

ROOT = Path(__file__).parent.parent
SRC = ROOT / "Assets" / "GrokWireframes"

TARGETS = {
    ROOT / "Assets/_Project/Resources/Art/Keys": [
        "key_anywhere.jpg",
        "key_head.jpg",
        "key_mending.jpg",
        "key_omega.jpg",
        "key_ghost.jpg",
        "key_shadow.jpg",
        "key_echo.jpg",
        "key_matchstick.jpg",
        "key_mirror.jpg",
        "key_music_box.jpg",
        "key_animal.jpg",
        "key_identity.jpg",
        "key_alpha.jpg",
    ],
    ROOT / "Assets/_Project/Resources/Art/Backgrounds": [
        "bg_keyhouse_foyer_16x9.jpg",
        "bg_keyhouse_foyer_9x16.jpg",
        "bg_wellhouse_exterior.jpg",
        "bg_black_door_chamber.jpg",
    ],
    ROOT / "Assets/_Project/Resources/Art/Storyboard": [
        "story_01_arrival.jpg",
        "story_02_first_discovery.jpg",
        "story_03_wellhouse_echo.jpg",
        "story_04_black_door.jpg",
    ],
    ROOT / "Assets/_Project/Resources/Art/UI": [
        "ui_key_slot_empty.jpg",
        "ui_btn_primary.jpg",
        "ui_codex_panel.jpg",
    ],
    ROOT / "Assets/ArtSource/wireframes": [
        "wireframe_main_scene_keyhouse.jpg",
        "wireframe_key_discovery.jpg",
        "wireframe_story_strip.jpg",
        "wireframe_key_ring.jpg",
        "wireframe_foyer_detailed.jpg",
        "wireframe_lock_3state.jpg",
        "wireframe_flowchart.jpg",
        "wireframe_chapter_map.jpg",
        "wireframe_codex_discovery.jpg",
    ],
}

copied = []
missing = []

for dest_dir, files in TARGETS.items():
    dest_dir.mkdir(parents=True, exist_ok=True)
    for fname in files:
        src_file = SRC / fname
        dst_file = dest_dir / fname
        if src_file.exists():
            shutil.copy2(src_file, dst_file)
            copied.append(f"  {fname} → {dest_dir.relative_to(ROOT)}")
        else:
            missing.append(f"  MISSING: {fname}")

print(f"\n✅ Copied {len(copied)} assets:")
for line in copied:
    print(line)

if missing:
    print(f"\n⚠️  Missing {len(missing)} files:")
    for line in missing:
        print(line)
else:
    print("\n✅ All files present — no missing assets.")

print("\nDone. Open Unity to let it generate .meta files for the new paths.")
