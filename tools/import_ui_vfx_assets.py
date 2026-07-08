#!/usr/bin/env python3
"""Import UI key slots, VFX sprite sheets, memory panels, and zone backgrounds."""

from __future__ import annotations

import json
import shutil
from datetime import datetime, timezone
from pathlib import Path

from PIL import Image

from art_sources import resolve_art_sources

ROOT = Path(__file__).resolve().parents[1]
SRC = resolve_art_sources()

KEY_SLOTS_OUT = ROOT / "Assets/_Project/Art/UI/KeySlots"
VFX_OUT = ROOT / "Assets/_Project/Art/VFX"
MEMORY_OUT = ROOT / "Assets/_Project/Art/Memory/Panels"
RES_MEMORY = ROOT / "Assets/_Project/Resources/Art/Memory"
RES_VFX = ROOT / "Assets/_Project/Resources/Art/VFX"
RES_KEY_SLOTS = ROOT / "Assets/_Project/Resources/Art/UI/KeySlots"
PARALLAX_OUT = ROOT / "Assets/_Project/Art/Parallax"
MANIFEST = ROOT / "Assets/_Project/Art/ai_assets_manifest.json"

FOLDER_GUIDS = {
    "Assets/_Project/Art/UI/KeySlots": "b5000000000000100000000000000010",
    "Assets/_Project/Art/VFX": "b6000000000000100000000000000010",
    "Assets/_Project/Art/Memory": "b7000000000000200000000000000020",
    "Assets/_Project/Art/Memory/Panels": "b7000000000000210000000000000021",
    "Assets/_Project/Resources/Art/Memory": "b7000000000000300000000000000030",
    "Assets/_Project/Resources/Art/VFX": "b6000000000000200000000000000020",
    "Assets/_Project/Resources/Art/UI/KeySlots": "b5000000000000200000000000000020",
}

KEY_SLOTS = [
    ("17_key_slots.jpg", 5, [
        ("key_slot_empty.png", "b5000000000000010000000000000001"),
        ("key_slot_ghost_active.png", "b5000000000000020000000000000002"),
        ("key_slot_head_active.png", "b5000000000000030000000000000003"),
        ("key_slot_cooldown.png", "b5000000000000040000000000000004"),
        ("key_slot_discovered.png", "b5000000000000050000000000000005"),
    ]),
]

VFX_SHEETS = [
    ("18_vfx_ghost_phase.jpg", "GhostPhase", 6, "b6000000000000010000000000000001"),
    ("20_vfx_memory.jpg", "Memory", 6, "b6000000000000110000000000000011"),
    ("19_vfx_echo_fog.jpg", "EchoFog", 6, "b6000000000000210000000000000021"),
]

MEMORY_PANELS = [
    ("21_memory_panel_1.jpg", "memory_panel_01.png", "b7000000000000010000000000000001"),
    ("22_memory_panel_2.jpg", "memory_panel_02.png", "b7000000000000020000000000000002"),
    ("23_memory_panel_3.jpg", "memory_panel_03.png", "b7000000000000030000000000000003"),
]

ZONE_BACKGROUNDS = [
    ("24_foyer_bg_far.jpg", "foyer_far.png", "b3000000000000040000000000000004", 2048),
    ("25_library_bg_far.jpg", "library_far.png", "b3000000000000050000000000000005", 2048),
]

KEY_SLOT_LIBRARY_ASSET_GUID = "b5000000000000300000000000000030"
MEMORY_LIBRARY_ASSET_GUID = "b7000000000000400000000000000040"
VFX_LIBRARY_ASSET_GUID = "b6000000000000300000000000000030"
SCRIPT_GUID_KEY_SLOT = "a1b2c3d4e5f6789012345678abcdef39"
SCRIPT_GUID_PARTICLE_VFX = "a1b2c3d4e5f6789012345678abcdef3a"
SCRIPT_GUID_MEMORY_LIBRARY = "a1b2c3d4e5f6789012345678abcdef3b"


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


def sprite_meta(guid: str, ppu: int = 100) -> str:
    return f"""fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
    mipMapFadePercent: 0
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
    flipGreenChannel: 0
  isReadable: 0
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMipmapLimit: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: -1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 1
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 1
  spriteExtrude: 1
  spriteMeshType: 1
  alignment: 0
  spritePivot: {{x: 0.5, y: 0.5}}
  spritePixelsToUnits: {ppu}
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
  spriteGenerateFallbackPhysicsShape: 1
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationDetail: -1
  textureType: 8
  textureShape: 1
  singleChannelComponent: 0
  flipbookRows: 1
  flipbookColumns: 1
  maxTextureSizeSet: 0
  compressionQualitySet: 0
  textureFormatSet: 0
  ignorePngGamma: 0
  applyGammaDecoding: 0
  swizzle: 50462976
  cookieLightType: 0
  platformSettings:
  - serializedVersion: 4
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 2048
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 1
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
    ignorePlatformSupport: 0
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_BC6H_BC7: 0
  - serializedVersion: 4
    buildTarget: iPhone
    maxTextureSize: 1024
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 1
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
    ignorePlatformSupport: 0
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_BC6H_BC7: 0
  spriteSheet:
    serializedVersion: 2
    sprites: []
    outline: []
    customData: 
    physicsShape: []
    bones: []
    spriteID: 5e97eb03825dee720800000000000000
    internalID: 0
    vertices: []
    indices: 
    edges: []
    weights: []
    secondaryTextures: []
    spriteCustomMetadata:
      entries: []
    nameFileIdTable: {{}}
  mipmapLimitGroupName: 
  pSDRemoveMatte: 0
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""


def asset_meta(guid: str) -> str:
    return f"""fileFormatVersion: 2
guid: {guid}
NativeFormatImporter:
  externalObjects: {{}}
  mainObjectFileID: 11400000
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""


def fit_width(img: Image.Image, width: int) -> Image.Image:
    w, h = img.size
    target_h = max(64, int(h * (width / w)))
    return img.resize((width, target_h), Image.Resampling.LANCZOS)


def slice_row(src: Path, count: int, specs: list[tuple[str, str]], out_dir: Path, res_dir: Path | None = None) -> dict[str, str]:
    img = Image.open(src).convert("RGBA")
    w, h = img.size
    cell_w = w // count
    guids: dict[str, str] = {}
    for i, (name, guid) in enumerate(specs):
        box = (i * cell_w, 0, (i + 1) * cell_w if i < count - 1 else w, h)
        crop = img.crop(box)
        bbox = crop.getbbox()
        if bbox:
            crop = crop.crop(bbox)
        dest = out_dir / name
        dest.parent.mkdir(parents=True, exist_ok=True)
        crop.save(dest, format="PNG", optimize=True)
        (Path(str(dest) + ".meta")).write_text(sprite_meta(guid))
        guids[name.replace(".png", "")] = guid
        if res_dir is not None:
            res_dest = res_dir / name
            res_dest.write_bytes(dest.read_bytes())
            (Path(str(res_dest) + ".meta")).write_text(sprite_meta(guid))
        print(f"  {name}")
    return guids


def slice_vfx_sheet(src_name: str, prefix: str, count: int, base_guid: str) -> dict[str, str]:
    src = SRC / src_name
    if not src.exists():
        raise FileNotFoundError(src)
    specs = []
    guids: dict[str, str] = {}
    for i in range(count):
        suffix = f"{int(base_guid[-2:], 16) + i:02x}"
        guid = base_guid[:-2] + suffix
        name = f"{prefix.lower()}_{i:02d}.png"
        specs.append((name, guid))
    result = slice_row(src, count, specs, RES_VFX, None)
    guids.update(result)
    return guids


def write_key_slot_library(guids: dict[str, str]) -> None:
    asset_path = RES_KEY_SLOTS / "KeySlotLibrary.asset"
    fields = {
        "empty": "key_slot_empty",
        "ghostActive": "key_slot_ghost_active",
        "headActive": "key_slot_head_active",
        "cooldown": "key_slot_cooldown",
        "discovered": "key_slot_discovered",
    }
    lines = [
        "%YAML 1.1",
        "%TAG !u! tag:unity3d.com,2011:",
        "--- !u!114 &11400000",
        "MonoBehaviour:",
        "  m_ObjectHideFlags: 0",
        "  m_CorrespondingSourceObject: {fileID: 0}",
        "  m_PrefabInstance: {fileID: 0}",
        "  m_PrefabAsset: {fileID: 0}",
        "  m_GameObject: {fileID: 0}",
        "  m_Enabled: 1",
        "  m_EditorHideFlags: 0",
        "  m_Script: {fileID: 11500000, guid: a1b2c3d4e5f6789012345678abcdef39, type: 3}",
        "  m_Name: KeySlotLibrary",
        "  m_EditorClassIdentifier: ",
    ]
    for field, key in fields.items():
        guid = guids.get(key)
        ref = f"{{fileID: 21300000, guid: {guid}, type: 3}}" if guid else "{fileID: 0}"
        lines.append(f"  {field}: {ref}")
    asset_path.parent.mkdir(parents=True, exist_ok=True)
    asset_path.write_text("\n".join(lines) + "\n")
    (Path(str(asset_path) + ".meta")).write_text(asset_meta(KEY_SLOT_LIBRARY_ASSET_GUID))


def write_memory_library(guids: dict[str, str]) -> None:
    asset_path = RES_MEMORY / "MemoryPanelLibrary.asset"
    lines = [
        "%YAML 1.1",
        "%TAG !u! tag:unity3d.com,2011:",
        "--- !u!114 &11400000",
        "MonoBehaviour:",
        "  m_ObjectHideFlags: 0",
        "  m_CorrespondingSourceObject: {fileID: 0}",
        "  m_PrefabInstance: {fileID: 0}",
        "  m_PrefabAsset: {fileID: 0}",
        "  m_GameObject: {fileID: 0}",
        "  m_Enabled: 1",
        "  m_EditorHideFlags: 0",
        "  m_Script: {fileID: 11500000, guid: a1b2c3d4e5f6789012345678abcdef3b, type: 3}",
        "  m_Name: MemoryPanelLibrary",
        "  m_EditorClassIdentifier: ",
    ]
    for i, key in enumerate(["memory_panel_01", "memory_panel_02", "memory_panel_03"], start=1):
        guid = guids.get(key)
        ref = f"{{fileID: 21300000, guid: {guid}, type: 3}}" if guid else "{fileID: 0}"
        lines.append(f"  panel{i}: {ref}")
    asset_path.parent.mkdir(parents=True, exist_ok=True)
    asset_path.write_text("\n".join(lines) + "\n")
    (Path(str(asset_path) + ".meta")).write_text(asset_meta(MEMORY_LIBRARY_ASSET_GUID))


def main() -> None:
    print(f"Using art sources: {SRC}")
    for rel, guid in FOLDER_GUIDS.items():
        path = ROOT / rel
        path.mkdir(parents=True, exist_ok=True)
        (Path(str(path) + ".meta")).write_text(folder_meta(guid))

    all_guids: dict[str, dict[str, str]] = {}

    print("Key slots:")
    for src_name, count, specs in KEY_SLOTS:
        src = SRC / src_name
        all_guids["key_slots"] = slice_row(src, count, specs, RES_KEY_SLOTS, None)
    write_key_slot_library(all_guids["key_slots"])

    print("VFX sheets:")
    all_guids["vfx"] = {}
    for src_name, prefix, count, base_guid in VFX_SHEETS:
        sheet_guids = slice_vfx_sheet(src_name, prefix, count, base_guid)
        all_guids["vfx"].update(sheet_guids)

    print("Memory panels:")
    panel_guids: dict[str, str] = {}
    for src_name, out_name, guid in MEMORY_PANELS:
        src = SRC / src_name
        img = fit_width(Image.open(src).convert("RGBA"), 1920)
        dest = RES_MEMORY / out_name
        img.save(dest, format="PNG", optimize=True)
        (Path(str(dest) + ".meta")).write_text(sprite_meta(guid, ppu=100))
        panel_guids[out_name.replace(".png", "")] = guid
        print(f"  {out_name}")
    write_memory_library(panel_guids)
    all_guids["memory_panels"] = panel_guids

    print("Zone backgrounds:")
    for src_name, out_name, guid, width in ZONE_BACKGROUNDS:
        src = SRC / src_name
        img = fit_width(Image.open(src).convert("RGBA"), width)
        dest = PARALLAX_OUT / out_name
        img.save(dest, format="PNG", optimize=True)
        (Path(str(dest) + ".meta")).write_text(sprite_meta(guid, ppu=100))
        all_guids.setdefault("backgrounds", {})[out_name.replace(".png", "")] = guid
        print(f"  {out_name}")

    manifest = json.loads(MANIFEST.read_text()) if MANIFEST.exists() else {"version": "1.0", "assets": []}
    manifest["updated_at"] = datetime.now(timezone.utc).isoformat()
    manifest["ui_key_slots"] = [{"id": k, "guid": v} for k, v in all_guids["key_slots"].items()]
    manifest["vfx_sprites"] = [{"id": k, "guid": v} for k, v in all_guids["vfx"].items()]
    manifest["memory_panels"] = [{"id": k, "guid": v} for k, v in panel_guids.items()]
    manifest["zone_backgrounds"] = [{"id": k, "guid": v} for k, v in all_guids.get("backgrounds", {}).items()]
    MANIFEST.write_text(json.dumps(manifest, indent=2) + "\n")

    (ROOT / "tools/ui_vfx_guid_map.json").write_text(json.dumps(all_guids, indent=2) + "\n")
    print("UI/VFX import complete.")


if __name__ == "__main__":
    main()