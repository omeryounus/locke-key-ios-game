# Vertical Slice Specification - Chapter 1: "Welcome to Keyhouse"

**Version:** 1.0  
**Target Duration:** 8–10 weeks  
**Goal:** Prove the core gameplay fantasy (exploration + key usage + puzzle solving + light horror) in a polished, shippable slice.

## 1. Chapter Overview

**Title:** Welcome to Keyhouse  
**Focus Key:** Ghost Key (primary) + introduction to Head Key  
**Tone:** Mysterious, slightly unsettling, wonder mixed with dread  
**Length:** 25–40 minutes of gameplay

**Narrative Goal:**
Introduce the Locke siblings, establish Keyhouse as a living, strange place, and give the player their first taste of the power (and danger) of the keys.

## 2. Playable Areas (Vertical Slice Scope)

### Area 1: Front Hall & Foyer
- Main entrance
- Grand staircase
- Locked doors leading to other wings
- First environmental storytelling (old photos, letters)

### Area 2: Living Room & Library
- Fireplace area
- Bookshelves with clues
- First simple puzzle

### Area 3: Upstairs Hallway & Bode's Room
- Introduction to Ghost Key
- First real puzzle using the key
- Light horror moment (Echo encounter)

### Area 4: Hidden Passage (Stretch Goal)
- Small secret area unlocked with Ghost Key

**Total Unique Rooms:** 5–6 rooms

## 3. Key Progression in Chapter 1

| Key          | When Found     | Primary Use in Chapter 1                  | Risk Introduced? |
|--------------|----------------|-------------------------------------------|------------------|
| **Ghost Key**   | Mid-chapter    | Phase through walls/doors                 | Yes (Echoes)     |
| **Head Key**    | Late chapter   | View memories (tutorial only)             | Teased           |

## 4. Puzzles in Chapter 1 (Target: 5–6 Puzzles)

### Puzzle 1: The Stuck Door (Tutorial)
- **Type:** Environmental interaction
- **Solution:** Find and use a normal key first (teaching basic interaction)

### Puzzle 2: The Collapsed Bookshelf
- **Type:** Physics / environmental
- **Solution:** Push or move objects (basic interaction)

### Puzzle 3: The Sealed Door (Ghost Key Introduction)
- **Type:** Key ability puzzle
- **Solution:** Use Ghost Key to phase through the door
- **Learning Goal:** Teach key activation + risk (attracts a small Echo)

### Puzzle 4: Memory Fragment (Head Key Teaser)
- **Type:** Narrative / light puzzle
- **Solution:** Use Head Key on a specific object or character to see a short memory
- **Purpose:** Introduce second key and deeper lore

### Puzzle 5: The Hidden Key (Optional)
- **Type:** Exploration + observation
- **Solution:** Use Ghost Key + observation to find a hidden key

### Puzzle 6: First Echo Encounter (Horror Moment)
- **Type:** Avoidance / tension
- **Solution:** Use Ghost Key creatively to escape or hide

## 5. Core Systems Required for Vertical Slice

- [ ] Player Controller (2.5D movement + interaction)
- [ ] KeyManager + IKeyAbility system
- [ ] GhostKey implementation
- [ ] Basic HeadKey (memory viewing)
- [ ] PuzzleBase + at least 3 concrete puzzles
- [ ] EventBus integration
- [ ] Simple Save System (chapter progress)
- [ ] Basic UI (Key selection wheel or hotbar)
- [ ] 2D Lighting + colored key glows
- [ ] Simple Echo enemy (basic AI or scripted event)

## 6. Narrative Beats (Key Moments)

1. **Arrival** — Family enters Keyhouse, tension between siblings
2. **First Discovery** — Player finds the first normal key + locked door
3. **Ghost Key Moment** — Dramatic reveal + first use
4. **First Horror** — Brief Echo encounter (teaches consequence)
5. **Head Key Tease** — Player sees a memory that hints at deeper mystery
6. **Chapter Cliffhanger** — Something important is revealed or a new threat appears

## 7. Technical & Art Requirements

- All core mechanics must feel responsive and satisfying on touch
- Keys must have clear visual feedback (glow + particle effects)
- Puzzles must communicate clearly what the player needs to do
- Performance target: 60 FPS on iPhone 13 and newer
- Use 2D URP lights and shadows heavily

## 8. Success Criteria for Vertical Slice

The Vertical Slice will be considered successful if:
- A new player can understand the core loop within 5 minutes
- The Ghost Key feels magical and meaningful
- There is at least one memorable "wow" moment
- The game runs smoothly on mobile with good touch controls
- The atmosphere is strong and consistent with the Art Bible

## 9. Out of Scope for Vertical Slice
- Full voice acting
- Complex combat
- Multiple chapters
- Advanced consequence system
- Full inventory
- Polished animations (placeholder is acceptable)

---

**This document defines exactly what we will build first.**