# Locke & Key - Art Bible

**Version:** 1.0  
**Date:** July 2026  
**Purpose:** Define visual style, mood, and consistency rules for all assets.

## 1. Overall Visual Direction

**Style Keywords:**
- Atmospheric horror
- Dark fantasy
- Hand-painted / illustrated feel with strong lighting
- Slightly stylized but grounded (not cartoonish)
- Heavy use of contrast, deep shadows, and colored light sources

**Inspiration References:**
- Gabriel Rodríguez's original comic art
- Little Nightmares series (lighting + silhouette)
- The Room series (detailed object interaction + atmosphere)
- What Remains of Edith Finch (environmental storytelling)
- Dark Souls / Bloodborne environmental mood (without the grimdark excess)

**Core Visual Pillars:**
1. **Keyhouse feels alive and slightly wrong** (shifting perspective, impossible architecture in places)
2. **Light is the star** — Keys glow, windows cast dramatic beams, candles flicker
3. **Texture and material clarity** — Wood, stone, fabric, metal should read clearly even in low light
4. **Silhouette readability** important for 2.5D gameplay

## 2. Color Palette

**Primary Palette:**
- Deep desaturated blues and teals (#0A1628, #1A2A3A)
- Warm key glows (Purple for Ghost, Amber/Orange for Head, Green for Shadow)
- Muted wood browns and stone grays
- Accents of deep red and sickly yellow for horror moments

**Lighting Philosophy:**
- Most scenes are dimly lit with strong key lights from windows or candles
- Colored rim lights on keys and important objects
- Strong use of 2D lights + shadows in Unity URP

## 3. Asset Categories & Generation Guidelines

### A. Environments (Keyhouse)
- Walls, floors, ceilings with normal maps for texture
- Tileable stone, wood planks, wallpaper, carpet
- Large set pieces (staircases, fireplaces, bookshelves)

### B. Keys (Hero Assets)
- Each key should have a distinct silhouette and colored glow
- Ghost Key: Ethereal, slightly transparent, purple glow
- Head Key: Metallic with brain-like engravings, warm orange glow
- Mirror Key: Reflective surface, green/teal accents

### C. Characters
- Locke siblings (Tyler, Kinsey, Bode) — expressive but not overly cartoony
- Supporting characters (Nina, Duncan, etc.)
- Demonic Echoes — distorted, shadowy, high-contrast silhouettes

### D. UI
- Clean, slightly aged paper aesthetic
- Key icons with glow
- Inventory and interaction prompts should feel tactile

## 4. Midjourney / Flux Prompt Templates

### Environment Prompt Template
```
 atmospheric interior of an old mysterious mansion, dark wood paneling, heavy curtains, dramatic window light beams, dust particles, moody cinematic lighting, highly detailed, painterly style, Gabriel Rodriguez inspired --stylize 250 --v 6
```

### Key Prompt Template
```
 magical ornate key floating in darkness, [color] ethereal glow, intricate engravings, fantasy key design, dramatic rim lighting, dark background, highly detailed, cinematic --stylize 200
```

### Character Prompt Template
```
 young teenager in old mansion, moody atmospheric lighting, detailed clothing, expressive face, painterly style, Gabriel Rodriguez comic influence, cinematic composition --stylize 180
```

## 5. Scenario.gg Training Recommendations

**Best Practice:**
1. Generate 15–20 high-quality images in Midjourney that perfectly represent the desired style.
2. Upload them to Scenario.gg and train a custom model (LoRA).
3. Use the trained model for all future asset generation to maintain consistency across chapters.

**Training Tips:**
- Include a mix of environments, keys, and characters
- Use `--stylize` values between 150-300
- Keep `--v 6` or Flux for best results

## 6. Polish Pipeline

1. Generate base assets with Scenario / Midjourney
2. Import into Unity
3. Use Unity AI Texture Generator for variations and normal maps
4. Final cleanup and animation frames in **Aseprite**
5. Document all AI-generated assets in `ai_assets_manifest.json`

## 7. Technical Requirements for Unity

- All sprites should support **Normal Maps** and **Emission Maps**
- Keys should use **2D Lights** with colored halos
- Parallax layers for background depth
- Tileable textures for walls and floors

---

## 8. Character Concept & Turnaround Guidelines

To ensure the animation team has consistent model sheets, all character assets must be designed or generated with multi-angle reference guides (front, side, and back views).

### A. Core Cast Visual Descriptions

* **Bode Locke (The Explorer)**:
  * **Age**: 6 years old.
  * **Aesthetic**: Energetic, naive, adventurous.
  * **Wardrobe**: Bright yellow hooded rain jacket, dark blue denim jeans, small red sneakers.
  * **Visual Traits**: Messy, light-brown hair; large, wide eyes expressing curiosity and wonder.
  * **Posture**: Leaning forward, hand in pockets or pointing, ready to run.

* **Kinsey Locke (The Protectors/Creative)**:
  * **Age**: 15 years old.
  * **Aesthetic**: Expressive, guarded, slightly alternative.
  * **Wardrobe**: Muted grey knit sweater over a dark collared shirt, black trousers, combat boots.
  * **Visual Traits**: Dark braided hair tied back; cautious and alert facial expression.
  * **Posture**: Folded arms or hands held near chest, defensively alert.

* **Tyler Locke (The Eldest)**:
  * **Age**: 17 years old.
  * **Aesthetic**: Athletic, weary, protective.
  * **Wardrobe**: Red-and-black checkered flannel jacket, classic blue jeans, leather work boots.
  * **Visual Traits**: Short, dark, structured hair; worried brows, stern jawline.
  * **Posture**: Tall, broad-shouldered, protective stance.

* **Dodge / The Echo (The Antagonist)**:
  * **Aesthetic**: Sinister, alluring, otherworldly.
  * **Wardrobe**: Long, flowing dark velvet coat with high collar, dark trousers.
  * **Visual Traits**: Sleek pitch-black hair; pale skin; eyes that reflect a faint purple light from within.
  * **Posture**: Tall, upright, fluid posture with shadow-like visual artifacts bleeding from the edges of the silhouette.

### B. Turnaround Prompt Template (Midjourney / Flux)
When generating character model reference sheets, use the following layout parameters:
```
model sheet, character turnaround, front view, side view, back view, orthographic projection, [Character Description from above], Gabriel Rodriguez comic illustration style, clean white background, dark fantasy lighting accents, high contrast --stylize 120 --v 6
```

---

## 9. Technical Sprite & Resolution Standards

To maintain sharp rendering without causing high memory pressure on mobile devices, all assets must conform to these standard constraints:

### A. Target Resolutions & Aspect Ratios

| Asset Type | Dimension (WxH in Pixels) | Aspect Ratio | Use Case |
| :--- | :--- | :--- | :--- |
| **Parallax Backgrounds** | $2732 \times 2048$ | $4:3$ (Native iPad Pro) | Distant scenery layers |
| **Midground Environments**| $2436 \times 1125$ | $19.5:9$ (iPhone Screen) | Playable room bounds |
| **Character Sprites** | $512 \times 768$ | $2:3$ | Standing idle and movement sheets |
| **Major Interactables** | $512 \times 512$ | $1:1$ | Bookshelves, fireplaces, cabinets |
| **Key & UI Icons** | $256 \times 256$ | $1:1$ | HUD icons, key wheel assets |

### B. Unity Sprite Import Configuration Rules

For every graphic asset imported into the project, apply the following properties in the Inspector:
* **Texture Type**: `Sprite (2D and UI)`
* **Sprite Mode**: `Single` (for environment/props) or `Multiple` (for character/UI sheets)
* **Pixels Per Unit (PPU)**: Standardized to exactly **`100`** (ensures physics colliders and lighting scale linearly)
* **Mesh Type**: `Tight` (optimizes GPU transparent pixel rendering)
* **Filter Mode**: `Bilinear` (preserves the hand-painted, soft brush look)
* **Compression**: `ASTC 6x6` (or `ASTC 8x8` for background elements) to optimize iOS memory bandwidth

---

## 10. Mobile Touch UI Layouts & Safe Area Specs

Designing for modern iOS devices requires respecting physical constraints like the camera notch, Dynamic Island, and home indicator.

### A. Screen Margins & Safe Areas
* **Safe Zone Padding**: A minimum margin of **`44 pt`** (132 pixels on Retina @3x) must be kept clear of any interactive controls on all screen borders.
* **Anchor Points**: Use Unity Canvas Anchors to bind panels:
  * Left/Right movement controls: Anchored to **Bottom-Left**.
  * Action controls (Jump/Interact/Use): Anchored to **Bottom-Right**.
  * Inventory/Key Selection: Anchored to **Top-Right**.
  * Dialogue/Subtitle boxes: Anchored to **Bottom-Center** (floating above the home indicator safe zone).

### B. Mobile Layout Diagram

```
+-------------------------------------------------------------+
| [Back Menu]                                    [Key Selection] |
|                                                (Active Key)  |
|                                                              |
|                                                              |
|                        [Gameplay Area]                       |
|                                                              |
|                                                              |
|   < [Left]  [Right] >                     (Use Key)          |
|                                         (Interact) (Jump)    |
|                       [Dialogue Box]                         |
+-------------------------------------------------------------+
```

### C. Touch target sizes
* All buttons must have a minimum physical screen size of **`48 x 48 dp`** to prevent accidental touches.
* Buttons should fade out to $30\%$ opacity during movement to maintain high gameplay visibility, returning to $100\%$ when touched.

---

*This Art Bible must be followed strictly to maintain visual consistency.*