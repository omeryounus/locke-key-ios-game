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
- [x] Chapter 1 greybox vertical slice (Puzzles 1–4, Echo encounter, uGUI HUD)

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
python3 tools/patch_chapter1_parallax.py
```

**Art folders:** `Sprites/`, `NormalMaps/`, `Parallax/`, `UI/Icons/` · HUD icons load from `Resources/Art/UI/UIIconLibrary.asset`

### Chapter 1 Play Flow

1. Pick up the **house key** → unlock the **stuck door**
2. Claim the **Ghost Key** → push the **collapsed bookshelf**
3. Phase through the **sealed door** (Echo spawns)
4. Claim the **Head Key** → interact with the **family portrait** for a memory teaser

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

1. Import first environment art pass per Art Bible
2. Replace placeholder sprites and tune 2D lighting
3. Puzzle 5 (hidden key) and expanded Echo AI
4. Ship TestFlight build from Xcode

## License & IP

This is a fan project / prototype. All rights to *Locke & Key* belong to Joe Hill and Gabriel Rodríguez / IDW Publishing.

---

Built with passion for the source material.