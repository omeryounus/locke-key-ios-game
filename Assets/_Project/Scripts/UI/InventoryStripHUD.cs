using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Top-left inventory panel:
///   Inventory
///   🗝 House Key   (or ✓ House Key when collected)
///   0 / 6
/// </summary>
public class InventoryStripHUD : MonoBehaviour
{
    private Image card;
    private Text headerText;
    private Text itemText;
    private Text countText;
    private Image keyIcon;
    private Font font;
    private Vector3 baseScale = Vector3.one;
    private float popTimer;
    private int lastCount = -1;

    public static InventoryStripHUD Ensure(Transform canvasRoot, Font font)
    {
        var existing = Object.FindFirstObjectByType<InventoryStripHUD>();
        if (existing != null)
        {
            // Rebuild if still old compact icon-only size
            var r = existing.GetComponent<RectTransform>();
            if (r != null && r.sizeDelta.x < 100f)
            {
                Object.Destroy(existing.gameObject);
            }
            else
            {
                existing.Relayout();
                return existing;
            }
        }

        var go = new GameObject("InventoryStrip", typeof(RectTransform), typeof(InventoryStripHUD), typeof(Button));
        go.transform.SetParent(canvasRoot, false);
        var hud = go.GetComponent<InventoryStripHUD>();
        hud.Build(font);
        return hud;
    }

    private void Build(Font f)
    {
        font = f ?? LockeUILayout.GetUIFont();
        var rect = GetComponent<RectTransform>();
        TopHudLayout.PlaceInventory(rect);
        baseScale = Vector3.one;

        card = gameObject.AddComponent<Image>();
        TopHudLayout.ApplyGlass(card);
        TopHudLayout.AddSoftBlurLayer(transform);

        var btn = GetComponent<Button>();
        btn.targetGraphic = card;
        btn.onClick.AddListener(() => GrokUIFlowManager.Instance?.ShowKeyRing());
        UIButtonFeedback.Ensure(gameObject);

        headerText = MakeText("Header", "KEYS", 8, FontStyle.Bold, LockeKeyUITheme.CaptionText,
            new Vector2(0.5f, 0.82f), new Vector2(46f, 12f));

        keyIcon = new GameObject("KeyIcon", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
        keyIcon.transform.SetParent(transform, false);
        keyIcon.preserveAspect = true;
        keyIcon.raycastTarget = false;
        var iRect = keyIcon.rectTransform;
        iRect.anchorMin = iRect.anchorMax = new Vector2(0.5f, 0.48f);
        iRect.pivot = new Vector2(0.5f, 0.5f);
        iRect.sizeDelta = new Vector2(24f, 24f);

        itemText = MakeText("Item", "", 1, FontStyle.Normal, Color.clear,
            new Vector2(0.5f, 0.45f), Vector2.zero);
        itemText.gameObject.SetActive(false);

        countText = MakeText("Count", "0", 11, FontStyle.Bold, GameSettings.AccentColor,
            new Vector2(0.5f, 0.14f), new Vector2(42f, 14f));
    }

    public void Relayout()
    {
        TopHudLayout.PlaceInventory(GetComponent<RectTransform>());
        if (card != null) TopHudLayout.ApplyGlass(card);
    }

    private Text MakeText(string name, string text, int size, FontStyle style, Color color, Vector2 anchor, Vector2 sizeDelta)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(transform, false);
        var t = go.GetComponent<Text>();
        t.font = font;
        t.fontSize = size;
        t.fontStyle = style;
        t.color = color;
        t.text = text;
        t.alignment = TextAnchor.MiddleCenter;
        t.raycastTarget = false;
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = r.anchorMax = anchor;
        r.pivot = new Vector2(0.5f, 0.5f);
        r.sizeDelta = sizeDelta;
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
        int count = (hasHouse ? 1 : 0) + (hasGhost ? 1 : 0) + (hasHead ? 1 : 0);

        if (countText != null)
            countText.text = count.ToString();

        if (hasHouse)
        {
            SetItem("✓ House Key", LockeKeyUITheme.Success,
                lib != null ? lib.houseKeyIcon : Resources.Load<Sprite>(ArtPaths.KeySpriteForId("house")));
        }
        else if (hasGhost)
        {
            SetItem("✓ Ghost Key", LockeKeyUITheme.Success, lib?.ghostKeyIcon);
        }
        else if (hasHead)
        {
            SetItem("✓ Head Key", LockeKeyUITheme.Success, lib?.headKeyIcon);
        }
        else
        {
            // Empty state still shows key glyph + label so it never looks like a blank box
            SetItem("🗝  House Key", LockeKeyUITheme.CaptionText, null);
            if (keyIcon != null)
            {
                keyIcon.enabled = true;
                keyIcon.sprite = lib != null ? lib.houseKeyIcon : Resources.Load<Sprite>(ArtPaths.KeySpriteForId("house"));
                keyIcon.color = new Color(1f, 1f, 1f, 0.28f);
            }
            if (itemText != null)
                itemText.text = "🗝  House Key";
        }

        if (count != lastCount && count > lastCount && lastCount >= 0)
            PlayCollectPop();
        lastCount = count;

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

    private void SetItem(string label, Color color, Sprite sprite)
    {
        if (itemText != null)
        {
            itemText.text = label;
            itemText.color = color;
        }

        if (keyIcon != null)
        {
            if (sprite != null)
            {
                keyIcon.sprite = sprite;
                keyIcon.enabled = true;
                keyIcon.color = Color.white;
            }
        }
    }

    public void PlayCollectPop()
    {
        popTimer = 0.4f;
        GameHaptics.KeyPickup();
        if (itemText != null)
        {
            // Flash collected state
            itemText.text = "✓ House Key";
            itemText.color = LockeKeyUITheme.Success;
        }
    }
}
