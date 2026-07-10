using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Compact quest tracker — top-left, not a yellow full-width instruction bar.
/// </summary>
public class ObjectiveTrackerHUD : MonoBehaviour
{
    private Text titleText;
    private Text bodyText;
    private Text stepText;
    private Image cardBg;
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
        // Top-left compact card
        rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(10f, -52f);
        rect.sizeDelta = new Vector2(200f * scale, 68f * scale);

        cardBg = gameObject.AddComponent<Image>();
        cardBg.color = new Color(0.05f, 0.06f, 0.1f, 0.82f);
        var outline = gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(GameSettings.AccentColor.r, GameSettings.AccentColor.g, GameSettings.AccentColor.b, 0.4f);
        outline.effectDistance = new Vector2(1.2f, -1.2f);

        var accent = new GameObject("Accent", typeof(RectTransform), typeof(Image));
        accent.transform.SetParent(transform, false);
        var aRect = accent.GetComponent<RectTransform>();
        aRect.anchorMin = new Vector2(0f, 0f);
        aRect.anchorMax = new Vector2(0f, 1f);
        aRect.pivot = new Vector2(0f, 0.5f);
        aRect.sizeDelta = new Vector2(3f, 0f);
        accent.GetComponent<Image>().color = GameSettings.AccentColor;

        // Quest diamond icon
        var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconGo.transform.SetParent(transform, false);
        var iRect = iconGo.GetComponent<RectTransform>();
        iRect.anchorMin = iRect.anchorMax = new Vector2(0f, 0.5f);
        iRect.pivot = new Vector2(0f, 0.5f);
        iRect.anchoredPosition = new Vector2(10f, 0f);
        iRect.sizeDelta = new Vector2(14f, 14f);
        iconGo.GetComponent<Image>().color = GameSettings.AccentColor;

        stepText = MakeText("Step", 10, FontStyle.Bold, GameSettings.AccentColor,
            new Vector2(0.12f, 0.78f), new Vector2(160f, 16f));
        titleText = MakeText("Title", 13, FontStyle.Bold, Color.white,
            new Vector2(0.12f, 0.48f), new Vector2(170f, 20f));
        bodyText = MakeText("Body", 11, FontStyle.Normal, LockeKeyUITheme.BodyText,
            new Vector2(0.12f, 0.2f), new Vector2(170f, 16f));
    }

    private Text MakeText(string name, int size, FontStyle style, Color color, Vector2 anchor, Vector2 sizeDelta)
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
        t.alignment = TextAnchor.MiddleLeft;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.raycastTarget = false;
        return t;
    }

    private void Update() => RefreshFromBeat();

    private void RefreshFromBeat()
    {
        var beat = FindFirstObjectByType<ChapterBeatDirector>();
        if (beat == null || titleText == null) return;

        switch (beat.CurrentBeat)
        {
            case ChapterBeatDirector.Beat.Arrival:
                Set("QUEST  1/6", "Collect House Key", "Follow the glow");
                break;
            case ChapterBeatDirector.Beat.StuckDoor:
                Set("QUEST  2/6", "Unlock Front Door", "Go to highlighted door");
                break;
            case ChapterBeatDirector.Beat.Library:
                Set("QUEST  3/6", "Clear Bookshelf", "Tap Interact once");
                break;
            case ChapterBeatDirector.Beat.GhostKeyUse:
                Set("QUEST  4/6", "Phase the Seal", "Use Key, walk through");
                break;
            case ChapterBeatDirector.Beat.EchoEncounter:
                Set("DANGER", "Escape the Echo", "Hide or run");
                break;
            default:
                Set("QUEST  6/6", "Uncover Memory", "Head Key → portrait");
                break;
        }
    }

    private void Set(string step, string title, string body)
    {
        if (stepText != null) stepText.text = step;
        if (titleText != null) titleText.text = title;
        if (bodyText != null) bodyText.text = body;
    }
}
