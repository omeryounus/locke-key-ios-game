#!/usr/bin/env python3
"""Static Unity project checks that do not require the Unity editor."""

from __future__ import annotations

import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
GUID_RE = re.compile(r"^[0-9a-f]{32}$")
GUID_REF_RE = re.compile(r"guid: ([0-9a-f]{32})")
# Unity built-in / package placeholders that do not have project .meta files.
BUILTIN_GUIDS = {
    "0000000000000000e000000000000000",
    "0000000000000000f000000000000000",
}
# GUID prefixes for repo-authored assets (skip package/URP script refs).
PROJECT_GUID_PREFIXES = ("a1b2c3d4", "b1", "b2", "b3", "b4", "e1f2a3b4")


def check_meta_yaml() -> list[str]:
    errors: list[str] = []
    for meta in ROOT.rglob("*.meta"):
        text = meta.read_text(encoding="utf-8", errors="replace")
        if "assetBundleVariant:\n" in text or text.rstrip("\n").endswith("assetBundleVariant:"):
            errors.append(
                f"Invalid YAML in {meta.relative_to(ROOT)}: assetBundleVariant needs trailing space"
            )
    return errors


def check_meta_guids() -> list[str]:
    errors: list[str] = []
    for meta in ROOT.rglob("*.meta"):
        for line in meta.read_text(encoding="utf-8", errors="replace").splitlines():
            if not line.startswith("guid: "):
                continue
            guid = line.split(":", 1)[1].strip()
            if not GUID_RE.fullmatch(guid):
                errors.append(f"Invalid GUID in {meta.relative_to(ROOT)}: {guid!r}")
            break
    return errors


def check_preset_manager() -> list[str]:
    path = ROOT / "ProjectSettings/PresetManager.asset"
    if not path.exists():
        return [f"Missing {path.relative_to(ROOT)}"]
    text = path.read_text(encoding="utf-8")
    if "--- !u!1386491679 &1" not in text:
        return [
            "ProjectSettings/PresetManager.asset must use class ID 1386491679 "
            "(found legacy !u!138 which Unity 6 treats as FixedJoint)."
        ]
    return []


def check_duplicate_guids() -> list[str]:
    by_guid: dict[str, list[str]] = {}
    for meta in ROOT.rglob("*.meta"):
        for line in meta.read_text(encoding="utf-8", errors="replace").splitlines():
            if not line.startswith("guid: "):
                continue
            guid = line.split(":", 1)[1].strip()
            by_guid.setdefault(guid, []).append(str(meta.relative_to(ROOT)))
            break

    errors: list[str] = []
    for guid, paths in sorted(by_guid.items()):
        if len(paths) > 1:
            joined = ", ".join(paths)
            errors.append(f"Duplicate GUID {guid}: {joined}")
    return errors


def build_guid_index() -> dict[str, Path]:
    index: dict[str, Path] = {}
    for meta in ROOT.rglob("*.meta"):
        for line in meta.read_text(encoding="utf-8", errors="replace").splitlines():
            if not line.startswith("guid: "):
                continue
            guid = line.split(":", 1)[1].strip()
            index[guid] = meta
            break
    return index


def check_yaml_guid_references(guid_index: dict[str, Path]) -> list[str]:
    """Validate GUID refs in authored project content (not URP/package settings)."""
    errors: list[str] = []
    project_root = ROOT / "Assets/_Project"
    yaml_paths = [
        *project_root.glob("**/*.unity"),
        *project_root.glob("**/*.asset"),
        *project_root.glob("**/*.prefab"),
    ]
    seen: set[tuple[str, str]] = set()
    for path in yaml_paths:
        rel = str(path.relative_to(ROOT))
        for match in GUID_REF_RE.finditer(path.read_text(encoding="utf-8", errors="replace")):
            guid = match.group(1)
            if guid in BUILTIN_GUIDS or guid in guid_index:
                continue
            if not guid.startswith(PROJECT_GUID_PREFIXES):
                continue
            key = (guid, rel)
            if key in seen:
                continue
            seen.add(key)
            errors.append(f"Dangling GUID reference {guid} in {rel}")
    return errors


def check_ui_icon_library(guid_index: dict[str, Path]) -> list[str]:
    path = ROOT / "Assets/_Project/Resources/Art/UI/UIIconLibrary.asset"
    if not path.exists():
        return []

    errors: list[str] = []
    seen: set[str] = set()
    for match in GUID_REF_RE.finditer(path.read_text(encoding="utf-8")):
        guid = match.group(1)
        if guid in guid_index or guid in seen:
            continue
        seen.add(guid)
        errors.append(f"UIIconLibrary references missing GUID {guid}")
    return errors


def check_required_assets() -> list[str]:
    required = [
        "Assets/_Project/Resources/Art/UI/UIIconLibrary.asset",
        "Assets/_Project/Resources/Art/UI/UIIconLibrary.asset.meta",
        "Assets/_Project/Scenes/Chapter1/Chapter1.unity",
        "Assets/Editor/IOSBuildMenu.cs",
        "Assets/Editor/ProjectVerifyMenu.cs",
    ]
    return [f"Missing required file: {rel}" for rel in required if not (ROOT / rel).exists()]


def main() -> int:
    guid_index = build_guid_index()
    errors = [
        *check_meta_yaml(),
        *check_meta_guids(),
        *check_duplicate_guids(),
        *check_preset_manager(),
        *check_required_assets(),
        *check_yaml_guid_references(guid_index),
        *check_ui_icon_library(guid_index),
    ]
    if errors:
        print("Static verification failed:")
        for err in errors:
            print(f"  - {err}")
        return 1

    print("Static verification passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())