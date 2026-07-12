using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Readable chapter objective card. Auto-collapses after 5 seconds; tap expands.
/// </summary>
public class ObjectiveTrackerHUD : MonoBehaviour
{
    private Text chapterText;
    private Text titleText;
    private Text keysText;
    private Text hintText;
    private Image cardBg;
    private CanvasGroup group;
    private Font font;
    private bool expanded = true;
    private float expandTimer;
    private const float CollapseAfter = 5f;
    private Vector2 expandedSize = new(260f, 78f);
    private Vector2 collapsedSize = new(200f, 28f);
    private RectTransform rect;

    public static ObjectiveTrackerHUD Ensure(Transform canvasRoot, Font font)
    {
        var existing = Object.FindFirstObjectByType<ObjectiveTrackerHUD>();
        if (existing != null)
        {
            existing.Relayout();
            return existing;
        }

        var go = new GameObject("ObjectiveTracker", typeof(RectTransform), typeof(ObjectiveTrackerHUD), typeof(Button));
        go.transform.SetParent(canvasRoot, false);
        var hud = go.GetComponent<ObjectiveTrackerHUD>();
        hud.Build(font);
        return hud;
    }

    private void Build(Font f)
    {
        font = f ?? LockeUILayout.GetUIFont();
        rect = GetComponent<RectTransform>();
        TopHudLayout.PlaceObjective(rect);
        rect.sizeDelta = expandedSize;

        cardBg = gameObject.AddComponent<Image>();
        TopHudLayout.ApplyGlass(cardBg);
        TopHudLayout.AddSoftBlurLayer(transform);
        group = gameObject.AddComponent<CanvasGroup>();

        var btn = GetComponent<Button>();
        btn.targetGraphic = cardBg;
        btn.onClick.AddListener(ToggleExpand);

        var line = new GameObject("Accent", typeof(RectTransform), typeof(Image));
        line.transform.SetParent(transform, false);
        var lRect = line.GetComponent<RectTransform>();
        lRect.anchorMin = new Vector2(0.08f, 1f);
        lRect.anchorMax = new Vector2(0.92f, 1f);
        lRect.pivot = new Vector2(0.5f, 1f);
        lRect.sizeDelta = new Vector2(0f, 2f);
        line.GetComponent<Image>().color = GameSettings.AccentColor;
        line.GetComponent<Image>().raycastTarget = false;

        chapterText = Make("Chapter", 11, FontStyle.Bold, GameSettings.AccentColor, new Vector2(0.5f, 0.82f), 240f);
        titleText = Make("Title", 14, FontStyle.Bold, Color.white, new Vector2(0.5f, 0.55f), 240f);
        keysText = Make("Keys", 11, FontStyle.Normal, LockeKeyUITheme.BodyText, new Vector2(0.5f, 0.32f), 240f);
        hintText = Make("Hint", 11, FontStyle.Normal, LockeKeyUITheme.CaptionText, new Vector2(0.5f, 0.12f), 240f);

        expandTimer = CollapseAfter;
        expanded = true;
    }

    public void Relayout()
    {
        TopHudLayout.PlaceObjective(GetComponent<RectTransform>());
        if (cardBg != null) TopHudLayout.ApplyGlass(cardBg);
    }

    private Text Make(string name, int size, FontStyle style, Color color, Vector2 anchor, float width)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(transform, false);
        var t = go.GetComponent<Text>();
        t.font = font;
        t.fontSize = Mathf.RoundToInt(size * GameSettings.SubtitleScale);
        t.fontStyle = style;
        t.color = color;
        t.alignment = TextAnchor.MiddleCenter;
        t.raycastTarget = false;
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = r.anchorMax = anchor;
        r.sizeDelta = new Vector2(width, 20f);
        return t;
    }

    private void ToggleExpand()
    {
        expanded = !expanded;
        expandTimer = expanded ? CollapseAfter : 0f;
        ApplyExpandVisual();
    }

    private void ApplyExpandVisual()
    {
        if (rect == null) return;
        rect.sizeDelta = expanded ? expandedSize : collapsedSize;
        if (keysText != null) keysText.gameObject.SetActive(expanded);
        if (hintText != null) hintText.gameObject.SetActive(expanded);
        if (chapterText != null) chapterText.gameObject.SetActive(expanded);
        // Collapsed: only title line
        if (titleText != null)
        {
            var r = titleText.GetComponent<RectTransform>();
            r.anchorMin = r.anchorMax = expanded ? new Vector2(0.5f, 0.55f) : new Vector2(0.5f, 0.5f);
        }
    }

    private void Update()
    {
        RefreshFromBeat();

        if (expanded)
        {
            expandTimer -= Time.deltaTime;
            if (expandTimer <= 0f)
            {
                expanded = false;
                ApplyExpandVisual();
            }
        }
    }

    private void RefreshFromBeat()
    {
        var beat = FindFirstObjectByType<ChapterBeatDirector>();
        if (beat == null || titleText == null) return;

        int keys = CountKeys();
        if (keysText != null)
            keysText.text = $"{keys} / 4 Keys";

        if (chapterText != null)
            chapterText.text = "Chapter 1";

        switch (beat.CurrentBeat)
        {
            case ChapterBeatDirector.Beat.Arrival:
                Set("Find the House Key", "Follow the glowing trail");
                break;
            case ChapterBeatDirector.Beat.StuckDoor:
                Set("Unlock the Front Door", "Go to the highlighted door");
                break;
            case ChapterBeatDirector.Beat.Library:
                Set("Clear the Bookshelf", "Inspect, then shove twice");
                break;
            case ChapterBeatDirector.Beat.GhostKeyUse:
                Set("Phase Through the Seal", "Use Key, then walk through");
                break;
            case ChapterBeatDirector.Beat.EchoEncounter:
                Set("Escape the Echo", "Hide in the arch, then run");
                break;
            case ChapterBeatDirector.Beat.Aftermath:
                Set("Uncover a Memory", "Head Key → family portrait");
                break;
            case ChapterBeatDirector.Beat.MemorySolved:
                Set("Claim the Hidden Key", "Ghost-phase the cold wall");
                break;
            case ChapterBeatDirector.Beat.ChapterComplete:
                Set("Chapter Complete", "The Black Door waits…");
                break;
            default:
                Set("Explore Keyhouse", "Follow the objective trail");
                break;
        }
    }

    private static int CountKeys()
    {
        int n = 0;
        var inv = Object.FindFirstObjectByType<PlayerInventory>();
        var km = Object.FindFirstObjectByType<KeyManager>();
        if (inv != null && inv.HasHouseKey) n++;
        if (km != null)
        {
            if (km.ownedKeys.Exists(k => k.abilityType == KeyManager.KeyAbilityType.GhostPhase)) n++;
            if (km.ownedKeys.Exists(k => k.abilityType == KeyManager.KeyAbilityType.HeadMemory)) n++;
            if (km.ownedKeys.Exists(k => k.abilityType == KeyManager.KeyAbilityType.MirrorTravel)) n++;
        }
        return n;
    }

    private void Set(string title, string hint)
    {
        if (titleText != null) titleText.text = title;
        if (hintText != null) hintText.text = hint;
    }

    /// <summary>Call when a major beat advances so the card re-opens briefly.</summary>
    public void Peek()
    {
        expanded = true;
        expandTimer = CollapseAfter;
        ApplyExpandVisual();
    }
}
