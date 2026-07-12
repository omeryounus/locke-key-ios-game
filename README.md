# Locke & Key: Keyhouse Unlocked

**iOS Narrative Puzzle-Adventure Game**  
Based on the acclaimed comic series by Joe Hill and Gabriel Rodríguez.

## Project Overview

A 2.5D atmospheric puzzle-adventure game where players explore the mysterious Keyhouse, collect magical keys with unique abilities, solve intricate environmental puzzles, and uncover dark family secrets.

**Genre**: Narrative Puzzle-Adventure + Light Horror  
**Perspective**: 2.5D side-scrolling with parallax depth  
**Target Platform**: iOS + iPadOS (Premium $6.99, no IAP)  
**Engine**: Unity 6000.5.2f1 + 2D URP

## Current Status

- [x] Game Design Document (v0.1)
- [x] Core Architecture Scripts (IKeyAbility, EventBus, PuzzleBase)
- [x] Unity Project Scaffold (6000.5.2f1 + 2D URP)
- [x] Art Bible
- [x] Vertical Slice Plan (Chapter 1)
- [x] Chapter 1 vertical slice (All 5 puzzles, Mirror Key discovery, expanded Echo AI with Body Capture, Legacy Input migration, automated 4-layer Parallax)

## Repository Structure

```
LockeKey-iOS/
├── Assets/
│   ├── _Project/
│   │   ├── Art/
│   │   ├── Audio/
│   │   ├── Prefabs/
│   │   ├── Scenes/
│   │   ├── Scripts/
│   │   │   ├── Core/
│   │   │   ├── Keys/
│   │   │   ├── Puzzles/
│   │   │   ├── Player/
│   │   │   └── Narrative/
│   │   ├── ScriptableObjects/
│   │   └── UI/
│   └── Settings/
├── docs/
├── Packages/
└── ProjectSettings/
```

## Getting Started (Unity)

1. Clone this repository
2. Install **Unity 6000.5.2f1** with **iOS Build Support** and **2D** modules
3. Open Unity Hub → **Add project from disk** → select this folder
4. Let Unity import packages on first open (URP, Input System, 2D feature set)
5. Open `Assets/_Project/Scenes/Chapter1/Chapter1.unity` and press **Play**
6. Touch controls render via runtime uGUI (Left/Right/Jump/Interact/Use Key)

### iOS Build

**From Unity Editor (macOS):**

1. **File → Build Settings → iOS → Switch Platform**
2. **LockeKey → Build → iOS** (or run `tools/build_ios.sh`)
3. Open the generated Xcode project in `Builds/iOS/` and deploy to device

**From terminal (macOS with Unity 6000.5.2f1):**

```bash
export UNITY_PATH="/Applications/Unity/Hub/Editor/6000.5.2f1/Unity.app/Contents/MacOS/Unity"
tools/build_ios.sh
```

Bundle ID: `com.lockekeystudio.keyhouse` · Target iOS 13.0+

### Pre-commit Verification

**Linux (no Unity editor):** catches most asset/meta/scene reference issues:

```bash
tools/verify_linux.sh static
# or: python3 tools/verify_static.py
```

**Linux (with Unity editor or Docker):** adds script compile + asset import:

```bash
tools/verify_linux.sh unity          # native Linux Unity 6000.5.2f1
tools/verify_linux_docker.sh         # GameCI Docker image (needs license)
```

**macOS (required for iOS):**

```bash
export UNITY_PATH="/Applications/Unity/Hub/Editor/6000.5.2f1/Unity.app/Contents/MacOS/Unity"
tools/verify_unity.sh project   # compile, import, key assets
tools/verify_unity.sh ios       # full iOS Xcode project build (slower)
```

| Check | Linux static | Linux Unity | macOS iOS build |
|-------|-------------|-------------|-----------------|
| GUID / meta validity | yes | yes | yes |
| Scene/asset GUID refs | yes | yes | yes |
| C# compile | no | yes | yes |
| Asset import | no | yes | yes |
| iOS Xcode project | no | no | yes |

Optional git hook (macOS, requires Unity):

```bash
tools/install_git_hooks.sh
```

### First Open Notes

- Unity may regenerate `Packages/packages-lock.json` and some ProjectSettings — commit those changes.
- Legacy Input Manager is used (`activeInputHandler: 0`) for broad Editor compatibility.
- `EventBus` ScriptableObject lives at `Assets/_Project/Resources/EventBus.asset`.

### Art Assets

Chapter 1 sprites live in `Assets/_Project/Art/Sprites/` (AI-generated per Art Bible). Manifest: `Assets/_Project/Art/ai_assets_manifest.json`.

Source JPEGs (`1.jpg`–`16.jpg`) are bundled in `tools/art_sources/`. Override with `LOCKE_ART_SOURCES` if you keep originals elsewhere.

To re-import after regenerating source images:

```bash
python3 tools/import_art_assets.py
python3 tools/patch_chapter1_sprites.py
python3 tools/import_extended_art.py      # normal maps, UI icons, parallax
python3 tools/import_ui_vfx_assets.py     # key slots, VFX sheets, memory panels
python3 tools/patch_chapter1_parallax.py
```

**Art folders:** `Sprites/`, `NormalMaps/`, `Parallax/`, `UI/KeySlots/`, `VFX/`, `Memory/Panels/` · HUD loads from `Resources/Art/UI/` (`UIIconLibrary`, `KeySlotLibrary`)

### Chapter 1 Play Flow (Storyboard Beats)

1. **Arrival** — move toward the glinting **house key** (movement tutorial)
2. **Stuck door** — unlock foyer door (rattle if no key; warm light on open)
3. **Library** — push bookshelf **3 times** → Ghost Key appears in alcove
4. **Ghost Key use** — stand at sealed door, tap **Use Key**, phase through (VFX + vignette)
5. **Echo encounter** — hide behind the arch or escape through the passage
6. Claim the **Head Key** → interact with the **family portrait** for a memory teaser

## Core Architecture

- **IKeyAbility** — Interface all keys implement
- **EventBus** — Decoupled communication via ScriptableObject channels
- **PuzzleBase** — Base class for all environmental puzzles

## Art & Asset Pipeline

- Primary style locked via Art Bible (Midjourney)
- Consistent generation using **Scenario.gg**
- In-editor textures via Unity AI
- Final polish in Aseprite

## Next Milestones

1. [x] Import first environment art pass per Art Bible
2. [x] Replace placeholder sprites and tune 2D lighting (automated 4-layer Parallax)
3. [x] Puzzle 5 (hidden key) and expanded Echo AI
4. [ ] Ship TestFlight build from Xcode / physical device deployment

## License & IP

This is a fan project / prototype. All rights to *Locke & Key* belong to Joe Hill and Gabriel Rodríguez / IDW Publishing.

---

Built with passion for the source material.