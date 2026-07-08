#!/usr/bin/env python3
"""Import generated art into Unity sprite assets with .meta files."""

from __future__ import annotations

import json
from datetime import datetime, timezone
from pathlib import Path

from PIL import Image

from art_sources import resolve_art_sources

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "Assets/_Project/Art/Sprites"

ASSETS = [
    ("1.jpg", "Keys/ghost_key.png", "b1000000000000010000000000000001", 256, True),
    ("4.jpg", "Keys/head_key.png", "b1000000000000020000000000000002", 256, True),
    ("3.jpg", "Keys/house_key.png", "b1000000000000030000000000000003", 192, True),
    ("6.jpg", "Characters/player.png", "b1000000000000040000000000000004", 220, True),
    ("5.jpg", "Environments/bookshelf_collapsed.png", "b1000000000000050000000000000005", 360, True),
    ("2.jpg", "Enemies/echo.png", "b1000000000000060000000000000006", 220, True),
    ("7.jpg", "Environments/door_wood.png", "b1000000000000070000000000000007", 280, True),
    ("8.jpg", "Environments/door_sealed.png", "b1000000000000080000000000000008", 280, True),
    ("9.jpg", "Props/portrait.png", "b1000000000000090000000000000009", 240, True),
    ("10.jpg", "Environments/ground_tile.png", "b10000000000000a000000000000000a", 1024, False),
    ("11.jpg", "Environments/wall_panel.png", "b10000000000000b000000000000000b", 1024, False),
]

FOLDER_GUIDS = {
    "Assets/_Project/Art/Sprites": "b1000000000000100000000000000010",
    "Assets/_Project/Art/Sprites/Keys": "b1000000000000110000000000000011",
    "Assets/_Project/Art/Sprites/Characters": "b1000000000000120000000000000012",
    "Assets/_Project/Art/Sprites/Environments": "b1000000000000130000000000000013",
    "Assets/_Project/Art/Sprites/Props": "b1000000000000140000000000000014",
    "Assets/_Project/Art/Sprites/Enemies": "b1000000000000150000000000000015",
}


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


def texture_meta(guid: str, ppu: int = 100) -> str:
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


def remove_dark_background(img: Image.Image, threshold: int = 32) -> Image.Image:
    img = img.convert("RGBA")
    pixels = img.load()
    width, height = img.size

    corners = [
        pixels[0, 0],
        pixels[width - 1, 0],
        pixels[0, height - 1],
        pixels[width - 1, height - 1],
    ]
    bg = (
        sum(c[0] for c in corners) // 4,
        sum(c[1] for c in corners) // 4,
        sum(c[2] for c in corners) // 4,
    )

    for y in range(height):
        for x in range(width):
            r, g, b, a = pixels[x, y]
            dist = abs(r - bg[0]) + abs(g - bg[1]) + abs(b - bg[2])
            if dist < threshold:
                pixels[x, y] = (r, g, b, 0)
            elif r + g + b < 24:
                pixels[x, y] = (r, g, b, 0)

    bbox = img.getbbox()
    if bbox:
        img = img.crop(bbox)
    return img


def fit_long_edge(img: Image.Image, long_edge: int, wide_strip: bool) -> Image.Image:
    w, h = img.size
    if wide_strip:
        target_w = long_edge
        target_h = max(64, int(h * (long_edge / w)))
    else:
        scale = long_edge / max(w, h)
        target_w = max(1, int(w * scale))
        target_h = max(1, int(h * scale))
    return img.resize((target_w, target_h), Image.Resampling.LANCZOS)


def write_folder_metas() -> None:
    for rel, guid in sorted(FOLDER_GUIDS.items(), key=lambda item: item[0].count("/")):
        path = ROOT / rel
        path.mkdir(parents=True, exist_ok=True)
        (Path(str(path) + ".meta")).write_text(folder_meta(guid))


def main() -> None:
    src_dir = resolve_art_sources()
    print(f"Using art sources: {src_dir}")

    OUT.mkdir(parents=True, exist_ok=True)
    write_folder_metas()

    manifest = {
        "version": "1.0",
        "generated_at": datetime.now(timezone.utc).isoformat(),
        "style": "Art Bible v1.0 — atmospheric horror, painterly 2D",
        "assets": [],
    }

    guid_map: dict[str, str] = {}

    for src_name, rel_out, guid, long_edge, cut_bg in ASSETS:
        src = src_dir / src_name
        if not src.exists():
            raise FileNotFoundError(src)

        img = Image.open(src)
        if cut_bg:
            img = remove_dark_background(img)
        img = fit_long_edge(img, long_edge, wide_strip="ground" in rel_out or "wall" in rel_out)

        dest = OUT / rel_out
        dest.parent.mkdir(parents=True, exist_ok=True)
        img.save(dest, format="PNG", optimize=True)
        (Path(str(dest) + ".meta")).write_text(texture_meta(guid))

        key = Path(rel_out).stem
        guid_map[key] = guid
        manifest["assets"].append(
            {
                "id": key,
                "path": f"Assets/_Project/Art/Sprites/{rel_out}",
                "guid": guid,
                "source": src_name,
                "size": list(img.size),
            }
        )
        print(f"Imported {rel_out} ({img.size[0]}x{img.size[1]})")

    manifest_path = ROOT / "Assets/_Project/Art/ai_assets_manifest.json"
    manifest_path.write_text(json.dumps(manifest, indent=2) + "\n")
    (Path(str(manifest_path) + ".meta")).write_text(
        f"""fileFormatVersion: 2
guid: b1000000000000160000000000000016
TextScriptImporter:
  externalObjects: {{}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""
    )

    catalog_path = ROOT / "tools/sprite_guid_map.json"
    catalog_path.write_text(json.dumps(guid_map, indent=2) + "\n")
    print(f"Wrote manifest and {len(guid_map)} sprites.")


if __name__ == "__main__":
    main()