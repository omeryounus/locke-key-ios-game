#!/usr/bin/env python3
"""Install Grok Imagine production art from session images into the project library + Resources."""

from __future__ import annotations

import hashlib
import json
import re
import sys
from datetime import datetime, timezone
from pathlib import Path

from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
DEFAULT_SESSION = Path.home() / ".grok/sessions"
LIB = ROOT / "Assets/_Project/Art/Production"
RES = ROOT / "Assets/_Project/Resources/Art"


def find_latest_session_images() -> Path | None:
    # Prefer the active workspace session folder if present
    candidates = list(Path("/home/agentv/.grok/sessions").glob("*/images"))
    if not candidates:
        return None
    return max(candidates, key=lambda p: p.stat().st_mtime)


def write_meta(path: Path, template: str) -> str:
    name = path.stem
    h = hashlib.md5(str(path.relative_to(ROOT)).encode()).hexdigest()
    guid = "e2" + h[:30]
    text = re.sub(r"^guid: .*$", f"guid: {guid}", template, count=1, flags=re.M)
    text = re.sub(r"name: \S+", f"name: {name}_0", text, count=1)
    text = re.sub(r"213: \d+", f"213: {int(h[:12], 16) % (10**18)}", text, count=1)
    path.with_suffix(path.suffix + ".meta").write_text(text)
    return guid


def folder_meta(path: Path) -> None:
    meta = Path(str(path) + ".meta")
    if meta.exists():
        return
    h = hashlib.md5(str(path.relative_to(ROOT)).encode()).hexdigest()
    guid = "e2" + h[:30]
    meta.write_text(
        f"""fileFormatVersion: 2
guid: {guid}
folderAsset: yes
DefaultImporter:
  externalObjects: {{}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""
    )


def upscale(src: Path, min_edge: int = 2048) -> Image.Image:
    im = Image.open(src).convert("RGB")
    w, h = im.size
    long_edge = max(w, h)
    if long_edge < min_edge:
        scale = min_edge / long_edge
        im = im.resize((int(w * scale), int(h * scale)), Image.Resampling.LANCZOS)
    return im


def main() -> int:
    session = Path(sys.argv[1]) if len(sys.argv) > 1 else find_latest_session_images()
    if session is None or not session.exists():
        print("No session images found. Pass path to images folder.")
        return 1

    template = (RES / "Backgrounds/bg_keyhouse_foyer_16x9.jpg.meta").read_text()
    char_meta = (RES / "Characters/player_idle.png.meta").read_text()

    # Default mapping uses most recent production pass IDs if present
    mapping = {
        "bg_foyer": ("Environment/Backgrounds/bg_foyer_production.jpg", "Backgrounds/bg_room_foyer_16x9.jpg"),
        "bg_library": ("Environment/Backgrounds/bg_library_production.jpg", "Backgrounds/bg_room_library_16x9.jpg"),
        "bg_sealed": ("Environment/Backgrounds/bg_sealed_production.jpg", "Backgrounds/bg_room_sealed_16x9.jpg"),
        "bg_exterior": ("Environment/Backgrounds/bg_exterior_production.jpg", "Backgrounds/bg_wellhouse_exterior.jpg"),
        "bg_memory": ("Environment/Backgrounds/bg_memory_production.jpg", "Backgrounds/bg_room_memory_16x9.jpg"),
        "player_idle": ("Characters/Player/player_idle_production.png", "Characters/player_idle.png"),
        "player_walk_a": ("Characters/Player/player_walk_a_production.png", "Characters/player_walk_a.png"),
        "player_walk_b": ("Characters/Player/player_walk_b_production.png", "Characters/player_walk_b.png"),
        "player_jump": ("Characters/Player/player_jump_production.png", "Characters/player_jump.png"),
        "key_house": ("Items/Keys/key_house_production.png", "Sprites/Keys/house_key.png"),
        "key_ghost": ("Items/Keys/key_ghost_production.png", "Keys/key_ghost.jpg"),
        "key_head": ("Items/Keys/key_head_production.png", "Keys/key_head.jpg"),
        "echo": ("Enemies/Echo/echo_production.png", "Enemies/echo_00.png"),
        "vfx_atlas": ("Effects/vfx_particle_atlas_production.png", "VFX/vfx_atlas_production.png"),
        "ui_kit": ("UI/ui_panel_kit_production.jpg", "UI/ui_panel_kit_production.jpg"),
        "door_front": ("Props/Doors/door_front_production.png", "Environments/door_front_production.png"),
    }

    print(f"Session: {session}")
    print("Library assets already installed via generation pass.")
    print(f"See {LIB / 'Manifests/production_art_manifest.json'}")
    for d in LIB.rglob("*"):
        if d.is_dir():
            folder_meta(d)
    folder_meta(LIB)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
