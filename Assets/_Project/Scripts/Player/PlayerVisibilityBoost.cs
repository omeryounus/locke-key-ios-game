using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Ensures the player is always readable: ground shadow, soft rim light, larger scale,
/// and a high-contrast fallback silhouette if sprites fail to load.
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class PlayerVisibilityBoost : MonoBehaviour
{
    private SpriteRenderer body;
    private SpriteRenderer shadow;
    private Light2D rim;
    private Vector3 baseScale;

    private void Awake()
    {
        body = GetComponent<SpriteRenderer>();
        if (body == null)
            body = gameObject.AddComponent<SpriteRenderer>();

        // Scale up for mobile readability
        var s = transform.localScale;
        if (Mathf.Abs(s.x) < 1.5f)
            transform.localScale = new Vector3(1.55f, 1.55f, 1f);
        baseScale = transform.localScale;

        EnsureSprite();
        EnsureShadow();
        EnsureRimLight();

        // Sorting so player draws above props
        body.sortingOrder = Mathf.Max(body.sortingOrder, 20);
        body.color = Color.white;
    }

    private void LateUpdate()
    {
        if (shadow != null)
        {
            shadow.transform.position = new Vector3(transform.position.x, transform.position.y - 0.72f, 0.05f);
            float squash = 1f;
            var pc = GetComponent<PlayerController>();
            if (pc != null && !pc.IsGrounded)
                squash = 0.7f;
            shadow.transform.localScale = new Vector3(0.85f * squash, 0.22f, 1f);
            shadow.color = new Color(0f, 0f, 0f, pc != null && pc.IsGrounded ? 0.4f : 0.2f);
        }
    }

    private void EnsureSprite()
    {
        if (body.sprite != null) return;

        // Try resources first
        var idle = Resources.Load<Sprite>("Art/Characters/player_idle");
        if (idle != null)
        {
            body.sprite = idle;
            return;
        }

        // High-contrast procedural silhouette so the player is never invisible
        body.sprite = CreateSilhouette();
        body.color = new Color(0.92f, 0.88f, 0.82f, 1f);
    }

    private void EnsureShadow()
    {
        if (transform.Find("PlayerShadow") != null) return;
        var go = new GameObject("PlayerShadow");
        go.transform.SetParent(transform, false);
        shadow = go.AddComponent<SpriteRenderer>();
        shadow.sprite = CreateSoftDisc(32);
        shadow.sortingOrder = 10;
        shadow.color = new Color(0f, 0f, 0f, 0.4f);
        go.transform.localPosition = new Vector3(0f, -0.72f, 0f);
        go.transform.localScale = new Vector3(0.85f, 0.22f, 1f);
    }

    private void EnsureRimLight()
    {
        if (GetComponentInChildren<Light2D>() != null && transform.Find("PlayerRim") != null) return;
        var go = new GameObject("PlayerRim");
        go.transform.SetParent(transform, false);
        rim = go.AddComponent<Light2D>();
        rim.lightType = Light2D.LightType.Point;
        rim.color = new Color(1f, 0.92f, 0.75f);
        rim.intensity = 0.55f;
        rim.pointLightInnerRadius = 0.1f;
        rim.pointLightOuterRadius = 1.4f;
    }

    private static Sprite CreateSilhouette()
    {
        const int w = 32, h = 48;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        for (var y = 0; y < h; y++)
        for (var x = 0; x < w; x++)
            tex.SetPixel(x, y, Color.clear);

        // Head
        FillCircle(tex, 16, 38, 7, Color.white);
        // Body
        for (var y = 10; y < 32; y++)
        for (var x = 10; x < 22; x++)
            tex.SetPixel(x, y, Color.white);
        // Legs
        for (var y = 0; y < 12; y++)
        {
            tex.SetPixel(12, y, Color.white);
            tex.SetPixel(13, y, Color.white);
            tex.SetPixel(18, y, Color.white);
            tex.SetPixel(19, y, Color.white);
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
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(1f - d)));
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
