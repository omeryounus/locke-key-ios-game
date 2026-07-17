using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Front-door objective: edge light leak, keyhole glow, engraved symbols,
/// particles, and pulsing outline when it is the current objective.
/// </summary>
[RequireComponent(typeof(StuckDoorPuzzle))]
public class FrontDoorHighlight : MonoBehaviour
{
    private StuckDoorPuzzle door;
    private SpriteRenderer doorRenderer;
    private SpriteRenderer outline;
    private SpriteRenderer marker;
    private SpriteRenderer keyhole;
    private SpriteRenderer symbolL;
    private SpriteRenderer symbolR;
    private SpriteRenderer leakL;
    private SpriteRenderer leakR;
    private Light2D glow;
    private Light2D keyholeLight;
    private Transform particleRoot;
    private Color baseDoorColor = Color.white;
    private Vector3 baseScale = Vector3.one;
    private bool wasActive;
    private Sprite disc;

    private void Awake()
    {
        door = GetComponent<StuckDoorPuzzle>();
        doorRenderer = GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>();
        if (doorRenderer != null)
            baseDoorColor = doorRenderer.color;

        // Respect the authored room scale. The highlight, not raw size, carries focus.
        baseScale = transform.localScale;

        disc = SoftDisc(40);
        BuildVisuals();
        SetVisuals(false);
    }

    private void Update()
    {
        bool active = ShouldHighlight();
        if (active != wasActive)
        {
            SetVisuals(active);
            wasActive = active;
        }

        AnimateAlways();
        if (active)
            AnimateObjective();
    }

    private void AnimateAlways()
    {
        float t = Time.time;
        // Edge light leak — always a hint of warm light under door
        if (leakL != null)
        {
            float pulse = 0.35f + Mathf.Sin(t * 2.2f) * 0.15f;
            leakL.color = new Color(1f, 0.78f, 0.4f, pulse * 0.45f);
            leakR.color = new Color(1f, 0.78f, 0.4f, pulse * 0.4f);
        }

        if (keyhole != null)
        {
            float k = 0.55f + Mathf.Sin(t * 3.5f) * 0.35f;
            keyhole.color = new Color(1f, 0.85f, 0.35f, 0.4f + k * 0.5f);
            keyhole.transform.localScale = Vector3.one * (0.22f + k * 0.04f);
        }

        if (keyholeLight != null)
            keyholeLight.intensity = 0.45f + Mathf.Sin(t * 3.5f) * 0.25f;

        if (symbolL != null)
        {
            float s = 0.2f + Mathf.Sin(t * 1.4f) * 0.08f;
            symbolL.color = new Color(0.85f, 0.75f, 0.4f, s);
            symbolR.color = new Color(0.85f, 0.75f, 0.4f, s);
        }

        if (particleRoot != null)
        {
            for (var i = 0; i < particleRoot.childCount; i++)
            {
                var p = particleRoot.GetChild(i);
                var phase = t * (0.8f + i * 0.15f) + i;
                p.localPosition = new Vector3(
                    Mathf.Sin(phase) * 0.35f,
                    0.2f + Mathf.Repeat(phase * 0.15f, 1.2f),
                    0f);
                var sr = p.GetComponent<SpriteRenderer>();
                if (sr != null)
                    sr.color = new Color(1f, 0.9f, 0.55f, 0.15f + Mathf.Sin(phase * 2f) * 0.1f);
            }
        }
    }

    private void AnimateObjective()
    {
        float pulse = 0.55f + Mathf.Sin(Time.time * 3.6f) * 0.45f;
        if (outline != null)
        {
            outline.enabled = true;
            outline.transform.localScale = Vector3.one * (1.12f + pulse * 0.08f);
            var c = GameSettings.AccentColor;
            outline.color = new Color(c.r, c.g, c.b, 0.4f + pulse * 0.45f);
        }

        if (marker != null)
        {
            marker.enabled = true;
            marker.transform.localPosition = new Vector3(0f, 1.95f + Mathf.Sin(Time.time * 2.8f) * 0.14f, 0f);
            var c = GameSettings.AccentColor;
            marker.color = new Color(c.r, c.g, c.b, 0.8f + pulse * 0.2f);
        }

        if (glow != null)
        {
            glow.enabled = true;
            glow.intensity = 1.0f + pulse * 0.7f;
        }

        if (doorRenderer != null)
        {
            var warm = new Color(1.15f, 1.05f, 0.85f, 1f);
            doorRenderer.color = Color.Lerp(baseDoorColor * 1.1f, warm, 0.35f + pulse * 0.25f);
        }

        // Subtle scale pulse when objective
        transform.localScale = baseScale * (1f + pulse * 0.02f);
    }

    private bool ShouldHighlight()
    {
        if (door == null || door.isSolved) return false;
        var beat = FindFirstObjectByType<ChapterBeatDirector>();
        if (beat == null) return true;
        return beat.CurrentBeat is ChapterBeatDirector.Beat.StuckDoor
            or ChapterBeatDirector.Beat.Arrival;
    }

    private void SetVisuals(bool on)
    {
        if (outline != null) outline.enabled = on;
        if (marker != null) marker.enabled = on;
        if (glow != null) glow.enabled = on || true; // soft always
        if (!on)
        {
            transform.localScale = baseScale;
            if (doorRenderer != null)
                doorRenderer.color = baseDoorColor * 1.08f; // slightly brighter door always
        }
    }

    private void BuildVisuals()
    {
        // Outline
        var o = Child("DoorGlowOutline");
        outline = o.GetComponent<SpriteRenderer>() ?? o.AddComponent<SpriteRenderer>();
        outline.sprite = doorRenderer != null && doorRenderer.sprite != null ? doorRenderer.sprite : SoftRing(48);
        outline.sortingOrder = doorRenderer != null ? doorRenderer.sortingOrder - 1 : 4;
        o.transform.localScale = Vector3.one * 1.12f;

        // Marker
        var m = Child("DoorObjectiveMarker");
        marker = m.GetComponent<SpriteRenderer>() ?? m.AddComponent<SpriteRenderer>();
        marker.sprite = Chevron();
        marker.sortingOrder = 50;
        m.transform.localPosition = new Vector3(0f, 1.95f, 0f);
        m.transform.localScale = Vector3.one * 0.6f;

        // Keyhole
        var k = Child("KeyholeGlow");
        keyhole = k.GetComponent<SpriteRenderer>() ?? k.AddComponent<SpriteRenderer>();
        keyhole.sprite = disc;
        keyhole.sortingOrder = (doorRenderer != null ? doorRenderer.sortingOrder : 5) + 2;
        k.transform.localPosition = new Vector3(0.22f, 0.05f, 0f);
        k.transform.localScale = Vector3.one * 0.24f;

        var kl = k.transform.Find("KeyholeLight")?.GetComponent<Light2D>();
        if (kl == null)
        {
            var go = new GameObject("KeyholeLight");
            go.transform.SetParent(k.transform, false);
            keyholeLight = go.AddComponent<Light2D>();
        }
        else keyholeLight = kl;
        keyholeLight.lightType = Light2D.LightType.Point;
        keyholeLight.color = LockeKeyUITheme.LKMagicPurple;
        keyholeLight.intensity = 0.6f;
        keyholeLight.pointLightOuterRadius = 1.4f;

        // Engraved symbols
        symbolL = MakeSymbol("SymbolL", new Vector3(-0.35f, 0.45f, 0f));
        symbolR = MakeSymbol("SymbolR", new Vector3(0.35f, 0.45f, 0f));

        // Light leak edges
        leakL = MakeLeak("LeakL", new Vector3(-0.55f, 0.1f, 0f), new Vector3(0.12f, 1.4f, 1f));
        leakR = MakeLeak("LeakR", new Vector3(0.55f, 0.1f, 0f), new Vector3(0.12f, 1.4f, 1f));

        // Main door glow
        var g = transform.Find("DoorObjectiveLight");
        if (g == null)
        {
            var go = new GameObject("DoorObjectiveLight");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0.15f, 0.35f, 0f);
            glow = go.AddComponent<Light2D>();
        }
        else glow = g.GetComponent<Light2D>();
        glow.lightType = Light2D.LightType.Point;
        glow.color = new Color(1f, 0.8f, 0.4f);
        glow.intensity = 0.9f;
        glow.pointLightOuterRadius = 3.2f;

        // Particles
        particleRoot = transform.Find("DoorParticles");
        if (particleRoot == null)
        {
            var pr = new GameObject("DoorParticles");
            pr.transform.SetParent(transform, false);
            particleRoot = pr.transform;
            for (var i = 0; i < 8; i++)
            {
                var p = new GameObject($"P{i}", typeof(SpriteRenderer));
                p.transform.SetParent(particleRoot);
                var sr = p.GetComponent<SpriteRenderer>();
                sr.sprite = disc;
                sr.sortingOrder = 40;
                p.transform.localScale = Vector3.one * Random.Range(0.06f, 0.12f);
            }
        }
    }

    private SpriteRenderer MakeSymbol(string name, Vector3 localPos)
    {
        var go = Child(name);
        var sr = go.GetComponent<SpriteRenderer>() ?? go.AddComponent<SpriteRenderer>();
        sr.sprite = disc;
        sr.sortingOrder = (doorRenderer != null ? doorRenderer.sortingOrder : 5) + 1;
        go.transform.localPosition = localPos;
        go.transform.localScale = Vector3.one * 0.18f;
        return sr;
    }

    private SpriteRenderer MakeLeak(string name, Vector3 localPos, Vector3 scale)
    {
        var go = Child(name);
        var sr = go.GetComponent<SpriteRenderer>() ?? go.AddComponent<SpriteRenderer>();
        sr.sprite = SoftSquare(16);
        sr.sortingOrder = (doorRenderer != null ? doorRenderer.sortingOrder : 5) - 1;
        go.transform.localPosition = localPos;
        go.transform.localScale = scale;
        return sr;
    }

    private GameObject Child(string name)
    {
        var t = transform.Find(name);
        if (t != null) return t.gameObject;
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        return go;
    }

    private static Sprite SoftDisc(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var mid = (size - 1) * 0.5f;
        for (var y = 0; y < size; y++)
        for (var x = 0; x < size; x++)
        {
            var d = Vector2.Distance(new Vector2(x, y), new Vector2(mid, mid)) / mid;
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Pow(Mathf.Clamp01(1f - d), 1.4f)));
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    private static Sprite SoftRing(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var mid = (size - 1) * 0.5f;
        for (var y = 0; y < size; y++)
        for (var x = 0; x < size; x++)
        {
            var d = Vector2.Distance(new Vector2(x, y), new Vector2(mid, mid)) / mid;
            float a = d > 0.55f && d < 0.95f ? Mathf.Clamp01(1f - Mathf.Abs(d - 0.75f) * 4f) : 0f;
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    private static Sprite SoftSquare(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        for (var y = 0; y < size; y++)
        for (var x = 0; x < size; x++)
            tex.SetPixel(x, y, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    private static Sprite Chevron()
    {
        const int w = 28, h = 36;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        for (var y = 0; y < h; y++)
        for (var x = 0; x < w; x++)
            tex.SetPixel(x, y, Color.clear);
        for (var y = 6; y < 30; y++)
        {
            float half = Mathf.Lerp(2f, 11f, (y - 6) / 24f);
            for (var x = 0; x < w; x++)
            {
                float dx = Mathf.Abs(x - 14f);
                if (dx <= half && dx >= half - 2.5f)
                    tex.SetPixel(x, y, Color.white);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.15f), 36f);
    }
}
