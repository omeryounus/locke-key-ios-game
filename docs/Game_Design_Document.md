# Locke & Key: Keyhouse Unlocked
## iOS Game Design Document (GDD) v0.1
**Date:** July 7, 2026  
**Project Lead:** [To be assigned]  
**Status:** Initial Concept & Foundation

---

## 1. Executive Summary / Vision

**Game Title Options:**
- *Locke & Key: Keyhouse Unlocked*
- *The Keys of Keyhouse*
- *Locke & Key: Echoes*

**Genre:** Narrative-Driven Puzzle Adventure with Light Supernatural Horror  
**Perspective:** 3rd-person exploration with fixed or dynamic camera angles (inspired by *What Remains of Edith Finch* + modern point-and-click elements)  
**Target Platforms:** iOS 17+, iPadOS 17+ (optimized for touch). Potential Apple Arcade or Premium purchase.  
**Target Audience:** Ages 13–35, fans of narrative adventures, mystery, horror, and the *Locke & Key* comics/Netflix series.  
**Tone:** Atmospheric, mysterious, emotional, occasionally terrifying, with moments of wonder and family drama.  
**Unique Selling Points (USPs):**
- Authentic adaptation of the magical key system as core interactive gameplay.
- Beautiful, comic-inspired art direction.
- Deep narrative with meaningful choices and consequences.
- Touch-native puzzle design that feels elegant on iPhone/iPad.
- Episodic or chapter-based structure perfect for mobile play sessions.

**High Concept:**  
You are one of the Locke siblings (or create a custom character in later modes). After moving into the ancestral Keyhouse, you discover a set of magical keys that grant extraordinary — and dangerous — powers. Use them to uncover family secrets, solve intricate environmental puzzles, protect your loved ones from ancient evils, and decide how far you are willing to go to save what matters most.

---

## 2. Core Gameplay Loop

**Primary Loop (per session/chapter):**
1. **Explore** the current area of Keyhouse (rooms, grounds, hidden passages).
2. **Discover** new keys or clues.
3. **Experiment** with key abilities to interact with the environment.
4. **Solve** narrative-integrated puzzles.
5. **Advance** the story through dialogue, investigation, and key moments.
6. **Manage** risk: Some key uses have consequences (attract demons, change the house, affect characters).

**Session Length:** 15–45 minutes per play session (ideal for mobile).

---

## 3. Key Mechanics & Abilities (Core Fantasy)

The game will feature **8–10 major keys** across the full experience. Each key has a primary ability + potential upgrades or risky secondary uses.

### Confirmed Starting Keys (Vertical Slice candidates):

| Key Name              | Primary Ability                              | Puzzle Examples                              | Risk / Horror Element                     | Narrative Tie-in                  |
|-----------------------|----------------------------------------------|----------------------------------------------|-------------------------------------------|-----------------------------------|
| **Ghost Key**        | Phase through walls/doors for short time    | Reach inaccessible areas, avoid traps       | Can get stuck in walls or attract "Echoes" | Used to escape danger            |
| **Head Key**         | Enter a person's mind / view memories       | Solve emotional/memory puzzles, learn secrets | Psychological horror, trauma exposure     | Deep character moments           |
| **Mirror Key**       | Travel through reflective surfaces          | Quick travel between rooms, find hidden paths | Mirror dimension is dangerous             | Family history & reflections     |
| **Anywhere Key**     | Create doors to other locations             | Connect distant areas creatively            | Uncontrolled use can summon demons        | Major story progression tool     |
| **Shadow Key**       | Manipulate/control shadows                  | Hide objects, create paths, distract enemies| Shadows can become hostile                | Defense against demons           |
| **Omega Key**        | Ultimate key (late game) – resurrection / reality alteration | Climactic moral choices                    | Extremely high risk / permanent changes   | Series climax                    |

**Design Principle:** Every key use should feel **magical and consequential**. Players should hesitate before using powerful keys.

---

## 4. Puzzle Design Philosophy

- **Environmental & Narrative Integration:** Puzzles are never arbitrary. They reveal character backstories or advance the plot.
- **Multiple Solutions:** Many puzzles support creative key combinations.
- **Touch-First:** Drag, tap, hold, swipe gestures feel natural.
- **Difficulty Curve:** Gentle onboarding → increasingly complex multi-key puzzles.
- **Horror Integration:** Some puzzles involve avoiding or outsmarting demonic entities ("Echoes" and "Demons").

**Puzzle Categories:**
- Spatial / Phasing puzzles (Ghost Key)
- Memory / Emotional deduction (Head Key)
- Connectivity & pathfinding (Mirror + Anywhere Keys)
- Light manipulation & stealth (Shadow Key)

---

## 5. Narrative Structure

**Recommended Structure:** 6 Chapters (matching major comic arcs or original condensed story)

**Chapter 1: "Welcome to Keyhouse"** (Vertical Slice target)
- Introduction to siblings and house.
- Discovery of first 2–3 keys.
- Tutorial puzzles + first horror encounter.

**Later Chapters:**
- Deepening family mysteries
- Introduction of the Lovecraftian entities
- Moral choices with lasting consequences
- Climactic decisions about the Omega Key

**Narrative Delivery:**
- Environmental storytelling (notes, drawings, echoes)
- Character conversations (fully voiced or high-quality text + portraits)
- Cinematic key moments
- Optional deeper investigation for lore lovers

---

## 6. Art & Visual Direction

**Primary Style:** Stylized 3D with strong 2D comic influences (cel-shading or hand-painted textures).  
**Inspiration:** Gabriel Rodríguez's comic art + modern games like *DREDGE*, *What Remains of Edith Finch*, *Oxenfree*.

**Key Visual Goals:**
- Keyhouse feels alive and slightly off (shifting architecture).
- Beautiful lighting and atmosphere.
- Clear visual language for interactive objects vs. background.
- Horror elements are suggested more than shown (psychological + grotesque when needed).

**Technical Art Targets (Unity):**
- Use URP (Universal Render Pipeline) for beautiful lighting on mobile.
- Optimized assets for iPhone 12+ / recent iPads.
- Strong use of post-processing for mood.

---

## 7. Technical Recommendations

**Recommended Engine:** **Unity 6** (or Unity 2022 LTS if stability preferred)
- Excellent iOS export, strong 2D/3D tools, large community, Asset Store.
- C# scripting is accessible.
- Good performance on mobile with proper optimization.

**Strong Alternative:** **Godot 4.3+**
- Completely free, lightweight, excellent 2D/3D.
- GDScript is very readable.
- Growing mobile support.

**Why not native Swift/SpriteKit?**
- Too limiting for 3D exploration and complex narrative systems. Better for very simple 2D games.

**Core Systems to Build:**
- Key Management System (inventory + ability activation)
- Interaction System (raycast or touch-based)
- Puzzle State Machine
- Narrative System (Ink or custom dialogue tree)
- Save System (cloud sync via iCloud)
- Horror / Tension System (dynamic music, lighting, entity AI)

---

## 8. Scope & Development Phases (Realistic Roadmap)

### Phase 0: Foundation (Current – 2–4 weeks)
- Finalize GDD
- Create vertical slice plan
- Choose engine & set up project
- Concept art & key ability prototypes (paper or simple digital)

### Phase 1: Vertical Slice (6–10 weeks)
- One fully playable chapter (e.g., first floor of Keyhouse + 3 keys)
- Core systems working: exploration, key usage, 4–5 puzzles
- Basic narrative + 1–2 characters
- Polish + playtesting

### Phase 2: Full Production (4–8 months depending on team size)
- All 6 chapters
- Full art, animation, sound, voice (or high-quality text)
- Additional keys, deeper systems, consequences
- QA, balancing, accessibility

### Phase 3: Polish, Certification & Launch
- Apple App Store guidelines
- Performance optimization
- Marketing assets (trailers, screenshots)
- Launch on App Store (Premium or Apple Arcade pitch)

**Team Size Recommendation (Indie/Small Studio):**
- 1–2 Designers
- 2–4 Programmers (Unity)
- 1–2 Artists
- 1 Narrative Writer
- Part-time Audio + QA

---

## 9. Monetization & Business Model

**Recommended Options (ranked):**
1. **Premium Purchase** ($6.99 – $12.99) — Best for narrative experience. One-time purchase, no IAP.
2. **Apple Arcade** — Excellent fit for high-quality narrative games. Steady revenue + visibility.
3. **Free + One-time Unlock** of full game after Chapter 1 (demo model).

Avoid aggressive IAP or gacha — it would damage the artistic integrity.

---

## 10. Risks & Mitigations

| Risk                        | Mitigation                                      |
|----------------------------|-------------------------------------------------|
| Scope creep                 | Strict vertical slice first + chapter gating   |
| Horror too intense for mobile | Tone options + psychological focus             |
| Performance on older devices | Strong optimization from day one               |
| Narrative writing quality   | Hire experienced writer or use Ink + iteration |
| Rights / Licensing          | Critical: Secure IP rights before major investment |

---

## 11. Immediate Next Steps (Action Items)

1. **Confirm Vision** — Reply with feedback on this GDD (scope, perspective, chapter count, art style preference).
2. **Choose Engine** — Unity 6 or Godot? I can help set up either.
3. **Create Vertical Slice Plan** — Detailed breakdown of Chapter 1.
4. **Prototype Core Systems** — I can generate initial Unity C# scripts or Godot scenes for:
   - Key activation system
   - Basic interaction
   - Simple puzzle example (Ghost Key phasing)
5. **Art Direction References** — Collect mood boards.

---

**Document Status:** Living document. Will be updated as we progress.

**Next Deliverable (upon your approval):** Detailed Vertical Slice Specification + first technical prototypes.

---

*This document is the foundation. We will build the complete game iteratively from here.*