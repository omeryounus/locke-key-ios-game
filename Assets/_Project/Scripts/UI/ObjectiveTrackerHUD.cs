using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Compact centered objective pill beneath the title bar.
/// </summary>
public class ObjectiveTrackerHUD : MonoBehaviour
{
    private Text titleText;
    private Text stepText;
    private Image cardBg;
    private Font font;

    public static ObjectiveTrackerHUD Ensure(Transform canvasRoot, Font font)
    {
        var existing = Object.FindFirstObjectByType<ObjectiveTrackerHUD>();
        if (existing != null)
        {
            existing.Relayout();
            return existing;
        }

        var go = new GameObject("ObjectiveTracker", typeof(RectTransform), typeof(ObjectiveTrackerHUD));
        go.transform.SetParent(canvasRoot, false);
        var hud = go.GetComponent<ObjectiveTrackerHUD>();
        hud.Build(font);
        return hud;
    }

    private void Build(Font f)
    {
        font = f ?? LockeUILayout.GetUIFont();
        var rect = GetComponent<RectTransform>();
        TopHudLayout.PlaceObjective(rect);

        cardBg = gameObject.AddComponent<Image>();
        TopHudLayout.ApplyGlass(cardBg);
        TopHudLayout.AddSoftBlurLayer(transform);

        // Thin gold accent line on top edge (not a heavy border)
        var line = new GameObject("AccentLine", typeof(RectTransform), typeof(Image));
        line.transform.SetParent(transform, false);
        var lRect = line.GetComponent<RectTransform>();
        lRect.anchorMin = new Vector2(0.12f, 1f);
        lRect.anchorMax = new Vector2(0.88f, 1f);
        lRect.pivot = new Vector2(0.5f, 1f);
        lRect.sizeDelta = new Vector2(0f, 2f);
        line.GetComponent<Image>().color = new Color(
            GameSettings.AccentColor.r, GameSettings.AccentColor.g, GameSettings.AccentColor.b, 0.55f);
        line.GetComponent<Image>().raycastTarget = false;

        stepText = MakeText("Step", 10, FontStyle.Bold, GameSettings.AccentColor,
            new Vector2(0.5f, 0.72f), new Vector2(220f, 14f), TextAnchor.MiddleCenter);
        titleText = MakeText("Title", 13, FontStyle.Bold, Color.white,
            new Vector2(0.5f, 0.32f), new Vector2(220f, 18f), TextAnchor.MiddleCenter);
    }

    public void Relayout()
    {
        TopHudLayout.PlaceObjective(GetComponent<RectTransform>());
        if (cardBg != null) TopHudLayout.ApplyGlass(cardBg);
    }

    private Text MakeText(string name, int size, FontStyle style, Color color, Vector2 anchor, Vector2 sizeDelta, TextAnchor align)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(transform, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = sizeDelta;
        var t = go.GetComponent<Text>();
        t.font = font;
        t.fontSize = Mathf.RoundToInt(size * GameSettings.SubtitleScale);
        t.fontStyle = style;
        t.color = color;
        t.alignment = align;
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
                Set("QUEST 1/6", "Collect the House Key");
                break;
            case ChapterBeatDirector.Beat.StuckDoor:
                Set("QUEST 2/6", "Unlock the Front Door");
                break;
            case ChapterBeatDirector.Beat.Library:
                Set("QUEST 3/6", "Clear the Bookshelf");
                break;
            case ChapterBeatDirector.Beat.GhostKeyUse:
                Set("QUEST 4/6", "Phase Through the Seal");
                break;
            case ChapterBeatDirector.Beat.EchoEncounter:
                Set("DANGER", "Escape the Echo");
                break;
            default:
                Set("QUEST 6/6", "Uncover a Memory");
                break;
        }
    }

    private void Set(string step, string title)
    {
        if (stepText != null) stepText.text = step;
        if (titleText != null) titleText.text = title;
    }
}
