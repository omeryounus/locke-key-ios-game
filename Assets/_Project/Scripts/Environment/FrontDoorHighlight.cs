using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Strong glowing outline + marker on the front (stuck) door while it is the objective.
/// </summary>
[RequireComponent(typeof(StuckDoorPuzzle))]
public class FrontDoorHighlight : MonoBehaviour
{
    private StuckDoorPuzzle door;
    private SpriteRenderer doorRenderer;
    private SpriteRenderer outline;
    private SpriteRenderer marker;
    private Light2D glow;
    private Color baseDoorColor = Color.white;
    private bool wasActive;

    private void Awake()
    {
        door = GetComponent<StuckDoorPuzzle>();
        doorRenderer = GetComponent<SpriteRenderer>()
                       ?? GetComponentInChildren<SpriteRenderer>();
        if (doorRenderer != null)
            baseDoorColor = doorRenderer.color;

        BuildOutline();
        BuildMarker();
        BuildGlow();
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

        if (!active) return;

        float pulse = 0.55f + Mathf.Sin(Time.time * 3.6f) * 0.45f;
        if (outline != null)
        {
            outline.transform.localScale = Vector3.one * (1.08f + pulse * 0.06f);
            var c = GameSettings.AccentColor;
            outline.color = new Color(c.r, c.g, c.b, 0.35f + pulse * 0.4f);
        }

        if (marker != null)
        {
            marker.transform.localPosition = new Vector3(0f, 1.65f + Mathf.Sin(Time.time * 2.8f) * 0.12f, 0f);
            var c = GameSettings.AccentColor;
            marker.color = new Color(c.r, c.g, c.b, 0.75f + pulse * 0.25f);
        }

        if (glow != null)
            glow.intensity = 0.7f + pulse * 0.55f;

        if (doorRenderer != null)
        {
            var warm = new Color(1f, 0.92f, 0.7f, 1f);
            doorRenderer.color = Color.Lerp(baseDoorColor, warm, 0.25f + pulse * 0.2f);
        }
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
        if (glow != null) glow.enabled = on;
        if (!on && doorRenderer != null)
            doorRenderer.color = baseDoorColor;
    }

    private void BuildOutline()
    {
        var go = new GameObject("DoorGlowOutline");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one * 1.12f;
        outline = go.AddComponent<SpriteRenderer>();
        outline.sprite = doorRenderer != null ? doorRenderer.sprite : CreateRingSprite(48);
        outline.sortingOrder = doorRenderer != null ? doorRenderer.sortingOrder - 1 : 4;
        outline.color = new Color(1f, 0.85f, 0.3f, 0.5f);
    }

    private void BuildMarker()
    {
        var go = new GameObject("DoorObjectiveMarker");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0f, 1.7f, 0f);
        marker = go.AddComponent<SpriteRenderer>();
        marker.sprite = CreateChevronSprite();
        marker.sortingOrder = 50;
        marker.transform.localScale = Vector3.one * 0.55f;
    }

    private void BuildGlow()
    {
        var go = new GameObject("DoorObjectiveLight");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0.2f, 0.4f, 0f);
        glow = go.AddComponent<Light2D>();
        glow.lightType = Light2D.LightType.Point;
        glow.color = new Color(1f, 0.82f, 0.4f);
        glow.intensity = 0.9f;
        glow.pointLightInnerRadius = 0.2f;
        glow.pointLightOuterRadius = 2.6f;
    }

    private static Sprite CreateRingSprite(int size)
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

    private static Sprite CreateChevronSprite()
    {
        const int w = 28, h = 36;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        for (var y = 0; y < h; y++)
        for (var x = 0; x < w; x++)
            tex.SetPixel(x, y, Color.clear);

        for (var y = 6; y < 30; y++)
        {
            float t = (y - 6) / 24f;
            float half = Mathf.Lerp(2f, 11f, t);
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
