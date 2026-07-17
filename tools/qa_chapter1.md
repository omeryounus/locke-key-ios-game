# Chapter 1 QA Script — Phase A–F Gates

**Build:** Mac/iOS standalone from `tools/build_ios.sh`  
**Profile:** Fresh install + returning player save  
**Date:** ___________ **Tester:** ___________

## Gate B — Title flow (S0 + S1)

| # | Step | Expected | Pass |
|---|------|----------|------|
| B1 | Launch fresh profile | S0 splash shows **Enter Keyhouse** (not Chapter 1) | ☐ |
| B2 | Tap Enter Keyhouse | S1 reel: 4 slides, Skip / Next / final **Enter Keyhouse** | ☐ |
| B3 | Complete reel | `hasCompletedOnboarding=true`, loads Chapter1 map | ☐ |
| B4 | Relaunch app | Title shows Continue/New Game; **no S1 flash** | ☐ |
| B4b | Tap Continue | Opens Chapter1 **map directly** (no foyer flash) | ☐ |

## Gate C — Map (S2 + S8)

| # | Step | Expected | Pass |
|---|------|----------|------|
| C1 | On map | 3 cards: Foyer, Wellhouse, Black Door; X/13 keys | ☐ |
| C2 | Tap Black Door | Toast 2–3s: no keyhole message | ☐ |
| C3 | Tap locked Wellhouse | Toast: locked message | ☐ |
| C4 | Foyer card | Loads foyer gameplay | ☐ |
| C5 | Solve stair door + reload map | Foyer ✓, Wellhouse unlocked | ☐ |
| C6 | Replay Story | Returns to S1; **keys preserved** | ☐ |
| C7 | Settings | Settings stub opens and closes | ☐ |
| C8 | Codex | Manor Chronicles panel opens | ☐ |

## Gate D — Gameplay HUD (S3)

| # | Step | Expected | Pass |
|---|------|----------|------|
| D1 | In foyer | HUD: Map / room title / Key buttons | ☐ |
| D2 | Map → Foyer round-trip | Save state intact | ☐ |

## Gate E — Discovery + lock + ring (S4–S6)

| # | Step | Expected | Pass |
|---|------|----------|------|
| E1 | Floorboard house key | S4 sheet; Add / Add&Equip writes save | ☐ |
| E2 | Stair door (no key) | S5 lock; Try Key disabled | ☐ |
| E3 | Equip Anywhere + Try Key | Success → map; Wellhouse unlocks | ☐ |
| E4 | Key ring | 3×5 grid, equip shows gold ring | ☐ |
| E5 | Ghost key (if in scene) | S4 discovery pattern | ☐ |
| E6 | Head key (if in scene) | S4 discovery pattern | ☐ |

## Gate A — Foundation (editor)

| # | Step | Expected | Pass |
|---|------|----------|------|
| A1 | Delete save, relaunch | Foyer only, 0 keys; save survives restart | ☐ |
| A2 | Resources load | `Art/Keys/key_anywhere` sprite loads | ☐ |
| A3 | Landscape | Centered 393×852 viewport | ☐ |
| A4 | Menu: Spawn A4 Test Canvas | Toast + Hello button, zero missing refs | ☐ |

## Gate F — Echo encounter (S7)

| # | Step | Expected | Pass |
|---|------|----------|------|
| F1 | Open the sealed passage | Echo appears and objective says to escape | ☐ |
| F2 | Stay in the encounter for 20 seconds without entering the passage | Encounter remains active; it does not advance to aftermath | ☐ |
| F3 | Let the Echo catch the player | Player returns to the checkpoint, receives a short input lock, then Echo respawns | ☐ |
| F4 | Cross the passage exit | Echo despawns, tension drops, aftermath begins, and save marks encounter cleared | ☐ |

## Notes

```
Device / OS:
Build hash:
Blockers:
```
