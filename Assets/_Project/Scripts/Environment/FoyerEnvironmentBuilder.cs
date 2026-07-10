using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Dense gothic foyer dressing while keeping a clear walkable corridor (y ≈ -1.5 floor line).
/// Props sit on sides / walls / ceiling so the center path stays free.
/// </summary>
public class FoyerEnvironmentBuilder : MonoBehaviour
{
    private static Sprite disc;
    private static Sprite square;

    private void Awake()
    {
        // Rebuild richer set each session if an older sparse set exists
        var old = GameObject.Find("FoyerEnvironment");
        if (old != null) Destroy(old);
        var old2 = GameObject.Find("FoyerProps");
        if (old2 != null) Destroy(old2);

        disc ??= SoftDisc(48);
        square ??= SoftSquare(32);
        Build();
    }

    private void Build()
    {
        var root = new GameObject("FoyerEnvironment");

        // ── Floor: rugs along path edges (not blocking center) ──
        Prop(root, "RugMain", disc, new Vector3(0.2f, -1.48f, 0.12f), new Vector3(2.4f, 0.42f, 1f),
            new Color(0.38f, 0.14f, 0.12f, 0.55f), 2);
        Prop(root, "RugSideL", square, new Vector3(-3.2f, -1.45f, 0.12f), new Vector3(1.4f, 0.3f, 1f),
            new Color(0.28f, 0.12f, 0.1f, 0.5f), 2);
        Prop(root, "RugSideR", square, new Vector3(3.0f, -1.45f, 0.12f), new Vector3(1.3f, 0.3f, 1f),
            new Color(0.26f, 0.12f, 0.1f, 0.5f), 2);

        // ── Left: console table + candles + plant ──
        Prop(root, "TableL", square, new Vector3(-3.4f, -0.9f, 0.2f), new Vector3(1.15f, 0.5f, 1f),
            new Color(0.3f, 0.18f, 0.1f, 0.95f), 5);
        Prop(root, "BrokenTable", square, new Vector3(-4.0f, -1.15f, 0.18f), new Vector3(0.9f, 0.25f, 1f),
            new Color(0.25f, 0.15f, 0.08f, 0.85f), 4);
        Prop(root, "BrokenLeg", square, new Vector3(-3.7f, -1.0f, 0.18f), new Vector3(0.12f, 0.55f, 1f),
            new Color(0.22f, 0.12f, 0.08f, 0.9f), 4);
        var c1 = Prop(root, "CandleL1", disc, new Vector3(-3.55f, -0.45f, 0.25f), new Vector3(0.14f, 0.32f, 1f),
            new Color(1f, 0.9f, 0.55f, 0.95f), 6);
        AddLight(c1.transform, new Color(1f, 0.7f, 0.35f), 0.7f, 2.0f);
        var c2 = Prop(root, "CandleL2", disc, new Vector3(-3.2f, -0.4f, 0.25f), new Vector3(0.12f, 0.28f, 1f),
            new Color(1f, 0.88f, 0.5f, 0.9f), 6);
        AddLight(c2.transform, new Color(1f, 0.68f, 0.3f), 0.55f, 1.6f);
        Prop(root, "PlantPot", square, new Vector3(-4.3f, -1.2f, 0.2f), new Vector3(0.32f, 0.32f, 1f),
            new Color(0.32f, 0.2f, 0.14f, 0.95f), 5);
        Prop(root, "PlantLeaves", disc, new Vector3(-4.3f, -0.65f, 0.22f), new Vector3(0.75f, 0.9f, 1f),
            new Color(0.16f, 0.35f, 0.2f, 0.88f), 6);

        // ── Right: bookshelf + cabinet ──
        Prop(root, "Bookshelf", square, new Vector3(3.6f, 0.1f, 0.2f), new Vector3(1.0f, 2.2f, 1f),
            new Color(0.24f, 0.14f, 0.09f, 0.95f), 4);
        Prop(root, "Books1", square, new Vector3(3.55f, 0.55f, 0.21f), new Vector3(0.75f, 0.35f, 1f),
            new Color(0.45f, 0.2f, 0.15f, 0.9f), 5);
        Prop(root, "Books2", square, new Vector3(3.55f, 0.05f, 0.21f), new Vector3(0.7f, 0.3f, 1f),
            new Color(0.2f, 0.25f, 0.4f, 0.9f), 5);
        Prop(root, "Books3", square, new Vector3(3.55f, -0.45f, 0.21f), new Vector3(0.72f, 0.28f, 1f),
            new Color(0.35f, 0.3f, 0.15f, 0.9f), 5);
        Prop(root, "CabinetR", square, new Vector3(2.7f, -0.55f, 0.2f), new Vector3(0.85f, 1.15f, 1f),
            new Color(0.26f, 0.15f, 0.1f, 0.95f), 5);

        // ── Wall paintings ──
        Frame(root, "PortraitA", new Vector3(-1.8f, 1.2f, 0.3f), 0.7f, 0.9f,
            new Color(0.22f, 0.25f, 0.3f, 0.95f));
        Frame(root, "PortraitB", new Vector3(1.2f, 1.35f, 0.3f), 0.6f, 0.8f,
            new Color(0.28f, 0.2f, 0.22f, 0.95f));
        Frame(root, "PortraitC", new Vector3(0.1f, 1.55f, 0.3f), 0.5f, 0.65f,
            new Color(0.2f, 0.22f, 0.28f, 0.9f));

        // ── Stained glass window glows (sides, not path) ──
        Prop(root, "StainGlassL", square, new Vector3(-4.5f, 1.5f, 0.08f), new Vector3(0.9f, 1.6f, 1f),
            new Color(0.45f, 0.25f, 0.65f, 0.35f), 1);
        Prop(root, "StainGlassL2", square, new Vector3(-4.5f, 1.5f, 0.09f), new Vector3(0.7f, 1.3f, 1f),
            new Color(0.9f, 0.55f, 0.2f, 0.2f), 2);
        Prop(root, "StainGlassR", square, new Vector3(4.4f, 1.4f, 0.08f), new Vector3(0.85f, 1.5f, 1f),
            new Color(0.2f, 0.45f, 0.7f, 0.32f), 1);
        AddLightAt(root.transform, "WindowLightL", new Vector3(-4.3f, 1.4f, 0f),
            new Color(0.7f, 0.4f, 0.9f), 0.45f, 3f);
        AddLightAt(root.transform, "WindowLightR", new Vector3(4.2f, 1.3f, 0f),
            new Color(0.35f, 0.55f, 0.95f), 0.4f, 2.8f);

        // ── Wall sconces ──
        Sconce(root, "SconceL1", new Vector3(-2.2f, 0.85f, 0.25f));
        Sconce(root, "SconceL2", new Vector3(-0.6f, 1.0f, 0.25f));
        Sconce(root, "SconceR1", new Vector3(1.8f, 0.9f, 0.25f));
        Sconce(root, "SconceR2", new Vector3(3.2f, 0.95f, 0.25f));

        // ── Chandelier (ceiling, center — high enough to not block walk) ──
        Prop(root, "ChandelierChain", square, new Vector3(0.15f, 2.5f, 0.15f), new Vector3(0.08f, 0.7f, 1f),
            new Color(0.35f, 0.32f, 0.28f, 0.8f), 7);
        var chand = Prop(root, "Chandelier", disc, new Vector3(0.15f, 2.05f, 0.15f), new Vector3(1.1f, 0.55f, 1f),
            new Color(0.55f, 0.45f, 0.25f, 0.75f), 8);
        AddLight(chand.transform, new Color(1f, 0.82f, 0.5f), 1.15f, 4.5f);
        Prop(root, "ChandelierGlow", disc, new Vector3(0.15f, 2.0f, 0.14f), new Vector3(1.6f, 1.0f, 1f),
            new Color(1f, 0.85f, 0.5f, 0.12f), 6);

        // ── Cobwebs (corners) ──
        Prop(root, "CobwebTL", disc, new Vector3(-4.2f, 2.3f, 0.1f), new Vector3(1.2f, 0.9f, 1f),
            new Color(0.85f, 0.85f, 0.9f, 0.18f), 3);
        Prop(root, "CobwebTR", disc, new Vector3(4.1f, 2.25f, 0.1f), new Vector3(1.1f, 0.85f, 1f),
            new Color(0.85f, 0.85f, 0.9f, 0.16f), 3);
        Prop(root, "CobwebShelf", disc, new Vector3(3.5f, 1.0f, 0.22f), new Vector3(0.6f, 0.4f, 1f),
            new Color(0.9f, 0.9f, 0.95f, 0.14f), 6);

        // ── Molding strips ──
        Prop(root, "MoldingTop", square, new Vector3(0f, 2.65f, 0.05f), new Vector3(10f, 0.12f, 1f),
            new Color(0.35f, 0.25f, 0.15f, 0.55f), 2);
        Prop(root, "MoldingMid", square, new Vector3(0f, 0.35f, 0.05f), new Vector3(10f, 0.08f, 1f),
            new Color(0.3f, 0.22f, 0.14f, 0.4f), 2);

        // ── Floor shadow band (depth, not obstacle) ──
        Prop(root, "FloorShade", disc, new Vector3(0f, -1.55f, 0.08f), new Vector3(9f, 0.3f, 1f),
            new Color(0f, 0f, 0f, 0.16f), 1);
    }

    private static void Frame(GameObject root, string name, Vector3 pos, float w, float h, Color art)
    {
        Prop(root, name + "Frame", square, pos, new Vector3(w + 0.12f, h + 0.12f, 1f),
            new Color(0.38f, 0.28f, 0.12f, 0.95f), 3);
        Prop(root, name, square, pos + new Vector3(0f, 0f, 0.01f), new Vector3(w, h, 1f), art, 4);
    }

    private static void Sconce(GameObject root, string name, Vector3 pos)
    {
        Prop(root, name + "Base", square, pos, new Vector3(0.18f, 0.35f, 1f),
            new Color(0.4f, 0.32f, 0.2f, 0.95f), 5);
        var flame = Prop(root, name + "Flame", disc, pos + new Vector3(0f, 0.28f, 0f),
            new Vector3(0.2f, 0.28f, 1f), new Color(1f, 0.75f, 0.35f, 0.9f), 6);
        AddLight(flame.transform, new Color(1f, 0.7f, 0.35f), 0.65f, 1.9f);
        flame.AddComponent<FlickerLight>();
    }

    private static GameObject Prop(GameObject parent, string name, Sprite sprite, Vector3 pos, Vector3 scale, Color color, int order)
    {
        var go = new GameObject(name, typeof(SpriteRenderer));
        go.transform.SetParent(parent.transform);
        go.transform.position = pos;
        go.transform.localScale = scale;
        var sr = go.GetComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        sr.sortingOrder = order;
        return go;
    }

    private static void AddLight(Transform parent, Color color, float intensity, float radius)
    {
        var go = new GameObject("Light");
        go.transform.SetParent(parent);
        go.transform.localPosition = Vector3.zero;
        var light = go.AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.pointLightInnerRadius = 0.08f;
        light.pointLightOuterRadius = radius;
    }

    private static void AddLightAt(Transform parent, string name, Vector3 pos, Color color, float intensity, float radius)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.position = pos;
        var light = go.AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.pointLightOuterRadius = radius;
    }

    private static Sprite SoftDisc(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var mid = (size - 1) * 0.5f;
        for (var y = 0; y < size; y++)
        for (var x = 0; x < size; x++)
        {
            var d = Vector2.Distance(new Vector2(x, y), new Vector2(mid, mid)) / mid;
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Pow(Mathf.Clamp01(1f - d), 1.35f)));
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    private static Sprite SoftSquare(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        for (var y = 0; y < size; y++)
        for (var x = 0; x < size; x++)
        {
            float edge = Mathf.Min(x, y, size - 1 - x, size - 1 - y) / (size * 0.5f);
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(edge * 2.4f)));
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    private class FlickerLight : MonoBehaviour
    {
        private Light2D light;
        private float baseI;
        private void Start()
        {
            light = GetComponentInChildren<Light2D>();
            if (light != null) baseI = light.intensity;
        }
        private void Update()
        {
            if (light == null) return;
            light.intensity = baseI * (0.85f + Mathf.PerlinNoise(Time.time * 3.5f, 0.2f) * 0.3f);
        }
    }
}
