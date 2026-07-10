using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// In-game settings: brightness, UI scale, left-handed, colorblind, subtitle size.
/// </summary>
public class AccessibilitySettingsPanel : MonoBehaviour
{
    private CanvasGroup group;
    private Font font;
    private Text brightnessLabel;
    private Text uiScaleLabel;

    public static AccessibilitySettingsPanel Ensure(Transform canvasRoot, Font font)
    {
        var existing = Object.FindFirstObjectByType<AccessibilitySettingsPanel>();
        if (existing != null) return existing;
        var go = new GameObject("AccessibilitySettings", typeof(RectTransform), typeof(AccessibilitySettingsPanel));
        go.transform.SetParent(canvasRoot, false);
        var panel = go.GetComponent<AccessibilitySettingsPanel>();
        panel.Build(font);
        return panel;
    }

    private void Build(Font f)
    {
        font = f ?? LockeUILayout.GetUIFont();
        var rect = GetComponent<RectTransform>();
        LockeUILayout.Stretch(rect);

        group = gameObject.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;

        var scrim = new GameObject("Scrim", typeof(RectTransform), typeof(Image), typeof(Button));
        scrim.transform.SetParent(transform, false);
        LockeUILayout.Stretch(scrim.GetComponent<RectTransform>());
        scrim.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);
        scrim.GetComponent<Button>().onClick.AddListener(Hide);

        var sheet = new GameObject("Sheet", typeof(RectTransform), typeof(Image));
        sheet.transform.SetParent(transform, false);
        var sRect = sheet.GetComponent<RectTransform>();
        sRect.anchorMin = new Vector2(0.08f, 0.18f);
        sRect.anchorMax = new Vector2(0.92f, 0.82f);
        sRect.offsetMin = sRect.offsetMax = Vector2.zero;
        sheet.GetComponent<Image>().color = new Color(0.08f, 0.09f, 0.13f, 0.96f);

        AddLabel(sheet.transform, "Title", "Settings", 22, FontStyle.Bold, GameSettings.AccentColor,
            new Vector2(0.5f, 0.92f), new Vector2(280f, 32f));

        brightnessLabel = AddLabel(sheet.transform, "Bright", $"Brightness  {GameSettings.Brightness:0.00}", 14,
            FontStyle.Normal, Color.white, new Vector2(0.5f, 0.78f), new Vector2(280f, 24f));
        AddButton(sheet.transform, "B-", new Vector2(0.28f, 0.68f), () => AdjustBrightness(-0.08f));
        AddButton(sheet.transform, "B+", new Vector2(0.72f, 0.68f), () => AdjustBrightness(0.08f));

        uiScaleLabel = AddLabel(sheet.transform, "UiScale", $"UI Scale  {GameSettings.UiScale:0.00}", 14,
            FontStyle.Normal, Color.white, new Vector2(0.5f, 0.55f), new Vector2(280f, 24f));
        AddButton(sheet.transform, "UI-", new Vector2(0.28f, 0.45f), () => AdjustUiScale(-0.05f));
        AddButton(sheet.transform, "UI+", new Vector2(0.72f, 0.45f), () => AdjustUiScale(0.05f));

        AddButton(sheet.transform, GameSettings.LeftHanded ? "Left-Handed: ON" : "Left-Handed: OFF",
            new Vector2(0.5f, 0.32f), ToggleLeft, width: 220f);
        AddButton(sheet.transform, GameSettings.ColorblindMode == 1 ? "Colorblind: ON" : "Colorblind: OFF",
            new Vector2(0.5f, 0.2f), ToggleColorblind, width: 220f);
        AddButton(sheet.transform, "Close", new Vector2(0.5f, 0.08f), Hide, width: 160f);
    }

    private void AdjustBrightness(float d)
    {
        GameSettings.Brightness = GameSettings.Brightness + d;
        if (brightnessLabel != null)
            brightnessLabel.text = $"Brightness  {GameSettings.Brightness:0.00}";
        FindFirstObjectByType<SceneAtmosphereController>()?.ApplyBrightness(GameSettings.Brightness);
    }

    private void AdjustUiScale(float d)
    {
        GameSettings.UiScale = GameSettings.UiScale + d;
        if (uiScaleLabel != null)
            uiScaleLabel.text = $"UI Scale  {GameSettings.UiScale:0.00}";
    }

    private void ToggleLeft()
    {
        GameSettings.LeftHanded = !GameSettings.LeftHanded;
        FindFirstObjectByType<GameplayHUD>()?.ApplyLeftHandedLayout();
        Hide();
        Show();
    }

    private void ToggleColorblind()
    {
        GameSettings.ColorblindMode = GameSettings.ColorblindMode == 0 ? 1 : 0;
    }

    public void Show()
    {
        group.alpha = 1f;
        group.interactable = true;
        group.blocksRaycasts = true;
        transform.SetAsLastSibling();
    }

    public void Hide()
    {
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
    }

    private Text AddLabel(Transform parent, string name, string text, int size, FontStyle style, Color color,
        Vector2 anchor, Vector2 sizeDelta)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = anchor;
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

    private void AddButton(Transform parent, string label, Vector2 anchor, UnityEngine.Events.UnityAction onClick,
        float width = 64f)
    {
        LockeUIComponents.CreateSecondaryButton(parent, font, label, anchor, onClick, width);
    }
}
