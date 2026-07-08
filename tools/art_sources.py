"""Resolve bundled AI art source JPEGs for import scripts."""

from __future__ import annotations

import os
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
DEFAULT_SRC = ROOT / "tools" / "art_sources"


def resolve_art_sources() -> Path:
    """Return the folder containing numbered source JPEGs (1.jpg … 16.jpg)."""
    override = os.environ.get("LOCKE_ART_SOURCES")
    if override:
        path = Path(override).expanduser().resolve()
        if not path.is_dir():
            raise FileNotFoundError(f"LOCKE_ART_SOURCES is not a directory: {path}")
        return path

    if DEFAULT_SRC.is_dir() and any(DEFAULT_SRC.glob("*.jpg")):
        return DEFAULT_SRC

    raise FileNotFoundError(
        "Art source JPEGs not found. Clone the repo with tools/art_sources/ "
        f"or set LOCKE_ART_SOURCES to a directory containing 1.jpg–16.jpg.\n"
        f"Expected bundled path: {DEFAULT_SRC}"
    )