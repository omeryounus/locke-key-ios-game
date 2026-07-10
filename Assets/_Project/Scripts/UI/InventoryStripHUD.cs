using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Named inventory strip: shows Front Door Key with checkmark, not just a tiny icon.
/// </summary>
public class InventoryStripHUD : MonoBehaviour
{
    private Text header;
    private Text line1;
    private Image icon;
    private Image card;
    private Font font;
    private Vector3 baseScale = Vector3.one;
    private float popTimer;

    public static InventoryStripHUD Ensure(Transform canvasRoot, Font font)
    {
        var existing = Object.FindFirstObjectByType<InventoryStripHUD>();
        if (existing != null) return existing;
        var go = new GameObject("InventoryStrip", typeof(RectTransform), typeof(InventoryStripHUD));
        go.transform.SetParent(canvasRoot, false);
        var hud = go.GetComponent<InventoryStripHUD>();
        hud.Build(font);
        return hud;
    }

    private void Build(Font f)
    {
        font = f ?? LockeUILayout.GetUIFont();
        var scale = GameSettings.UiScale;
        var rect = GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(10f, -54f);
        rect.sizeDelta = new Vector2(168f * scale, 72f * scale);
        baseScale = Vector3.one;

        card = gameObject.AddComponent<Image>();
        card.color = new Color(0.05f, 0.06f, 0.1f, 0.86f);
        var outline = gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 1f, 1f, 0.12f);
        outline.effectDistance = new Vector2(1f, -1f);

        header = Make("Header", "INVENTORY", 10, FontStyle.Bold, LockeKeyUITheme.CaptionText,
            new Vector2(0.5f, 0.82f), new Vector2(150f, 16f));

        icon = new GameObject("Icon", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
        icon.transform.SetParent(transform, false);
        icon.preserveAspect = true;
        var iRect = icon.rectTransform;
        iRect.anchorMin = iRect.anchorMax = new Vector2(0f, 0.35f);
        iRect.pivot = new Vector2(0f, 0.5f);
        iRect.anchoredPosition = new Vector2(10f, 0f);
        iRect.sizeDelta = new Vector2(34f, 34f); // ~30% larger than prior 24–28px status icons

        line1 = Make("Line1", "— empty —", 12, FontStyle.Normal, LockeKeyUITheme.BodyText,
            new Vector2(0.62f, 0.35f), new Vector2(110f, 36f));
        line1.alignment = TextAnchor.MiddleLeft;
    }

    private Text Make(string name, string text, int size, FontStyle style, Color color, Vector2 anchor, Vector2 sizeDelta)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(transform, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = sizeDelta;
        var t = go.GetComponent<Text>();
        t.font = font;
        t.fontSize = size;
        t.fontStyle = style;
        t.color = color;
        t.text = text;
        t.alignment = TextAnchor.MiddleCenter;
        t.raycastTarget = false;
        return t;
    }

    private void Update()
    {
        var inv = FindFirstObjectByType<PlayerInventory>();
        var km = FindFirstObjectByType<KeyManager>();
        var lib = UIIconLibrary.LoadDefault();

        bool hasHouse = inv != null && inv.HasHouseKey;
        bool hasGhost = km != null && km.ownedKeys.Exists(k => k.abilityType == KeyManager.KeyAbilityType.GhostPhase);
        bool hasHead = km != null && km.ownedKeys.Exists(k => k.abilityType == KeyManager.KeyAbilityType.HeadMemory);

        if (hasHouse)
        {
            if (line1 != null)
            {
                line1.text = "✓ Front Door Key";
                line1.color = LockeKeyUITheme.Success;
            }
            if (icon != null)
            {
                icon.sprite = lib != null ? lib.houseKeyIcon : Resources.Load<Sprite>(ArtPaths.KeySpriteForId("house"));
                icon.enabled = icon.sprite != null;
                // Glow pulse
                var a = 0.85f + Mathf.Sin(Time.time * 3f) * 0.15f;
                icon.color = new Color(1f, 0.95f, 0.7f, a);
                icon.rectTransform.localScale = Vector3.one * (1f + Mathf.Sin(Time.time * 3f) * 0.06f);
            }
        }
        else if (hasGhost)
        {
            if (line1 != null) { line1.text = "✓ Ghost Key"; line1.color = LockeKeyUITheme.Success; }
            if (icon != null)
            {
                icon.sprite = lib?.ghostKeyIcon;
                icon.enabled = icon.sprite != null;
                icon.color = Color.white;
            }
        }
        else if (hasHead)
        {
            if (line1 != null) { line1.text = "✓ Head Key"; line1.color = LockeKeyUITheme.Success; }
            if (icon != null)
            {
                icon.sprite = lib?.headKeyIcon;
                icon.enabled = icon.sprite != null;
            }
        }
        else
        {
            if (line1 != null) { line1.text = "— empty —"; line1.color = LockeKeyUITheme.CaptionText; }
            if (icon != null) icon.enabled = false;
        }

        // Multi-key summary in header when more than one
        int count = (hasHouse ? 1 : 0) + (hasGhost ? 1 : 0) + (hasHead ? 1 : 0);
        if (header != null)
            header.text = count > 0 ? $"INVENTORY  ·  {count}" : "INVENTORY";

        if (popTimer > 0f)
        {
            popTimer -= Time.deltaTime;
            float k = 1f + Mathf.Sin((1f - popTimer) * Mathf.PI) * 0.12f;
            transform.localScale = baseScale * k;
        }
        else
        {
            transform.localScale = Vector3.Lerp(transform.localScale, baseScale, Time.deltaTime * 10f);
        }
    }

    public void PlayCollectPop()
    {
        popTimer = 0.35f;
        GameHaptics.KeyPickup();
    }
}
