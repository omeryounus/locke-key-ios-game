using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Runtime foyer dressing: rugs, tables, lamps, portraits, plants, and soft lamp lights
/// so the room never reads as an empty void.
/// </summary>
public class FoyerEnvironmentBuilder : MonoBehaviour
{
    private static Sprite softDisc;
    private static Sprite softSquare;

    private void Awake()
    {
        if (GameObject.Find("FoyerEnvironment") != null) return;
        Build();
    }

    private void Build()
    {
        softDisc ??= CreateSoftDisc(48);
        softSquare ??= CreateSoftSquare(32);

        var root = new GameObject("FoyerEnvironment");

        // Floor rug
        Prop(root, "RugMain", softDisc, new Vector3(0.3f, -1.45f, 0.15f), new Vector3(3.6f, 0.55f, 1f),
            new Color(0.42f, 0.16f, 0.14f, 0.7f), 2);
        Prop(root, "RugRunner", softSquare, new Vector3(-2.2f, -1.42f, 0.15f), new Vector3(1.8f, 0.28f, 1f),
            new Color(0.28f, 0.14f, 0.12f, 0.55f), 2);

        // Console table + lamp
        Prop(root, "Table", softSquare, new Vector3(-2.8f, -0.85f, 0.2f), new Vector3(1.1f, 0.55f, 1f),
            new Color(0.32f, 0.2f, 0.12f, 0.95f), 5);
        Prop(root, "TableLegL", softSquare, new Vector3(-3.15f, -1.15f, 0.2f), new Vector3(0.12f, 0.45f, 1f),
            new Color(0.22f, 0.14f, 0.08f, 0.95f), 4);
        Prop(root, "TableLegR", softSquare, new Vector3(-2.45f, -1.15f, 0.2f), new Vector3(0.12f, 0.45f, 1f),
            new Color(0.22f, 0.14f, 0.08f, 0.95f), 4);
        var lamp = Prop(root, "Lamp", softDisc, new Vector3(-2.8f, -0.35f, 0.25f), new Vector3(0.35f, 0.55f, 1f),
            new Color(0.95f, 0.82f, 0.45f, 0.85f), 6);
        AddPointLight(lamp.transform, "LampLight", new Color(1f, 0.75f, 0.4f), 1.1f, 2.8f);

        // Right side plant
        Prop(root, "PlantPot", softSquare, new Vector3(2.9f, -1.15f, 0.2f), new Vector3(0.35f, 0.35f, 1f),
            new Color(0.35f, 0.22f, 0.15f, 0.95f), 5);
        Prop(root, "PlantLeaves", softDisc, new Vector3(2.9f, -0.55f, 0.22f), new Vector3(0.85f, 0.95f, 1f),
            new Color(0.18f, 0.38f, 0.22f, 0.88f), 6);

        // Portraits / frames
        Prop(root, "PortraitFrameA", softSquare, new Vector3(-1.5f, 1.15f, 0.3f), new Vector3(0.75f, 0.95f, 1f),
            new Color(0.38f, 0.28f, 0.12f, 0.95f), 3);
        Prop(root, "PortraitA", softSquare, new Vector3(-1.5f, 1.15f, 0.31f), new Vector3(0.58f, 0.75f, 1f),
            new Color(0.25f, 0.28f, 0.32f, 0.95f), 4);
        Prop(root, "PortraitFrameB", softSquare, new Vector3(1.4f, 1.25f, 0.3f), new Vector3(0.65f, 0.85f, 1f),
            new Color(0.35f, 0.26f, 0.12f, 0.95f), 3);
        Prop(root, "PortraitB", softSquare, new Vector3(1.4f, 1.25f, 0.31f), new Vector3(0.5f, 0.68f, 1f),
            new Color(0.3f, 0.22f, 0.28f, 0.95f), 4);

        // Side cabinet
        Prop(root, "Cabinet", softSquare, new Vector3(2.5f, -0.55f, 0.2f), new Vector3(0.95f, 1.25f, 1f),
            new Color(0.26f, 0.16f, 0.1f, 0.95f), 5);
        Prop(root, "CabinetDoor", softSquare, new Vector3(2.5f, -0.5f, 0.21f), new Vector3(0.75f, 0.95f, 1f),
            new Color(0.32f, 0.2f, 0.12f, 0.9f), 6);

        // Window light shafts (spooky but readable)
        Prop(root, "WindowShaft", softDisc, new Vector3(-3.2f, 1.6f, 0.05f), new Vector3(1.4f, 2.2f, 1f),
            new Color(1f, 0.88f, 0.55f, 0.12f), 1);
        Prop(root, "WindowShaft2", softDisc, new Vector3(0.8f, 1.8f, 0.05f), new Vector3(1.0f, 1.8f, 1f),
            new Color(0.7f, 0.8f, 1f, 0.08f), 1);

        // Candle sticks near door approach
        var candle = Prop(root, "Candle", softDisc, new Vector3(0.9f, -0.7f, 0.25f), new Vector3(0.18f, 0.35f, 1f),
            new Color(1f, 0.9f, 0.55f, 0.9f), 6);
        AddPointLight(candle.transform, "CandleLight", new Color(1f, 0.7f, 0.35f), 0.75f, 2.2f);

        // Floor shadow pool under player path
        Prop(root, "FloorShade", softDisc, new Vector3(0f, -1.55f, 0.1f), new Vector3(8f, 0.35f, 1f),
            new Color(0f, 0f, 0f, 0.18f), 1);
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

    private static void AddPointLight(Transform parent, string name, Color color, float intensity, float radius)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.localPosition = Vector3.up * 0.15f;
        var light = go.AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.pointLightInnerRadius = 0.1f;
        light.pointLightOuterRadius = radius;
    }

    private static Sprite CreateSoftDisc(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var mid = (size - 1) * 0.5f;
        for (var y = 0; y < size; y++)
        for (var x = 0; x < size; x++)
        {
            var d = Vector2.Distance(new Vector2(x, y), new Vector2(mid, mid)) / mid;
            var a = Mathf.Clamp01(1f - d);
            a = Mathf.Pow(a, 1.4f);
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
        }

        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    private static Sprite CreateSoftSquare(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        for (var y = 0; y < size; y++)
        for (var x = 0; x < size; x++)
        {
            float edge = Mathf.Min(x, y, size - 1 - x, size - 1 - y) / (size * 0.5f);
            float a = Mathf.Clamp01(edge * 2.2f);
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
        }

        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
