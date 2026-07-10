using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Top-left compact inventory: icon + numeric badge. Taps open key ring.
/// </summary>
public class InventoryStripHUD : MonoBehaviour
{
    private Image icon;
    private Image card;
    private Text badgeText;
    private GameObject badgeGo;
    private Font font;
    private Vector3 baseScale = Vector3.one;
    private float popTimer;
    private int lastCount = -1;

    public static InventoryStripHUD Ensure(Transform canvasRoot, Font font)
    {
        var existing = Object.FindFirstObjectByType<InventoryStripHUD>();
        if (existing != null)
        {
            existing.Relayout();
            return existing;
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

        icon = new GameObject("Icon", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
        icon.transform.SetParent(transform, false);
        icon.preserveAspect = true;
        icon.raycastTarget = false;
        var iRect = icon.rectTransform;
        iRect.anchorMin = new Vector2(0.15f, 0.15f);
        iRect.anchorMax = new Vector2(0.85f, 0.85f);
        iRect.offsetMin = iRect.offsetMax = Vector2.zero;

        // Badge (top-right of icon)
        badgeGo = new GameObject("Badge", typeof(RectTransform), typeof(Image));
        badgeGo.transform.SetParent(transform, false);
        var bImg = badgeGo.GetComponent<Image>();
        bImg.color = GameSettings.AccentColor;
        bImg.raycastTarget = false;
        var bRect = badgeGo.GetComponent<RectTransform>();
        bRect.anchorMin = bRect.anchorMax = new Vector2(1f, 1f);
        bRect.pivot = new Vector2(0.5f, 0.5f);
        bRect.anchoredPosition = new Vector2(-2f, -2f);
        bRect.sizeDelta = new Vector2(16f, 16f);

        var bTextGo = new GameObject("Num", typeof(RectTransform), typeof(Text));
        bTextGo.transform.SetParent(badgeGo.transform, false);
        badgeText = bTextGo.GetComponent<Text>();
        badgeText.font = font;
        badgeText.fontSize = 10;
        badgeText.fontStyle = FontStyle.Bold;
        badgeText.color = Color.black;
        badgeText.alignment = TextAnchor.MiddleCenter;
        badgeText.raycastTarget = false;
        var btRect = bTextGo.GetComponent<RectTransform>();
        btRect.anchorMin = Vector2.zero;
        btRect.anchorMax = Vector2.one;
        btRect.offsetMin = btRect.offsetMax = Vector2.zero;

        badgeGo.SetActive(false);
    }

    public void Relayout()
    {
        TopHudLayout.PlaceInventory(GetComponent<RectTransform>());
        if (card != null) TopHudLayout.ApplyGlass(card);
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

        if (icon != null)
        {
            Sprite spr = null;
            if (hasHouse) spr = lib != null ? lib.houseKeyIcon : Resources.Load<Sprite>(ArtPaths.KeySpriteForId("house"));
            else if (hasGhost) spr = lib?.ghostKeyIcon;
            else if (hasHead) spr = lib?.headKeyIcon;

            if (spr != null)
            {
                icon.sprite = spr;
                icon.enabled = true;
                icon.color = Color.white;
            }
            else
            {
                // Empty bag glyph
                icon.enabled = true;
                icon.sprite = null;
                icon.color = new Color(1f, 1f, 1f, 0.25f);
            }
        }

        if (badgeGo != null)
        {
            badgeGo.SetActive(count > 0);
            if (badgeText != null)
                badgeText.text = count.ToString();
        }

        if (count != lastCount && count > lastCount && lastCount >= 0)
            PlayCollectPop();
        lastCount = count;

        if (popTimer > 0f)
        {
            popTimer -= Time.deltaTime;
            float k = 1f + Mathf.Sin((1f - popTimer) * Mathf.PI) * 0.14f;
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
