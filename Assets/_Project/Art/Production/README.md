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

## Status
Chapter 1 vertical slice assets installed. Expand packs chapter-by-chapter using the same prompt bible.
