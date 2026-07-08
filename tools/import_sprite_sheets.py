#!/usr/bin/env python3
"""Import player/Echo sprite sheets, room layers, and sealed passage art."""

from __future__ import annotations

import json
from datetime import datetime, timezone
from pathlib import Path

from PIL import Image

from art_sources import resolve_art_sources

ROOT = Path(__file__).resolve().parents[1]
SPRITES = ROOT / "Assets/_Project/Art/Sprites"
RES = ROOT / "Assets/_Project/Resources/Art"
PARALLAX = ROOT / "Assets/_Project/Art/Parallax"

FOLDER_GUIDS = {
    "Assets/_Project/Resources/Art/Characters": "b8000000000000100000000000000010",
    "Assets/_Project/Resources/Art/Parallax": "b8000000000000200000000000000020",
    "Assets/_Project/Resources/Art/Environments": "b8000000000000300000000000000030",
}

PLAYER_FRAMES = {
    "player_idle": ("b8000000000001010000000000000001", "b8000000000002010000000000000001"),
    "player_walk_a": ("b8000000000001020000000000000002", "b8000000000002020000000000000002"),
    "player_walk_b": ("b8000000000001030000000000000003", "b8000000000002030000000000000003"),
    "player_jump": ("b8000000000001040000000000000004", "b8000000000002040000000000000004"),
}

ECHO_FRAMES = {
    "echo_00": ("b8000000000003010000000000000001", "b8000000000004010000000000000001"),
    "echo_01": ("b8000000000003020000000000000002", "b8000000000004030000000000000003"),
    "echo_02": ("b8000000000003030000000000000004", "b8000000000004040000000000000004"),
    "echo_03": ("b8000000000003040000000000000005", "b8000000000004050000000000000005"),
    "echo_04": ("b8000000000003050000000000000006", "b8000000000004060000000000000006"),
    "echo_05": ("b8000000000003060000000000000007", "b8000000000004070000000000000007"),
}

ZONE_LAYERS = [
    ("24_foyer_bg_far.jpg", "foyer_far.png", "b3000000000000040000000000000004", "b8000000000005010000000000000001", 2048),
    ("13.jpg", "foyer_mid.png", "b3000000000000060000000000000006", "b8000000000005020000000000000002", 2048),
    ("14.jpg", "foyer_near.png", "b3000000000000070000000000000007", "b8000000000005030000000000000003", 2048),
    ("25_library_bg_far.jpg", "library_far.png", "b3000000000000050000000000000005", "b8000000000005040000000000000004", 2048),
    ("14.jpg", "library_mid.png", "b3000000000000080000000000000008", "b8000000000005050000000000000005", 2048),
    ("15.jpg", "library_near.png", "b3000000000000090000000000000009", "b8000000000005060000000000000006", 2048),
    ("8.jpg", "sealed_passage.png", "b30000000000000a000000000000000a", "b8000000000005070000000000000007", 2048),
]


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
  spritePivot: {{x: 0.5, y: 0.0}}
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
    w, h = img.size
    corners = [pixels[0, 0], pixels[w - 1, 0], pixels[0, h - 1], pixels[w - 1, h - 1]]
    bg = (
        sum(c[0] for c in corners) // 4,
        sum(c[1] for c in corners) // 4,
        sum(c[2] for c in corners) // 4,
    )
    for y in range(h):
        for x in range(w):
            r, g, b, a = pixels[x, y]
            dist = abs(r - bg[0]) + abs(g - bg[1]) + abs(b - bg[2])
            if dist < threshold or r + g + b < 24:
                pixels[x, y] = (r, g, b, 0)
    bbox = img.getbbox()
    return img.crop(bbox) if bbox else img


def fit_long_edge(img: Image.Image, long_edge: int) -> Image.Image:
    w, h = img.size
    scale = long_edge / max(w, h)
    return img.resize((max(1, int(w * scale)), max(1, int(h * scale))), Image.Resampling.LANCZOS)


def slice_horizontal(img: Image.Image, frames: int) -> list[Image.Image]:
    w, h = img.size
    frame_w = max(1, w // frames)
    return [img.crop((i * frame_w, 0, (i + 1) * frame_w if i < frames - 1 else w, h)) for i in range(frames)]


def write_sprite(path: Path, img: Image.Image, guid: str, ppu: int = 220) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    img.save(path, format="PNG", optimize=True)
    (Path(str(path) + ".meta")).write_text(sprite_meta(guid, ppu))


def main() -> None:
    src_dir = resolve_art_sources()
    print(f"Using art sources: {src_dir}")

    for rel, guid in FOLDER_GUIDS.items():
        (ROOT / rel).mkdir(parents=True, exist_ok=True)
        (Path(str(ROOT / rel) + ".meta")).write_text(folder_meta(guid))

    guid_map: dict[str, str] = {}

    # Player animation frames from 6.jpg
    player_src = Image.open(src_dir / "6.jpg")
    player_src = remove_dark_background(player_src)
    player_src = fit_long_edge(player_src, 880)
    frames = slice_horizontal(player_src, len(PLAYER_FRAMES))
    for (name, (art_guid, res_guid)), frame in zip(PLAYER_FRAMES.items(), frames):
        art_path = SPRITES / "Characters" / f"{name}.png"
        res_path = RES / "Characters" / f"{name}.png"
        write_sprite(art_path, frame, art_guid, ppu=220)
        write_sprite(res_path, frame, res_guid, ppu=220)
        guid_map[name] = art_guid
        print(f"Player frame {name}")

    # Echo sprite sheet from echo fog source
    echo_src = Image.open(src_dir / "19_vfx_echo_fog.jpg")
    echo_src = fit_long_edge(echo_src, 1200)
    echo_frame_images = slice_horizontal(echo_src, len(ECHO_FRAMES))
    for (name, (art_guid, res_guid)), frame in zip(ECHO_FRAMES.items(), echo_frame_images):
        frame = remove_dark_background(frame, threshold=40)
        art_path = SPRITES / "Enemies" / f"{name}.png"
        res_path = RES / "Enemies" / f"{name}.png"
        write_sprite(art_path, frame, art_guid, ppu=180)
        write_sprite(res_path, frame, res_guid, ppu=180)
        guid_map[name] = art_guid
        print(f"Echo frame {name}")

    # Room layers + sealed passage
    PARALLAX.mkdir(parents=True, exist_ok=True)
    for src_name, out_name, art_guid, res_guid, long_edge in ZONE_LAYERS:
        img = Image.open(src_dir / src_name)
        img = fit_long_edge(img, long_edge)
        parallax_path = PARALLAX / out_name
        res_path = RES / "Parallax" / out_name
        write_sprite(parallax_path, img, art_guid, ppu=100)
        write_sprite(res_path, img, res_guid, ppu=100)
        guid_map[out_name.replace(".png", "")] = art_guid
        print(f"Zone layer {out_name}")

    manifest = {
        "generated_at": datetime.now(timezone.utc).isoformat(),
        "player_frames": list(PLAYER_FRAMES.keys()),
        "echo_frames": list(ECHO_FRAMES.keys()),
        "zone_layers": [z[1] for z in ZONE_LAYERS],
        "guids": guid_map,
    }
    out = ROOT / "tools/art_pass_guid_map.json"
    out.write_text(json.dumps(manifest, indent=2) + "\n")
    print(f"Wrote {out}")


if __name__ == "__main__":
    main()