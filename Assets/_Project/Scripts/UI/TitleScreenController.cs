using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Player-facing title screen: Continue and New Game.
/// </summary>
public class TitleScreenController : MonoBehaviour
{
    [SerializeField] private string chapterBlurb =
        "Return to Keyhouse. Find the keys. Some doors were never meant to open.";

    private Button continueButton;

    private void Start()
    {
        EnsureEventSystem();
        BuildUi();
        RefreshButtons();
    }

    private void RefreshButtons()
    {
        if (continueButton == null) return;
        continueButton.interactable = ChapterSaveManager.HasContinuableSaveOnDisk();
    }

    private void HandleContinue()
    {
        if (!ChapterSaveManager.HasContinuableSaveOnDisk())
            return;

        ChapterSaveManager.ContinueFromTitle();
    }

    private void HandleNewGame()
    {
        ChapterSaveManager.StartNewGameFromTitle();
    }

    private void BuildUi()
    {
        var canvasGo = new GameObject("TitleCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler),
            typeof(GraphicRaycaster));
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;

        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var bg = CreatePanel(canvasGo.transform, "Background", new Color(0.03f, 0.05f, 0.1f, 1f),
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        CreateLabel(bg.transform, "Title", font, 56, new Color(0.88f, 0.9f, 0.96f, 1f),
            new Vector2(0.5f, 0.72f), "Locke & Key", 900f, 72f);
        CreateLabel(bg.transform, "Subtitle", font, 30, new Color(0.55f, 0.75f, 0.95f, 1f),
            new Vector2(0.5f, 0.62f), "Keyhouse — Chapter 1", 900f, 44f);
        CreateLabel(bg.transform, "Blurb", font, 22, new Color(0.78f, 0.76f, 0.72f, 1f),
            new Vector2(0.5f, 0.5f), chapterBlurb, 820f, 90f);

        continueButton = CreateButton(bg.transform, "Continue", font,
            new Vector2(0.5f, 0.34f), HandleContinue);
        CreateButton(bg.transform, "New Game", font,
            new Vector2(0.5f, 0.24f), HandleNewGame);

        var progress = ChapterSaveManager.ReadSaveSummaryFromDisk();
        if (!string.IsNullOrEmpty(progress))
        {
            CreateLabel(bg.transform, "Progress", font, 18, new Color(0.65f, 0.68f, 0.74f, 1f),
                new Vector2(0.5f, 0.16f), progress, 760f, 36f);
        }
    }

    private static void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null)
            return;

        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();
    }

    private static GameObject CreatePanel(Transform parent, string name, Color color,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = color;
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = sizeDelta;
        rect.offsetMin = anchoredPos;
        rect.offsetMax = sizeDelta;
        return go;
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

    private static Button CreateButton(Transform parent, string label, Font font, Vector2 anchor,
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
        rect.sizeDelta = new Vector2(320f, 56f);

        var textGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
        textGo.transform.SetParent(go.transform, false);
        var text = textGo.GetComponent<Text>();
        text.font = font;
        text.fontSize = 24;
        text.color = new Color(0.9f, 0.92f, 0.98f, 1f);
        text.alignment = TextAnchor.MiddleCenter;
        text.text = label;
        var textRect = textGo.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        return button;
    }
}