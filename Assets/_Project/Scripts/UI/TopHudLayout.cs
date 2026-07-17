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
    public const float BarHeight = 0f;
    public const float InvSize = 40f;
    public const float InvPanelW = 52f;
    public const float InvPanelH = 48f;
    public const float MiniW = 52f;
    public const float MiniH = 48f;
    public const float ObjectiveW = 304f;
    public const float ObjectiveH = 40f;
    public const float EdgePad = 12f;
    public const float TopInset = 8f;

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
        rect.sizeDelta = new Vector2(InvPanelW, InvPanelH);
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
        // One focused objective below the compact key/map controls.
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -(TopInset + InvPanelH + 6f));
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

        // Map and key actions live in the compact controls, not a competing title bar.
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
