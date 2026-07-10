using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Reusable uGUI builders aligned to ux_s0–ux_s6 and ux_design_system_board.
/// </summary>
public static class LockeUIComponents
{
    public static Image CreateScrim(Transform parent, UnityAction onDismiss = null)
    {
        var go = new GameObject("Scrim", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        LockeUILayout.Stretch(go.GetComponent<RectTransform>());
        var img = go.GetComponent<Image>();
        img.color = LockeKeyUITheme.OverlayScrim;
        var btn = go.GetComponent<Button>();
        if (onDismiss != null)
            btn.onClick.AddListener(onDismiss);
        return img;
    }

    public static GameObject CreateBottomSheet(Transform parent, float heightFraction,
        out RectTransform sheetRect, out Image sheetImage)
    {
        var sheet = new GameObject("BottomSheet", typeof(RectTransform), typeof(Image));
        sheet.transform.SetParent(parent, false);
        sheetRect = sheet.GetComponent<RectTransform>();
        sheetRect.anchorMin = new Vector2(0f, 0f);
        sheetRect.anchorMax = new Vector2(1f, heightFraction);
        sheetRect.offsetMin = sheetRect.offsetMax = Vector2.zero;
        sheetImage = sheet.GetComponent<Image>();
        sheetImage.color = LockeKeyUITheme.SheetBottom;

        // Drag handle 36×4
        var handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handle.transform.SetParent(sheet.transform, false);
        var hRect = handle.GetComponent<RectTransform>();
        hRect.anchorMin = hRect.anchorMax = new Vector2(0.5f, 1f);
        hRect.pivot = new Vector2(0.5f, 1f);
        hRect.anchoredPosition = new Vector2(0f, -12f);
        hRect.sizeDelta = new Vector2(36f, 4f);
        handle.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.25f);

        return sheet;
    }

    public static Button CreatePrimaryButton(Transform parent, Font font, string label,
        Vector2 anchor, UnityAction onClick, float width = 280f)
    {
        var sprite = Resources.Load<Sprite>(ArtPaths.UiBtnPrimary);
        var go = new GameObject(label.Replace(" ", "") + "Btn",
            typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(width, LockeKeyUITheme.PrimaryButtonHeight);
        var img = go.GetComponent<Image>();
        if (sprite != null) { img.sprite = sprite; img.type = Image.Type.Sliced; img.color = Color.white; }
        else img.color = LockeKeyUITheme.LKGold;
        var btn = go.GetComponent<Button>();
        btn.targetGraphic = img;
        if (onClick != null) btn.onClick.AddListener(onClick);
        ApplyProductionButton(btn, go);
        AddButtonLabel(go.transform, font, label, LockeKeyUITheme.ButtonOnGold);
        return btn;
    }

    public static Button CreateSecondaryButton(Transform parent, Font font, string label,
        Vector2 anchor, UnityAction onClick, float width = 280f)
    {
        var go = new GameObject(label.Replace(" ", "") + "Btn",
            typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(width, LockeKeyUITheme.PrimaryButtonHeight);
        var img = go.GetComponent<Image>();
        img.color = new Color(0.08f, 0.09f, 0.14f, 0.92f);
        var btn = go.GetComponent<Button>();
        btn.targetGraphic = img;
        if (onClick != null) btn.onClick.AddListener(onClick);

        var outline = go.AddComponent<Outline>();
        outline.effectColor = LockeKeyUITheme.LKGold;
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        ApplyProductionButton(btn, go);
        AddButtonLabel(go.transform, font, label, LockeKeyUITheme.LKGold);
        return btn;
    }

    private static void ApplyProductionButton(Button btn, GameObject go)
    {
        if (btn == null) return;
        var colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.97f, 0.9f, 1f);
        colors.pressedColor = new Color(0.82f, 0.78f, 0.65f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.45f, 0.45f, 0.5f, 0.5f);
        colors.fadeDuration = 0.08f;
        btn.colors = colors;
        btn.transition = Selectable.Transition.ColorTint;
        UIButtonFeedback.Ensure(go);
    }

    public static (Button cardBtn, GameObject lockIcon, Image border) CreateChapterCard(
        Transform parent, Font font, string title, Vector2 anchor, bool unlocked,
        string thumbResourcePath, UnityAction onClick)
    {
        var cardGo = new GameObject(title.Replace(" ", "") + "Card",
            typeof(RectTransform), typeof(Image), typeof(Button));
        cardGo.transform.SetParent(parent, false);
        var rect = cardGo.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(340f, 96f);
        cardGo.GetComponent<Image>().color = unlocked
            ? LockeKeyUITheme.LKMoon
            : new Color(0.08f, 0.09f, 0.12f, 0.9f);

        // Thumb 16:9 left
        var thumbGo = new GameObject("Thumb", typeof(RectTransform), typeof(Image));
        thumbGo.transform.SetParent(cardGo.transform, false);
        var tRect = thumbGo.GetComponent<RectTransform>();
        tRect.anchorMin = new Vector2(0.04f, 0.12f);
        tRect.anchorMax = new Vector2(0.38f, 0.88f);
        tRect.offsetMin = tRect.offsetMax = Vector2.zero;
        var thumb = thumbGo.GetComponent<Image>();
        var thumbSprite = string.IsNullOrEmpty(thumbResourcePath)
            ? null
            : Resources.Load<Sprite>(thumbResourcePath);
        if (thumbSprite != null) { thumb.sprite = thumbSprite; thumb.preserveAspect = true; }
        else thumb.color = new Color(0.15f, 0.17f, 0.22f);

        AddText(cardGo.transform, "Label", font, LockeKeyUITheme.TitleSize, FontStyle.Bold,
            unlocked ? LockeKeyUITheme.White : LockeKeyUITheme.CaptionText,
            new Vector2(0.42f, 0.5f), title, new Vector2(160f, 40f), TextAnchor.MiddleLeft);

        var borderGo = new GameObject("Border", typeof(RectTransform), typeof(Image));
        borderGo.transform.SetParent(cardGo.transform, false);
        LockeUILayout.Stretch(borderGo.GetComponent<RectTransform>());
        borderGo.transform.SetAsFirstSibling();
        var border = borderGo.GetComponent<Image>();
        border.color = unlocked ? LockeKeyUITheme.LKGold : Color.clear;
        border.raycastTarget = false;

        var lockGo = new GameObject("Lock", typeof(RectTransform), typeof(Text));
        lockGo.transform.SetParent(cardGo.transform, false);
        var lRect = lockGo.GetComponent<RectTransform>();
        lRect.anchorMin = new Vector2(0.88f, 0.5f);
        lRect.anchorMax = new Vector2(0.96f, 0.5f);
        lRect.sizeDelta = new Vector2(28f, 28f);
        var lockText = lockGo.GetComponent<Text>();
        lockText.font = font;
        lockText.text = "\U0001F512";
        lockText.fontSize = 20;
        lockText.alignment = TextAnchor.MiddleCenter;
        lockGo.SetActive(!unlocked);

        var btn = cardGo.GetComponent<Button>();
        btn.interactable = unlocked;
        if (onClick != null) btn.onClick.AddListener(onClick);
        ApplyProductionButton(btn, cardGo);
        return (btn, lockGo, border);
    }

    public static (Image slot, Image ring) CreateKeySlot(Transform parent, Vector2 anchor, string keyId)
    {
        var slotGo = new GameObject($"Slot_{keyId}", typeof(RectTransform), typeof(Image), typeof(Button));
        slotGo.transform.SetParent(parent, false);
        var rect = slotGo.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(LockeKeyUITheme.KeySlotSize, LockeKeyUITheme.KeySlotSize);

        var ringGo = new GameObject("Ring", typeof(RectTransform), typeof(Image));
        ringGo.transform.SetParent(slotGo.transform, false);
        LockeUILayout.Stretch(ringGo.GetComponent<RectTransform>());
        var ringRect = ringGo.GetComponent<RectTransform>();
        float pad = LockeKeyUITheme.KeySlotRingWidth;
        ringRect.offsetMin = new Vector2(-pad, -pad);
        ringRect.offsetMax = new Vector2(pad, pad);
        var ring = ringGo.GetComponent<Image>();
        ring.color = new Color(0.3f, 0.3f, 0.3f, 0.35f);
        ring.raycastTarget = false;

        var slot = slotGo.GetComponent<Image>();
        slot.color = Color.white;
        return (slot, ring);
    }

    public static GameObject CreateHudBar(Transform parent, Font font, string title,
        UnityAction onMap, UnityAction onKeyRing)
    {
        var barGo = new GameObject("HudBar", typeof(RectTransform), typeof(Image), typeof(SafeAreaFitter));
        barGo.transform.SetParent(parent, false);
        var barRect = barGo.GetComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0f, 1f);
        barRect.anchorMax = new Vector2(1f, 1f);
        barRect.pivot = new Vector2(0.5f, 1f);
        barRect.sizeDelta = new Vector2(0f, TopHudLayout.BarHeight);
        // Clean glass bar — no heavy chrome; Map/Key live on inv/minimap taps
        TopHudLayout.ApplyGlass(barGo.GetComponent<Image>(), deep: true);
        TopHudLayout.AddSoftBlurLayer(barGo.transform);

        // Keep optional hooks but hide default side buttons (avoid overlap with inv/minimap)
        CreateHudNavButton(barGo.transform, font, "Map", new Vector2(0f, 0.5f), true, onMap);
        CreateHudNavButton(barGo.transform, font, "Key", new Vector2(1f, 0.5f), false, onKeyRing);
        var mapBtn = barGo.transform.Find("MapBtn");
        if (mapBtn != null) mapBtn.gameObject.SetActive(false);
        var keyBtn = barGo.transform.Find("KeyBtn");
        if (keyBtn != null) keyBtn.gameObject.SetActive(false);

        AddText(barGo.transform, "Title", font, 17, FontStyle.Bold,
            LockeKeyUITheme.White, new Vector2(0.5f, 0.5f), title, new Vector2(180f, 28f), TextAnchor.MiddleCenter);
        return barGo;
    }

    public static Text CreateToastHost(Transform parent, Font font, out CanvasGroup group)
    {
        var toastGo = new GameObject("Toast", typeof(RectTransform), typeof(Image));
        toastGo.transform.SetParent(parent, false);
        var rect = toastGo.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.12f, 0f);
        rect.anchorMax = new Vector2(0.88f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, LockeKeyUITheme.ToastBottomInset);
        rect.sizeDelta = new Vector2(0f, 44f);
        var img = toastGo.GetComponent<Image>();
        img.color = new Color(0.08f, 0.09f, 0.14f, 0.94f);
        img.raycastTarget = false;

        group = toastGo.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
        return AddText(toastGo.transform, "ToastText", font, LockeKeyUITheme.BodySize, FontStyle.Normal,
            LockeKeyUITheme.BodyText, new Vector2(0.5f, 0.5f), "", new Vector2(320f, 40f), TextAnchor.MiddleCenter);
    }

    public static GameObject CreateWoodFrame(Transform parent, RectTransform contentRect)
    {
        var frame = new GameObject("WoodFrame", typeof(RectTransform), typeof(Image));
        frame.transform.SetParent(parent, false);
        var fRect = frame.GetComponent<RectTransform>();
        fRect.anchorMin = contentRect.anchorMin;
        fRect.anchorMax = contentRect.anchorMax;
        fRect.offsetMin = contentRect.offsetMin - new Vector2(8f, 8f);
        fRect.offsetMax = contentRect.offsetMax + new Vector2(8f, 8f);
        frame.GetComponent<Image>().color = LockeKeyUITheme.LKWood;
        frame.transform.SetSiblingIndex(contentRect.GetSiblingIndex());
        return frame;
    }

    public static Text AddText(Transform parent, string name, Font font, int size, FontStyle style,
        Color color, Vector2 anchor, string text, Vector2 sizeDelta, TextAnchor alignment)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
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
        t.alignment = alignment;
        t.raycastTarget = false;
        return t;
    }

    private static void CreateHudNavButton(Transform parent, Font font, string label,
        Vector2 anchorX, bool left, UnityAction onClick)
    {
        var go = new GameObject(label + "Btn", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(left ? 0f : 1f, 0f);
        rect.anchorMax = new Vector2(left ? 0f : 1f, 1f);
        rect.pivot = new Vector2(left ? 0f : 1f, 0.5f);
        rect.sizeDelta = new Vector2(72f, 0f);
        rect.anchoredPosition = new Vector2(left ? 8f : -8f, 0f);
        go.GetComponent<Image>().color = new Color(0.14f, 0.16f, 0.24f, 0.9f);
        var navBtn = go.GetComponent<Button>();
        navBtn.onClick.AddListener(onClick);
        ApplyProductionButton(navBtn, go);
        AddText(go.transform, "Label", font, 15, FontStyle.Bold,
            left ? LockeKeyUITheme.BodyText : LockeKeyUITheme.LKGold,
            new Vector2(0.5f, 0.5f), label, new Vector2(64f, 32f), TextAnchor.MiddleCenter);
    }

    private static void AddButtonLabel(Transform parent, Font font, string label, Color color)
    {
        AddText(parent, "Label", font, LockeKeyUITheme.ButtonSize, FontStyle.Bold,
            color, new Vector2(0.5f, 0.5f), label, new Vector2(260f, 40f), TextAnchor.MiddleCenter);
        var rect = parent.Find("Label")?.GetComponent<RectTransform>();
        if (rect != null) LockeUILayout.Stretch(rect);
    }
}