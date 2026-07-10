# Production Art Library (Grok Imagine)

Dark fantasy / gothic painterly pack for *Locke & Key: Keyhouse Unlocked*.

## Style
- Art Bible v1.0 + Little Nightmares / The Room / Locke & Key
- Warm candle orange · blue moonlight · gold UI · purple magic · green spirit

## Layout
```
Production/
  Characters/Player/     high-res character sources
  Environment/Backgrounds/  2048px panoramic rooms
  Items/Keys/            hero keys
  Enemies/Echo/
  Props/Doors/
  UI/
  Effects/
  Manifests/
```

## Runtime
Game loads optimized copies from `Assets/_Project/Resources/Art/`.

## Regenerate / reinstall
Session images → `python3 tools/install_production_art.py [session/images]`

## Status — v3.0 packs installed

### 1. Props
`prop_candle`, `prop_chandelier`, `prop_bookshelf`, `prop_carpet`, `prop_broken_chair`, `prop_grandfather_clock`, `prop_portrait_frame`, front door

### 2. Echo frames
`echo_00` idle · `echo_01` attack · `echo_02` hurt · `echo_03` death

### 3. UI
`ui_atlas_full.jpg` — buttons, slots, dialogue frame kit

### 4. Chapter expansion BGs
Dining · Study · Attic · Cemetery (+ existing foyer/library/sealed/exterior/memory)

### 5. Layered character parts
`player_hood_cape` · `player_portrait` · `player_arms_reach` · full idle/walk/jump

Runtime copies: `Assets/_Project/Resources/Art/`  
Spawned in scene via `ProductionPropSpawner`.
