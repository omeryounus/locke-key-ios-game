#!/usr/bin/env python3
"""Wire imported sprite GUIDs into Chapter1 scene SpriteRenderer fields."""

import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SCENE = ROOT / "Assets/_Project/Scenes/Chapter1/Chapter1.unity"
MAP = ROOT / "tools/sprite_guid_map.json"

def sprite_line(guid: str) -> str:
    return f"m_Sprite: {{fileID: 21300000, guid: {guid}, type: 3}}"


BINDINGS = {
    "400015": "player",          # Player SpriteRenderer
    "910002": "house_key",       # HouseKeyPickup
    "920004": "door_wood",       # StuckDoor
    "930002": "ghost_key",       # GhostKeyPickup
    "500004": "door_sealed",     # SealedDoor
    "600003": "ground_tile",     # Ground
    "800003": "wall_panel",      # FoyerWall
    "940004": "bookshelf_collapsed",
    "950002": "head_key",
    "960002": "portrait",
}


def main() -> None:
    guids = json.loads(MAP.read_text())
    text = SCENE.read_text()

    for file_id, key in BINDINGS.items():
        guid = guids[key]
        pattern = (
            rf"(--- !u!212 &{file_id}\nSpriteRenderer:.*?)"
            rf"m_Sprite: \{{fileID: \d+, guid: [0-9a-f]+, type: \d+\}}"
        )
        replacement = rf"\1{sprite_line(guid)}"
        text, count = re.subn(pattern, replacement, text, count=1, flags=re.DOTALL)
        if count != 1:
            raise RuntimeError(f"Failed to patch SpriteRenderer &{file_id} ({key})")

    # EchoEncounterManager echoSprite reference
    echo_guid = guids["echo"]
    text = text.replace(
        "  echoSprite: {fileID: 0}",
        f"  echoSprite: {{fileID: 21300000, guid: {echo_guid}, type: 3}}",
    )

    SCENE.write_text(text)
    print(f"Patched {len(BINDINGS)} sprites + Echo reference in Chapter1.unity")


if __name__ == "__main__":
    main()