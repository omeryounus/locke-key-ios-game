using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Dev/testing menu for Chapter 1 save: New Game, Continue, Reset.
/// </summary>
public class ChapterSaveDebugMenu : MonoBehaviour
{
    [SerializeField] private bool startExpanded;
    [SerializeField] private GameplayHUD hud;

    private GameObject panelRoot;
    private Text statusText;
    private Button continueButton;

    public void BindHud(GameplayHUD gameplayHud) => hud = gameplayHud;

    private void Start()
    {
        if (hud == null)
            hud = FindFirstObjectByType<GameplayHUD>();

        BuildMenu();
        RefreshState();
        SetExpanded(startExpanded);
    }

    private void OnEnable()
    {
        RefreshState();
    }

    public void RefreshState()
    {
        if (statusText == null) return;

        var save = ChapterSaveManager.Instance;
        if (save == null)
        {
            statusText.text = "Save: unavailable";
            if (continueButton != null)
                continueButton.interactable = false;
            return;
        }

        statusText.text = save.HasContinuableSave
            ? save.SaveSummary
            : "No chapter progress saved";

        if (continueButton != null)
            continueButton.interactable = save.HasContinuableSave;
    }

    private void SetExpanded(bool expanded)
    {
        if (panelRoot != null)
            panelRoot.SetActive(expanded);
    }

    private void BuildMenu()
    {
        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var panelColor = new Color(0.05f, 0.07f, 0.12f, 0.9f);
        var buttonColor = new Color(0.16f, 0.2f, 0.3f, 0.95f);
        var accent = new Color(0.55f, 0.78f, 0.95f, 1f);

        var toggleGo = CreateRect(canvas.transform, "SaveDebugToggle");
        var toggleRect = toggleGo.GetComponent<RectTransform>();
        toggleRect.anchorMin = new Vector2(1f, 1f);
        toggleRect.anchorMax = new Vector2(1f, 1f);
        toggleRect.pivot = new Vector2(1f, 1f);
        toggleRect.anchoredPosition = new Vector2(-16f, -16f);
        toggleRect.sizeDelta = new Vector2(120f, 40f);
        var toggleImage = toggleGo.AddComponent<Image>();
        toggleImage.color = buttonColor;
        var toggle = toggleGo.AddComponent<Button>();
        toggle.targetGraphic = toggleImage;
        toggle.onClick.AddListener(() => SetExpanded(panelRoot != null && !panelRoot.activeSelf));
        CreateLabel(toggleGo.transform, "Save", font, 18, accent);

        panelRoot = CreateRect(canvas.transform, "SaveDebugPanel");
        var panelRect = panelRoot.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1f, 1f);
        panelRect.anchorMax = new Vector2(1f, 1f);
        panelRect.pivot = new Vector2(1f, 1f);
        panelRect.anchoredPosition = new Vector2(-16f, -64f);
        panelRect.sizeDelta = new Vector2(300f, 220f);
        var panelImage = panelRoot.AddComponent<Image>();
        panelImage.color = panelColor;

        statusText = CreateLabel(panelRoot.transform, "Status", font, 16, Color.white,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(260f, 48f));

        continueButton = CreateActionButton(panelRoot.transform, "Continue", font, buttonColor, accent,
            new Vector2(0.5f, 1f), new Vector2(-130f, -88f), HandleContinue);
        CreateActionButton(panelRoot.transform, "New Game", font, buttonColor, accent,
            new Vector2(0.5f, 1f), new Vector2(0f, -88f), HandleNewGame);
        CreateActionButton(panelRoot.transform, "Reset Save", font, buttonColor, accent,
            new Vector2(0.5f, 1f), new Vector2(-130f, -148f), HandleReset);
        CreateActionButton(panelRoot.transform, "Save Now", font, buttonColor, accent,
            new Vector2(0.5f, 1f), new Vector2(0f, -148f), HandleSaveNow);
    }

    private void HandleNewGame()
    {
        if (ChapterSaveManager.Instance == null) return;
        ChapterSaveManager.Instance.StartNewGame();
        hud?.ShowToast("New Game — chapter restarted.", 2.5f);
    }

    private void HandleContinue()
    {
        if (ChapterSaveManager.Instance == null || !ChapterSaveManager.Instance.HasContinuableSave)
        {
            hud?.ShowToast("No saved chapter to continue.", 2.5f);
            return;
        }

        ChapterSaveManager.Instance.ContinueGame();
        hud?.ShowToast("Continuing saved chapter...", 2.5f);
    }

    private void HandleReset()
    {
        if (ChapterSaveManager.Instance == null) return;
        ChapterSaveManager.Instance.ResetChapterSaveAndReload();
        hud?.ShowToast("Chapter save reset.", 2.5f);
    }

    private void HandleSaveNow()
    {
        if (ChapterSaveManager.Instance == null) return;
        ChapterSaveManager.Instance.SaveNow();
        RefreshState();
        hud?.ShowToast("Progress saved.", 2f);
    }

    private static GameObject CreateRect(Transform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static Text CreateLabel(Transform parent, string name, Font font, int size, Color color,
        Vector2? anchor = null, Vector2? pivot = null, Vector2? pos = null, Vector2? sizeDelta = null)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var text = go.GetComponent<Text>();
        text.font = font;
        text.fontSize = size;
        text.color = color;
        text.alignment = TextAnchor.MiddleCenter;
        var rect = go.GetComponent<RectTransform>();
        if (anchor.HasValue)
        {
            rect.anchorMin = anchor.Value;
            rect.anchorMax = anchor.Value;
            rect.pivot = pivot ?? anchor.Value;
            rect.anchoredPosition = pos ?? Vector2.zero;
            rect.sizeDelta = sizeDelta ?? new Vector2(120f, 40f);
        }
        else
        {
            var parentRect = rect.parent as RectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        return text;
    }

    private static Button CreateActionButton(Transform parent, string label, Font font, Color bg, Color textColor,
        Vector2 anchor, Vector2 pos, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject(label + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.color = bg;
        var button = go.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);

        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = pos;
        rect.sizeDelta = new Vector2(120f, 40f);

        CreateLabel(go.transform, "Label", font, 17, textColor);
        return button;
    }
}