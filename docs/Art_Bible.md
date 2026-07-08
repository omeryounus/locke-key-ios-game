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

*This Art Bible must be followed strictly to maintain visual consistency.*