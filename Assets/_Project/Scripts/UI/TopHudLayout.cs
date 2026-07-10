using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Single owner of top HUD slots so inventory, title, objective, and minimap never overlap.
/// Layout (ref 393×852):
///   Row 0 (y=-6..-46):  [Inv 40] .... Title .... [MiniMap 96]
///   Row 1 (y=-50..-82):  ........ Objective pill (center) ........
/// </summary>
public static class TopHudLayout
{
    public const float BarHeight = 44f;
    public const float InvSize = 40f;
    public const float MiniW = 96f;
    public const float MiniH = 56f;
    public const float ObjectiveW = 260f;
    public const float ObjectiveH = 78f;
    public const float EdgePad = 10f;
    public const float TopInset = 6f;

    /// <summary>Semi-transparent dark glass (blur approximated with layered soft panels).</summary>
    public static readonly Color Glass = new(0.06f, 0.07f, 0.11f, 0.62f);
    public static readonly Color GlassDeep = new(0.04f, 0.05f, 0.08f, 0.45f);

    public static void ApplyGlass(Image img, bool deep = false)
    {
        if (img == null) return;
        img.color = deep ? GlassDeep : Glass;
        // Remove harsh outlines if present
        var outline = img.GetComponent<Outline>();
        if (outline != null)
            Object.Destroy(outline);
    }

    public static void AddSoftBlurLayer(Transform parent)
    {
        if (parent == null || parent.Find("BlurLayer") != null) return;
        var go = new GameObject("BlurLayer", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        go.transform.SetAsFirstSibling();
        LockeUILayout.Stretch(go.GetComponent<RectTransform>());
        var img = go.GetComponent<Image>();
        img.color = new Color(0.08f, 0.09f, 0.12f, 0.35f);
        img.raycastTarget = false;
    }

    public static void PlaceInventory(RectTransform rect)
    {
        if (rect == null) return;
        rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(EdgePad, -TopInset);
        rect.sizeDelta = new Vector2(InvSize, InvSize);
    }

    public static void PlaceMinimap(RectTransform rect)
    {
        if (rect == null) return;
        rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-EdgePad, -TopInset);
        rect.sizeDelta = new Vector2(MiniW, MiniH);
    }

    public static void PlaceObjective(RectTransform rect)
    {
        if (rect == null) return;
        // Centered under title row
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -(TopInset + BarHeight + 4f));
        rect.sizeDelta = new Vector2(ObjectiveW, ObjectiveH);
    }

    public static void StyleTitleBar(GameObject barGo)
    {
        if (barGo == null) return;
        var img = barGo.GetComponent<Image>();
        ApplyGlass(img, deep: true);
        AddSoftBlurLayer(barGo.transform);

        var rect = barGo.GetComponent<RectTransform>();
        if (rect != null)
            rect.sizeDelta = new Vector2(0f, BarHeight);

        // Hide default Map/Key nav that fights inventory + minimap (re-wire map via minimap tap if needed)
        var mapBtn = barGo.transform.Find("MapBtn");
        if (mapBtn != null) mapBtn.gameObject.SetActive(false);
        var keyBtn = barGo.transform.Find("KeyBtn");
        if (keyBtn != null) keyBtn.gameObject.SetActive(false);
    }

    public static void HideLegacyTopChrome(Transform canvasRoot)
    {
        if (canvasRoot == null) return;
        Hide(canvasRoot, "KeySlot");
        Hide(canvasRoot, "KeyStatusIcon");
        Hide(canvasRoot, "HouseKeyIcon");
        Hide(canvasRoot, "KeyStatus");
        Hide(canvasRoot, "HouseKeyStatus");
        Hide(canvasRoot, "SettingsBtn");
    }

    private static void Hide(Transform root, string name)
    {
        var t = root.Find(name);
        if (t != null) t.gameObject.SetActive(false);
    }
}
