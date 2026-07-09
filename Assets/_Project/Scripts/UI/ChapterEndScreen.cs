using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shown when Chapter 1 is complete — return to title or play again.
/// </summary>
public class ChapterEndScreen : MonoBehaviour
{
    [SerializeField] private GameplayHUD hud;

    private EventBus eventBus;
    private GameObject overlayRoot;

    private void Awake()
    {
        if (hud == null)
            hud = FindFirstObjectByType<GameplayHUD>();

        eventBus = Resources.Load<EventBus>("EventBus");
        if (eventBus != null)
            eventBus.OnChapterCompleted += ShowEndCard;
    }

    private void OnDestroy()
    {
        if (eventBus != null)
            eventBus.OnChapterCompleted -= ShowEndCard;
    }

    private void ShowEndCard()
    {
        if (overlayRoot != null)
        {
            overlayRoot.SetActive(true);
            return;
        }

        BuildOverlay();
    }

    private void BuildOverlay()
    {
        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        overlayRoot = new GameObject("ChapterEndOverlay", typeof(RectTransform), typeof(Image));
        overlayRoot.transform.SetParent(canvas.transform, false);
        var bg = overlayRoot.GetComponent<Image>();
        bg.color = new Color(0.02f, 0.03f, 0.07f, 0.88f);
        bg.raycastTarget = true;

        var rect = overlayRoot.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        CreateLabel(overlayRoot.transform, "Heading", font, 44, new Color(0.9f, 0.88f, 0.72f, 1f),
            new Vector2(0.5f, 0.62f), "Chapter 1 Complete", 900f, 56f);
        CreateLabel(overlayRoot.transform, "Body", font, 22, new Color(0.82f, 0.8f, 0.76f, 1f),
            new Vector2(0.5f, 0.5f),
            "You escaped the Echo and claimed the Head Key.\nThe house remembers more than it shows.",
            860f, 100f);
        CreateLabel(overlayRoot.transform, "Tease", font, 18, new Color(0.55f, 0.72f, 0.9f, 1f),
            new Vector2(0.5f, 0.38f), "Chapter 2 — coming soon.", 700f, 36f);

        CreateButton(overlayRoot.transform, "Main Menu", font, new Vector2(0.5f, 0.26f), HandleMainMenu);
        CreateButton(overlayRoot.transform, "Play Again", font, new Vector2(0.5f, 0.16f), HandlePlayAgain);
    }

    private void HandleMainMenu()
    {
        ChapterSaveManager.ReturnToTitle();
    }

    private void HandlePlayAgain()
    {
        ChapterSaveManager.ReplayChapterFromEnd();
    }

    private static void CreateLabel(Transform parent, string name, Font font, int size, Color color,
        Vector2 anchor, string text, float width, float height)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var label = go.GetComponent<Text>();
        label.font = font;
        label.fontSize = size;
        label.color = color;
        label.alignment = TextAnchor.MiddleCenter;
        label.text = text;
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(width, height);
    }

    private static void CreateButton(Transform parent, string label, Font font, Vector2 anchor,
        UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject(label + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.color = new Color(0.16f, 0.2f, 0.3f, 0.95f);
        var button = go.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);

        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(320f, 52f);

        var textGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
        textGo.transform.SetParent(go.transform, false);
        var text = textGo.GetComponent<Text>();
        text.font = font;
        text.fontSize = 22;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        text.text = label;
        var textRect = textGo.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }
}