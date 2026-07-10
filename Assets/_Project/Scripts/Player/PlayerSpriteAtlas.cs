using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Runtime sprite-atlas loader for 2.5D character animation.
/// Slices Resources textures into ordered frame arrays (works without Editor multi-sprite import).
/// </summary>
public static class PlayerSpriteAtlas
{
    private static readonly Dictionary<string, Sprite[]> Cache = new();
    private static readonly Dictionary<string, Sprite> SingleCache = new();

    public static Sprite[] LoadGrid(string resourcesPath, int columns, int rows, float pixelsPerUnit = 220f)
    {
        var key = $"{resourcesPath}|{columns}x{rows}|{pixelsPerUnit}";
        if (Cache.TryGetValue(key, out var cached) && cached != null && cached.Length > 0)
            return cached;

        var tex = Resources.Load<Texture2D>(resourcesPath);
        if (tex == null)
        {
            Cache[key] = System.Array.Empty<Sprite>();
            return Cache[key];
        }

        // Prefer readable copy — imported textures may not be readable
        var readable = MakeReadable(tex);
        int cellW = readable.width / columns;
        int cellH = readable.height / rows;
        var frames = new List<Sprite>(columns * rows);

        for (var row = 0; row < rows; row++)
        {
            // Top-to-bottom rows (standard sheet reading)
            int y = readable.height - (row + 1) * cellH;
            for (var col = 0; col < columns; col++)
            {
                int x = col * cellW;
                var rect = new Rect(x, y, cellW, cellH);
                // Bottom-center pivot for grounded character feet
                var spr = Sprite.Create(readable, rect, new Vector2(0.5f, 0.05f), pixelsPerUnit, 0, SpriteMeshType.FullRect);
                spr.name = $"{tex.name}_{row}_{col}";
                frames.Add(spr);
            }
        }

        Cache[key] = frames.ToArray();
        return Cache[key];
    }

    public static Sprite LoadSingle(string resourcesPath, float pixelsPerUnit = 220f)
    {
        if (SingleCache.TryGetValue(resourcesPath, out var s) && s != null)
            return s;

        var existing = Resources.Load<Sprite>(resourcesPath);
        if (existing != null)
        {
            SingleCache[resourcesPath] = existing;
            return existing;
        }

        var tex = Resources.Load<Texture2D>(resourcesPath);
        if (tex == null) return null;

        var readable = MakeReadable(tex);
        var spr = Sprite.Create(
            readable,
            new Rect(0, 0, readable.width, readable.height),
            new Vector2(0.5f, 0.05f),
            pixelsPerUnit,
            0,
            SpriteMeshType.FullRect);
        SingleCache[resourcesPath] = spr;
        return spr;
    }

    private static Texture2D MakeReadable(Texture2D source)
    {
        if (source == null) return null;
        // Always blit — Resources textures are often non-readable after import.
        var rt = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        var prev = RenderTexture.active;
        Graphics.Blit(source, rt);
        RenderTexture.active = rt;
        var copy = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        copy.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        copy.Apply(false, false);
        copy.filterMode = FilterMode.Bilinear;
        copy.wrapMode = TextureWrapMode.Clamp;
        copy.name = source.name + "_readable";
        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);
        return copy;
    }
}
