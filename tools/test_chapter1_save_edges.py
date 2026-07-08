#!/usr/bin/env python3
"""Validate Chapter 1 save payloads at critical beat boundaries."""

from __future__ import annotations

import json
import tempfile
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def beat_snapshot(beat: int) -> dict:
    base = {
        "version": 2,
        "playerX": -2.0,
        "playerY": -1.0,
        "hasSavedPosition": True,
        "checkpointX": -2.0,
        "checkpointY": -1.0,
        "hasHouseKey": False,
        "hasGhostKey": False,
        "hasHeadKey": False,
        "ghostKeyRevealed": False,
        "activeKeyAbility": "",
        "currentBeat": beat,
        "currentRoom": 0,
        "chapterComplete": False,
        "echoEncounterCleared": False,
        "echoEncounterActive": False,
        "solvedPuzzleIds": [],
        "collectedPickupIds": [],
    }

    if beat >= 1:
        base.update(hasHouseKey=True, collectedPickupIds=["house_key"], currentBeat=1, checkpointX=-1.0)
    if beat >= 2:
        base.update(currentBeat=2, checkpointX=2.0, solvedPuzzleIds=["chapter1_stuck_door"], currentRoom=2)
    if beat >= 3:
        base.update(
            currentBeat=3,
            checkpointX=4.5,
            ghostKeyRevealed=True,
            solvedPuzzleIds=["chapter1_stuck_door", "chapter1_bookshelf"],
            currentRoom=2,
        )
    if beat == 31:
        base.update(
            currentBeat=3,
            checkpointX=4.5,
            hasGhostKey=True,
            ghostKeyRevealed=True,
            collectedPickupIds=["house_key", "ghost_key"],
            activeKeyAbility="GhostPhase",
            solvedPuzzleIds=["chapter1_stuck_door", "chapter1_bookshelf"],
        )
    if beat >= 4:
        base.update(
            currentBeat=4,
            checkpointX=5.2,
            hasGhostKey=True,
            collectedPickupIds=["house_key", "ghost_key"],
            activeKeyAbility="GhostPhase",
            solvedPuzzleIds=["chapter1_stuck_door", "chapter1_bookshelf", "chapter1_sealed_door"],
            echoEncounterActive=True,
            currentRoom=3,
        )
    if beat >= 5:
        base.update(
            currentBeat=5,
            checkpointX=8.5,
            echoEncounterActive=False,
            echoEncounterCleared=True,
            chapterComplete=True,
            hasHeadKey=True,
            collectedPickupIds=["house_key", "ghost_key", "head_key"],
            activeKeyAbility="HeadMemory",
            currentRoom=4,
        )

    return base


def assert_fields(data: dict, **expected) -> None:
    for key, value in expected.items():
        assert data[key] == value, f"{key}: expected {value!r}, got {data[key]!r}"


def main() -> None:
    scenarios = {
        "arrival": (0, {"currentBeat": 0, "hasHouseKey": False}),
        "ghost_key_collected": (31, {"hasGhostKey": True, "echoEncounterActive": False}),
        "during_echo": (4, {"echoEncounterActive": True, "chapterComplete": False}),
        "sealed_door_solved": (4, {"solvedPuzzleIds": lambda v: "chapter1_sealed_door" in v}),
        "chapter_complete": (5, {"chapterComplete": True, "echoEncounterCleared": True}),
    }

    with tempfile.TemporaryDirectory() as tmp:
        for name, (beat, checks) in scenarios.items():
            data = beat_snapshot(beat)
            path = Path(tmp) / f"{name}.json"
            path.write_text(json.dumps(data, indent=2))

            loaded = json.loads(path.read_text())
            for key, value in checks.items():
                if callable(value):
                    assert value(loaded[key]), f"{name}: check failed for {key}"
                else:
                    assert loaded[key] == value, f"{name}: {key}"

    print("Chapter 1 save edge-case payloads validated.")


if __name__ == "__main__":
    main()