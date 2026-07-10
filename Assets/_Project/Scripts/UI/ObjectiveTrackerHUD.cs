using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Modern quest tracker card (The Room / adventure-game style).
/// </summary>
public class ObjectiveTrackerHUD : MonoBehaviour
{
    private Text titleText;
    private Text bodyText;
    private Text stepText;
    private Image iconImage;
    private Image cardBg;
    private CanvasGroup group;
    private Font font;

    public static ObjectiveTrackerHUD Ensure(Transform canvasRoot, Font font)
    {
        var existing = Object.FindFirstObjectByType<ObjectiveTrackerHUD>();
        if (existing != null) return existing;

        var go = new GameObject("ObjectiveTracker", typeof(RectTransform), typeof(ObjectiveTrackerHUD));
        go.transform.SetParent(canvasRoot, false);
        var hud = go.GetComponent<ObjectiveTrackerHUD>();
        hud.Build(font);
        return hud;
    }

    private void Build(Font f)
    {
        font = f ?? LockeUILayout.GetUIFont();
        var scale = GameSettings.UiScale;

        var rect = GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -56f);
        rect.sizeDelta = new Vector2(340f * scale, 86f * scale);

        group = gameObject.AddComponent<CanvasGroup>();

        cardBg = gameObject.AddComponent<Image>();
        cardBg.color = new Color(0.06f, 0.07f, 0.11f, 0.88f);
        var outline = gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(GameSettings.AccentColor.r, GameSettings.AccentColor.g, GameSettings.AccentColor.b, 0.45f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        // Left accent bar
        var accent = new GameObject("Accent", typeof(RectTransform), typeof(Image));
        accent.transform.SetParent(transform, false);
        var aRect = accent.GetComponent<RectTransform>();
        aRect.anchorMin = new Vector2(0f, 0f);
        aRect.anchorMax = new Vector2(0f, 1f);
        aRect.pivot = new Vector2(0f, 0.5f);
        aRect.sizeDelta = new Vector2(4f, 0f);
        accent.GetComponent<Image>().color = GameSettings.AccentColor;

        // Icon circle
        var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconGo.transform.SetParent(transform, false);
        var iRect = iconGo.GetComponent<RectTransform>();
        iRect.anchorMin = iRect.anchorMax = new Vector2(0f, 0.5f);
        iRect.pivot = new Vector2(0f, 0.5f);
        iRect.anchoredPosition = new Vector2(14f, 0f);
        iRect.sizeDelta = new Vector2(36f, 36f);
        iconImage = iconGo.GetComponent<Image>();
        iconImage.color = GameSettings.AccentColor;

        stepText = MakeText("Step", 11, FontStyle.Bold, GameSettings.AccentColor,
            new Vector2(0.18f, 0.78f), new Vector2(200f, 18f), TextAnchor.MiddleLeft);
        titleText = MakeText("Title", 16, FontStyle.Bold, Color.white,
            new Vector2(0.18f, 0.48f), new Vector2(250f, 24f), TextAnchor.MiddleLeft);
        bodyText = MakeText("Body", 12, FontStyle.Normal, LockeKeyUITheme.BodyText,
            new Vector2(0.18f, 0.22f), new Vector2(250f, 20f), TextAnchor.MiddleLeft);
    }

    private Text MakeText(string name, int size, FontStyle style, Color color, Vector2 anchor, Vector2 sizeDelta, TextAnchor align)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(transform, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = new Vector2(0f, 0.5f);
        rect.sizeDelta = sizeDelta;
        var t = go.GetComponent<Text>();
        t.font = font;
        t.fontSize = Mathf.RoundToInt(size * GameSettings.SubtitleScale);
        t.fontStyle = style;
        t.color = color;
        t.alignment = align;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.raycastTarget = false;
        return t;
    }

    private void Update()
    {
        RefreshFromBeat();
    }

    private void RefreshFromBeat()
    {
        var beat = FindFirstObjectByType<ChapterBeatDirector>();
        if (beat == null || titleText == null) return;

        var guide = FindFirstObjectByType<ObjectiveGuideController>();
        string targetName = guide != null && !string.IsNullOrEmpty(guide.TargetLabel)
            ? guide.TargetLabel
            : "Objective";

        switch (beat.CurrentBeat)
        {
            case ChapterBeatDirector.Beat.Arrival:
                Set("CURRENT OBJECTIVE", "Collect the House Key", "Follow the glowing trail", 1, 6);
                break;
            case ChapterBeatDirector.Beat.StuckDoor:
                Set("CURRENT OBJECTIVE", "Unlock the Front Door", $"Go to the highlighted {targetName}", 2, 6);
                break;
            case ChapterBeatDirector.Beat.Library:
                Set("CURRENT OBJECTIVE", "Clear the Bookshelf", "Reveal the Ghost Key alcove", 3, 6);
                break;
            case ChapterBeatDirector.Beat.GhostKeyUse:
                Set("CURRENT OBJECTIVE", "Phase Through the Seal", "Use Key, then walk through", 4, 6);
                break;
            case ChapterBeatDirector.Beat.EchoEncounter:
                Set("DANGER", "Escape the Echo", "Hide in the arch or keep running", 5, 6);
                break;
            default:
                Set("CURRENT OBJECTIVE", "Uncover a Memory", "Claim Head Key → family portrait", 6, 6);
                break;
        }
    }

    private void Set(string step, string title, string body, int n, int total)
    {
        if (stepText != null) stepText.text = $"{step}  ·  {n}/{total}";
        if (titleText != null) titleText.text = title;
        if (bodyText != null) bodyText.text = body;
    }
}
