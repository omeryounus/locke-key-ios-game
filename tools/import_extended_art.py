#!/usr/bin/env python3
"""Import normal maps, UI icons, parallax layers, and wire sprite secondary textures."""

from __future__ import annotations

import json
import math
from datetime import datetime, timezone
from pathlib import Path

from PIL import Image, ImageFilter

from art_sources import resolve_art_sources

ROOT = Path(__file__).resolve().parents[1]
SPRITES = ROOT / "Assets/_Project/Art/Sprites"
NORMALS = ROOT / "Assets/_Project/Art/NormalMaps"
UI_OUT = ROOT / "Assets/_Project/Art/UI/Icons"
PARALLAX_OUT = ROOT / "Assets/_Project/Art/Parallax"
RES_UI = ROOT / "Assets/_Project/Resources/Art/UI"
MANIFEST = ROOT / "Assets/_Project/Art/ai_assets_manifest.json"

FOLDER_GUIDS = {
    "Assets/_Project/Art/NormalMaps": "b2000000000000100000000000000010",
    "Assets/_Project/Art/UI": "b4000000000000200000000000000020",
    "Assets/_Project/Art/UI/Icons": "b4000000000000210000000000000021",
    "Assets/_Project/Art/Parallax": "b3000000000000100000000000000010",
    "Assets/_Project/Resources/Art": "b4000000000000300000000000000030",
    "Assets/_Project/Resources/Art/UI": "b4000000000000310000000000000031",
}

PARALLAX = [
    ("13.jpg", "parallax_far.png", "b3000000000000010000000000000001", 2048),
    ("14.jpg", "parallax_mid.png", "b3000000000000020000000000000002", 2048),
    ("15.jpg", "parallax_near.png", "b3000000000000030000000000000003", 2048),
]

UI_SLICES = [
    ("12.jpg", 6, [
        ("icon_move_left.png", "b4000000000000010000000000000001"),
        ("icon_move_right.png", "b4000000000000020000000000000002"),
        ("icon_jump.png", "b4000000000000030000000000000003"),
        ("icon_interact.png", "b4000000000000040000000000000004"),
        ("icon_use_key.png", "b4000000000000050000000000000005"),
        ("icon_key_cycle.png", "b4000000000000060000000000000006"),
    ]),
    ("16.jpg", 3, [
        ("icon_ghost_key.png", "b4000000000000070000000000000007"),
        ("icon_head_key.png", "b4000000000000080000000000000008"),
        ("icon_house_key.png", "b4000000000000090000000000000009"),
    ]),
]

NORMAL_GUID_OFFSET = "b2000000000000"
SPRITE_GUID_MAP = json.loads((ROOT / "tools/sprite_guid_map.json").read_text())

SCRIPT_GUIDS = {
    "ParallaxLayer.cs": "a1b2c3d4e5f6789012345678abcdef30",
    "UIIconLibrary.cs": "a1b2c3d4e5f6789012345678abcdef31",
}

UI_LIBRARY_ASSET_GUID = "b4000000000000100000000000000010"


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


def sprite_meta(guid: str, ppu: int = 100, normal_guid: str | None = None) -> str:
    secondary = ""
    if normal_guid:
        secondary = f"""
  secondaryTextures:
  - name: _NormalMap
    guid: {normal_guid}
    type: 3"""

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
    forceMaximumCompressionQuality_BC6H_BC7: 0{secondary}
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


def normal_meta(guid: str) -> str:
    return f"""fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 0
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
  spriteMode: 0
  spriteExtrude: 1
  spriteMeshType: 1
  alignment: 0
  spritePivot: {{x: 0.5, y: 0.5}}
  spritePixelsToUnits: 100
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
  spriteGenerateFallbackPhysicsShape: 1
  alphaUsage: 1
  alphaIsTransparency: 0
  spriteTessellationDetail: -1
  textureType: 1
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
  spriteSheet:
    serializedVersion: 2
    sprites: []
    outline: []
    customData: 
    physicsShape: []
    bones: []
    spriteID: 
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


def cs_meta(guid: str) -> str:
    return f"""fileFormatVersion: 2
guid: {guid}
MonoImporter:
  externalObjects: {{}}
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {{instanceID: 0}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""


def write_folders() -> None:
    for rel, guid in FOLDER_GUIDS.items():
        path = ROOT / rel
        path.mkdir(parents=True, exist_ok=True)
        (Path(str(path) + ".meta")).write_text(folder_meta(guid))


def luminance(r: int, g: int, b: int) -> float:
    return (0.299 * r + 0.587 * g + 0.114 * b) / 255.0


def generate_normal_map(path: Path, out_path: Path, guid: str) -> None:
    img = Image.open(path).convert("RGBA")
    img = img.filter(ImageFilter.GaussianBlur(radius=1))
    w, h = img.size
    pixels = img.load()

    heights = [[0.0] * w for _ in range(h)]
    for y in range(h):
        for x in range(w):
            r, g, b, a = pixels[x, y]
            if a < 16:
                heights[y][x] = 0.0
            else:
                heights[y][x] = luminance(r, g, b)

    normal = Image.new("RGBA", (w, h))
    npx = normal.load()
    strength = 2.5

    for y in range(h):
        for x in range(w):
            left = heights[y][max(0, x - 1)]
            right = heights[y][min(w - 1, x + 1)]
            down = heights[max(0, y - 1)][x]
            up = heights[min(h - 1, y + 1)][x]
            dx = (left - right) * strength
            dy = (down - up) * strength
            dz = 1.0
            length = math.sqrt(dx * dx + dy * dy + dz * dz)
            dx /= length
            dy /= length
            dz /= length
            nr = int((dx * 0.5 + 0.5) * 255)
            ng = int((dy * 0.5 + 0.5) * 255)
            nb = int((dz * 0.5 + 0.5) * 255)
            alpha = pixels[x, y][3]
            npx[x, y] = (nr, ng, nb, alpha)

    out_path.parent.mkdir(parents=True, exist_ok=True)
    normal.save(out_path, format="PNG", optimize=True)
    (Path(str(out_path) + ".meta")).write_text(normal_meta(guid))


def fit_width(img: Image.Image, width: int) -> Image.Image:
    w, h = img.size
    target_h = max(64, int(h * (width / w)))
    return img.resize((width, target_h), Image.Resampling.LANCZOS)


def slice_row(src: Path, count: int, out_specs: list[tuple[str, str]]) -> None:
    img = Image.open(src).convert("RGBA")
    w, h = img.size
    cell_w = w // count
    cell_h = min(h, cell_w)
    y_offset = (h - cell_h) // 2

    for i, (name, guid) in enumerate(out_specs):
        x0 = i * cell_w
        x1 = (i + 1) * cell_w if i < count - 1 else w
        crop = img.crop((x0, y_offset, x1, y_offset + cell_h))

        # Key out the solid background color
        pixels = crop.load()
        bg_r, bg_g, bg_b, bg_a = pixels[0, 0]
        cw, ch = crop.size
        for y in range(ch):
            for x in range(cw):
                r, g, b, a = pixels[x, y]
                dist = ((r - bg_r)**2 + (g - bg_g)**2 + (b - bg_b)**2)**0.5
                if dist < 45.0:
                    pixels[x, y] = (0, 0, 0, 0)

        bbox = crop.getbbox()
        if bbox:
            crop = crop.crop(bbox)

        dest = RES_UI / name
        crop.save(dest, format="PNG", optimize=True)
        (Path(str(dest) + ".meta")).write_text(sprite_meta(guid, ppu=100))


def normal_guid_for_sprite(sprite_guid: str) -> str:
    return f"b2{sprite_guid[2:]}"


def update_sprite_metas_with_normals() -> dict[str, str]:
    mapping: dict[str, str] = {}
    for rel in SPRITES.rglob("*.png"):
        meta_path = Path(str(rel) + ".meta")
        if not meta_path.exists():
            continue
        sprite_guid = None
        for line in meta_path.read_text().splitlines():
            if line.startswith("guid: "):
                sprite_guid = line.split(": ", 1)[1].strip()
                break
        if not sprite_guid:
            continue

        normal_guid = normal_guid_for_sprite(sprite_guid)
        normal_rel = NORMALS / rel.relative_to(SPRITES)
        generate_normal_map(rel, normal_rel, normal_guid)

        text = meta_path.read_text()
        marker = "  spriteSheet:"
        if "_NormalMap" not in text and marker in text:
            secondary = f"""  secondaryTextures:
  - name: _NormalMap
    guid: {normal_guid}
    type: 3
"""
            insert_at = text.find(marker)
            text = text[:insert_at] + secondary + text[insert_at:]
            meta_path.write_text(text)

        mapping[rel.stem] = normal_guid
        print(f"Normal map: {rel.name}")
    return mapping


def import_parallax(src_dir: Path) -> dict[str, str]:
    guids: dict[str, str] = {}
    for src_name, out_name, guid, width in PARALLAX:
        src = src_dir / src_name
        if not src.exists():
            raise FileNotFoundError(f"Missing parallax source: {src}")
        img = fit_width(Image.open(src).convert("RGBA"), width)
        dest = PARALLAX_OUT / out_name
        img.save(dest, format="PNG", optimize=True)
        (Path(str(dest) + ".meta")).write_text(sprite_meta(guid, ppu=100))
        guids[out_name.replace(".png", "")] = guid
        print(f"Parallax: {out_name}")
    return guids


def import_ui_icons(src_dir: Path) -> dict[str, str]:
    guids: dict[str, str] = {}
    for src_name, count, specs in UI_SLICES:
        src = src_dir / src_name
        if not src.exists():
            raise FileNotFoundError(f"Missing UI source: {src}")
        slice_row(src, count, specs)
        for name, guid in specs:
            guids[name.replace(".png", "").replace("icon_", "")] = guid
            print(f"UI icon: {name}")
    return guids


def write_ui_icon_library_asset(icon_guids: dict[str, str]) -> None:
    asset_path = RES_UI / "UIIconLibrary.asset"
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
        "  m_Script: {fileID: 11500000, guid: a1b2c3d4e5f6789012345678abcdef31, type: 3}",
        "  m_Name: UIIconLibrary",
        "  m_EditorClassIdentifier: ",
    ]

    fields = {
        "moveLeft": "move_left",
        "moveRight": "move_right",
        "jump": "jump",
        "interact": "interact",
        "useKey": "use_key",
        "ghostKeyIcon": "ghost_key",
        "headKeyIcon": "head_key",
        "houseKeyIcon": "house_key",
    }

    for field, key in fields.items():
        guid = icon_guids.get(key)
        ref = f"{{fileID: 21300000, guid: {guid}, type: 3}}" if guid else "{fileID: 0}"
        lines.append(f"  {field}: {ref}")

    asset_path.write_text("\n".join(lines) + "\n")
    (Path(str(asset_path) + ".meta")).write_text(asset_meta(UI_LIBRARY_ASSET_GUID))


def update_manifest(normals: dict, parallax: dict, icons: dict) -> None:
    manifest = json.loads(MANIFEST.read_text()) if MANIFEST.exists() else {"version": "1.0", "assets": []}
    manifest["updated_at"] = datetime.now(timezone.utc).isoformat()
    manifest["normal_maps"] = [{"id": k, "guid": v} for k, v in normals.items()]
    manifest["parallax"] = [{"id": k, "guid": v} for k, v in parallax.items()]
    manifest["ui_icons"] = [{"id": k, "guid": v} for k, v in icons.items()]
    MANIFEST.write_text(json.dumps(manifest, indent=2) + "\n")

    maps = {
        "parallax": parallax,
        "ui_icons": icons,
        "normal_maps": {k: normal_guid_for_sprite(v) for k, v in SPRITE_GUID_MAP.items()},
    }
    (ROOT / "tools/extended_art_guid_map.json").write_text(json.dumps(maps, indent=2) + "\n")


def write_script_metas() -> None:
    env_dir = ROOT / "Assets/_Project/Scripts/Environment"
    env_dir.mkdir(parents=True, exist_ok=True)
    (Path(str(env_dir) + ".meta")).write_text(folder_meta("a0000000000000170000000000000017"))
    (Path(str(env_dir / "ParallaxLayer.cs") + ".meta")).write_text(cs_meta(SCRIPT_GUIDS["ParallaxLayer.cs"]))
    (Path(str(ROOT / "Assets/_Project/Scripts/UI/UIIconLibrary.cs") + ".meta")).write_text(
        cs_meta(SCRIPT_GUIDS["UIIconLibrary.cs"])
    )


def main() -> None:
    src_dir = resolve_art_sources()
    print(f"Using art sources: {src_dir}")

    write_folders()
    write_script_metas()
    normals = update_sprite_metas_with_normals()
    parallax = import_parallax(src_dir)
    icons = import_ui_icons(src_dir)
    write_ui_icon_library_asset(icons)
    update_manifest(normals, parallax, icons)
    print("Extended art import complete.")


if __name__ == "__main__":
    main()