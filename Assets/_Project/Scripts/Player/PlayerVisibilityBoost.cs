using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Player readability: +20% brightness, rim light, soft foot shadow, hood sway helper.
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class PlayerVisibilityBoost : MonoBehaviour
{
    private const float BrightnessBoost = 1.2f; // +20%

    private SpriteRenderer body;
    private SpriteRenderer shadow;
    private SpriteRenderer hood;
    private Light2D rim;
    private Light2D fill;
    private Color baseBodyColor = Color.white;

    private bool modernRig;

    private void Awake()
    {
        body = GetComponent<SpriteRenderer>();
        if (body == null)
            body = gameObject.AddComponent<SpriteRenderer>();

        modernRig = GetComponent<PlayerCharacterRig>() != null
                    || GetComponent<PlayerSpriteAnimator>() != null;

        var s = transform.localScale;
        if (Mathf.Abs(s.x) < 1.5f)
            transform.localScale = new Vector3(1.55f, 1.55f, 1f);

        EnsureSprite();
        baseBodyColor = Color.white * BrightnessBoost;
        baseBodyColor.a = 1f;
        body.color = baseBodyColor;
        body.sortingOrder = Mathf.Max(body.sortingOrder, 20);

        // Modern 2.5D rig owns shadow/hood/secondary motion — only keep fill light here
        if (!modernRig)
        {
            EnsureShadow();
            EnsureHoodSway();
        }
        EnsureRimLight();
    }

    private void Start()
    {
        // Rig may be added later in bootstrap order
        modernRig = GetComponent<PlayerCharacterRig>() != null;
        if (modernRig)
        {
            if (shadow != null) shadow.enabled = false;
            if (hood != null) hood.enabled = false;
        }
    }

    private void LateUpdate()
    {
        var pcMain = GetComponent<PlayerController>();
        // Keep brightness unless ghosting or modern ghost tint is active
        if (body != null && (pcMain == null || !pcMain.IsGhostPhasing) && !modernRig)
        {
            var c = body.color;
            if (c.r < 0.9f || c.g < 0.9f || c.b < 0.9f)
                body.color = Color.Lerp(c, baseBodyColor, Time.deltaTime * 4f);
        }

        if (shadow != null)
        {
            shadow.transform.position = new Vector3(transform.position.x, transform.position.y - 0.78f, 0.05f);
            var pc = GetComponent<PlayerController>();
            float squash = pc != null && !pc.IsGrounded ? 0.65f : 1f;
            float walk = pc != null ? Mathf.Clamp01(pc.HorizontalSpeed / 4f) : 0f;
            shadow.transform.localScale = new Vector3(0.95f * squash * (1f + walk * 0.08f), 0.26f * squash, 1f);
            shadow.color = new Color(0f, 0f, 0f, pc != null && pc.IsGrounded ? 0.45f : 0.18f);
        }

        if (hood != null)
        {
            float t = Time.time;
            hood.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 1.7f) * 4.5f + Mathf.Sin(t * 0.6f) * 2f);
            hood.transform.localPosition = new Vector3(
                Mathf.Sin(t * 1.4f) * 0.02f,
                0.42f + Mathf.Sin(t * 2.1f) * 0.012f,
                0f);
        }

        if (rim != null && !modernRig)
            rim.intensity = 0.65f + Mathf.Sin(Time.time * 2f) * 0.08f;
        if (fill != null)
            fill.intensity = modernRig ? 0.3f : 0.25f;
    }

    private void EnsureSprite()
    {
        if (body.sprite != null) return;
        var idle = Resources.Load<Sprite>("Art/Characters/player_idle");
        body.sprite = idle != null ? idle : CreateSilhouette();
    }

    private void EnsureShadow()
    {
        var existing = transform.Find("PlayerShadow");
        if (existing != null)
        {
            shadow = existing.GetComponent<SpriteRenderer>();
            return;
        }

        var go = new GameObject("PlayerShadow");
        go.transform.SetParent(transform, false);
        shadow = go.AddComponent<SpriteRenderer>();
        shadow.sprite = CreateSoftDisc(48);
        shadow.sortingOrder = 10;
        shadow.color = new Color(0f, 0f, 0f, 0.45f);
        go.transform.localPosition = new Vector3(0f, -0.78f, 0f);
        go.transform.localScale = new Vector3(0.95f, 0.26f, 1f);
    }

    private void EnsureRimLight()
    {
        var existing = transform.Find("PlayerRim");
        if (existing != null)
        {
            rim = existing.GetComponent<Light2D>();
        }
        else
        {
            var go = new GameObject("PlayerRim");
            go.transform.SetParent(transform, false);
            rim = go.AddComponent<Light2D>();
        }

        rim.lightType = Light2D.LightType.Point;
        rim.color = new Color(1f, 0.94f, 0.82f);
        rim.intensity = 0.7f;
        rim.pointLightInnerRadius = 0.15f;
        rim.pointLightOuterRadius = 1.65f;

        var fillT = transform.Find("PlayerFill");
        if (fillT == null)
        {
            var go = new GameObject("PlayerFill");
            go.transform.SetParent(transform, false);
            fill = go.AddComponent<Light2D>();
        }
        else fill = fillT.GetComponent<Light2D>();

        fill.lightType = Light2D.LightType.Point;
        fill.color = new Color(0.95f, 0.9f, 1f);
        fill.intensity = 0.25f;
        fill.pointLightOuterRadius = 0.9f;
    }

    private void EnsureHoodSway()
    {
        if (transform.Find("HoodSway") != null)
        {
            hood = transform.Find("HoodSway").GetComponent<SpriteRenderer>();
            return;
        }

        var go = new GameObject("HoodSway");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0f, 0.42f, 0f);
        go.transform.localScale = new Vector3(0.55f, 0.35f, 1f);
        hood = go.AddComponent<SpriteRenderer>();
        hood.sprite = CreateSoftDisc(24);
        hood.color = new Color(0.15f, 0.12f, 0.18f, 0.35f);
        hood.sortingOrder = body.sortingOrder + 1;
    }

    private static Sprite CreateSilhouette()
    {
        const int w = 32, h = 48;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        for (var y = 0; y < h; y++)
        for (var x = 0; x < w; x++)
            tex.SetPixel(x, y, Color.clear);
        FillCircle(tex, 16, 38, 7, Color.white);
        for (var y = 10; y < 32; y++)
        for (var x = 10; x < 22; x++)
            tex.SetPixel(x, y, Color.white);
        for (var y = 0; y < 12; y++)
        {
            tex.SetPixel(12, y, Color.white); tex.SetPixel(13, y, Color.white);
            tex.SetPixel(18, y, Color.white); tex.SetPixel(19, y, Color.white);
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0f), 32f);
    }

    private static void FillCircle(Texture2D tex, int cx, int cy, int r, Color c)
    {
        for (var y = cy - r; y <= cy + r; y++)
        for (var x = cx - r; x <= cx + r; x++)
        {
            if (x < 0 || y < 0 || x >= tex.width || y >= tex.height) continue;
            if ((x - cx) * (x - cx) + (y - cy) * (y - cy) <= r * r)
                tex.SetPixel(x, y, c);
        }
    }

    private static Sprite CreateSoftDisc(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var mid = (size - 1) * 0.5f;
        for (var y = 0; y < size; y++)
        for (var x = 0; x < size; x++)
        {
            var d = Vector2.Distance(new Vector2(x, y), new Vector2(mid, mid)) / mid;
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Pow(Mathf.Clamp01(1f - d), 1.3f)));
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
