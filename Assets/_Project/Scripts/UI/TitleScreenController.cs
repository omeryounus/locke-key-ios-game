using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// S0 Splash + S1 Story Reel title flow.
///
/// Flow:
///   hasCompletedOnboarding = false  →  S0 Splash → S1 Reel → Chapter1 (map opens)
///   hasCompletedOnboarding = true   →  S0 Splash → Continue / New Game → Chapter1 (map opens)
/// </summary>
public class TitleScreenController : MonoBehaviour
{
    // ── reference resolution (portrait iPhone 15) ──────────────────────
    private const float REF_W = 393f;
    private const float REF_H = 852f;

    // ── story lore lines shown beneath each slide ───────────────────────
    private static readonly string[] StoryLore =
    {
        "You return to Keyhouse after years away.\nSomething is wrong with the doors.",
        "The first key found beneath the floorboards.\nA rusted thing with too much weight.",
        "The wellhouse hums at night.\nAn echo of something that should not be.",
        "The black door at the end of the hall\nhas no keyhole. It never did."
    };

    private static readonly string[] StoryPaths =
    {
        ArtPaths.Story01,
        ArtPaths.Story02,
        ArtPaths.Story03,
        ArtPaths.Story04,
    };

    // ── live refs ───────────────────────────────────────────────────────
    private Canvas canvas;
    private CanvasGroup splashGroup;
    private CanvasGroup reelGroup;

    // reel
    private Image reelImage;
    private Text reelLore;
    private Button[] dotButtons;
    private int currentSlide;

    private ChapterSaveData SaveData => ChapterSaveManager.Instance?.Data;

    // ────────────────────────────────────────────────────────────────────
    private void Start()
    {
        EnsureEventSystem();
        BuildCanvas();

        bool skipReel = SaveData?.hasCompletedOnboarding ?? false;
        if (skipReel)
            ShowSplashReturnState();
        else
            ShowSplashFirstTime();
    }

    // ── public entry points ─────────────────────────────────────────────

    private void ShowSplashFirstTime()
    {
        splashGroup.alpha = 1f;
        splashGroup.interactable = true;
        splashGroup.blocksRaycasts = true;
        reelGroup.alpha = 0f;
        reelGroup.interactable = false;
        reelGroup.blocksRaycasts = false;
    }

    private void ShowSplashReturnState()
    {
        // Already-onboarded player: show Continue / New Game directly
        ShowSplashFirstTime();
    }

    // ── button handlers ─────────────────────────────────────────────────

    private void HandleEnterKeyhouse()
    {
        // S0 → S1
        StartCoroutine(CrossFade(splashGroup, reelGroup, 0.35f));
        ShowSlide(0);
    }

    private void HandleContinue()
    {
        if (ChapterSaveManager.HasContinuableSaveOnDisk())
        {
            GameBootContext.OpenMapOnStart = true;
            SceneManager.LoadScene(SceneNames.Chapter1);
        }
    }

    private void HandleNewGame()
    {
        ChapterSaveManager.ResetSaveOnDisk();
        GameBootContext.OpenMapOnStart = true;
        SceneManager.LoadScene(SceneNames.Chapter1);
    }

    private void HandleNextSlide()
    {
        if (currentSlide < StoryPaths.Length - 1)
            ShowSlide(currentSlide + 1);
        else
            HandleOnboardingComplete();
    }

    private void HandleSkip() => HandleOnboardingComplete();

    private void HandleOnboardingComplete()
    {
        ChapterSaveManager.Instance?.RecordOnboardingComplete();
        GameBootContext.OpenMapOnStart = true;
        SceneManager.LoadScene(SceneNames.Chapter1);
    }

    private void ShowSlide(int index)
    {
        currentSlide = index;

        var sprite = Resources.Load<Sprite>(StoryPaths[index]);
        if (reelImage != null)
            reelImage.sprite = sprite;

        if (reelLore != null)
            reelLore.text = StoryLore[index];

        // Update dots
        for (int i = 0; i < dotButtons.Length; i++)
        {
            var img = dotButtons[i].GetComponent<Image>();
            if (img != null)
                img.color = i == index
                    ? new Color(0.88f, 0.76f, 0.40f, 1f)   // gold active
                    : new Color(0.4f, 0.4f, 0.4f, 0.6f);   // grey inactive
        }
    }

    // ── canvas builder ──────────────────────────────────────────────────

    private void BuildCanvas()
    {
        var go = new GameObject("TitleCanvas",
            typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;

        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(REF_W, REF_H);
        scaler.matchWidthOrHeight = 1f;  // portrait: match height

        splashGroup = BuildSplash(go.transform);
        reelGroup   = BuildReel(go.transform);
    }

    // ── S0 Splash ────────────────────────────────────────────────────────

    private CanvasGroup BuildSplash(Transform root)
    {
        var panel = MakePanel(root, "Splash", Color.black);
        var cg = panel.AddComponent<CanvasGroup>();

        // Background
        var bgImg = panel.AddComponent<Image>();
        var bgSprite = Resources.Load<Sprite>(ArtPaths.BgFoyerPortrait);
        if (bgSprite != null) { bgImg.sprite = bgSprite; bgImg.type = Image.Type.Simple; }
        else bgImg.color = new Color(0.03f, 0.04f, 0.08f);

        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Dark scrim
        MakeImage(panel.transform, "Scrim", new Color(0f, 0f, 0f, 0.55f), Vector2.zero, Vector2.one);

        // Title
        AddLabel(panel.transform, "GameTitle", font, 52,
            new Color(0.92f, 0.80f, 0.38f), new Vector2(0.5f, 0.72f),
            "LOCKE & KEY", REF_W - 40f, 68f, FontStyle.Bold);

        // Subtitle
        AddLabel(panel.transform, "ChapterLabel", font, 22,
            new Color(0.72f, 0.72f, 0.78f), new Vector2(0.5f, 0.63f),
            "Chapter 1 — Keyhouse", REF_W - 60f, 36f, FontStyle.Italic);

        bool hasOnboarding = SaveData?.hasCompletedOnboarding ?? false;

        if (!hasOnboarding)
        {
            // First launch: single "Enter Keyhouse" button
            AddButton(panel.transform, "Enter Keyhouse", font,
                new Vector2(0.5f, 0.28f), HandleEnterKeyhouse,
                new Color(0.88f, 0.74f, 0.28f), new Color(0.05f, 0.04f, 0.02f));
        }
        else
        {
            // Returning player: Continue + New Game
            bool canContinue = ChapterSaveManager.HasContinuableSaveOnDisk();
            var continueBtn = AddButton(panel.transform, "Continue", font,
                new Vector2(0.5f, 0.33f), HandleContinue,
                new Color(0.88f, 0.74f, 0.28f), new Color(0.05f, 0.04f, 0.02f));
            continueBtn.interactable = canContinue;

            AddButton(panel.transform, "New Game", font,
                new Vector2(0.5f, 0.22f), HandleNewGame,
                new Color(0.28f, 0.32f, 0.40f), Color.white);

            var summary = ChapterSaveManager.ReadSaveSummaryFromDisk();
            if (!string.IsNullOrEmpty(summary))
                AddLabel(panel.transform, "Progress", font, 16,
                    new Color(0.55f, 0.60f, 0.68f), new Vector2(0.5f, 0.13f),
                    summary, REF_W - 60f, 28f, FontStyle.Normal);
        }

        return cg;
    }

    // ── S1 Story Reel ────────────────────────────────────────────────────

    private CanvasGroup BuildReel(Transform root)
    {
        var panel = MakePanel(root, "StoryReel", Color.black);
        var cg = panel.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;

        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Slide image (top 62% of screen)
        var imageGo = new GameObject("SlideImage",
            typeof(RectTransform), typeof(Image));
        imageGo.transform.SetParent(panel.transform, false);
        var slideRect = imageGo.GetComponent<RectTransform>();
        slideRect.anchorMin = new Vector2(0, 0.36f);
        slideRect.anchorMax = Vector2.one;
        slideRect.offsetMin = slideRect.offsetMax = Vector2.zero;
        reelImage = imageGo.GetComponent<Image>();
        reelImage.preserveAspect = false;

        // Bottom lore area
        var loreGo = new GameObject("Lore",
            typeof(RectTransform), typeof(Text));
        loreGo.transform.SetParent(panel.transform, false);
        var loreRect = loreGo.GetComponent<RectTransform>();
        loreRect.anchorMin = new Vector2(0.06f, 0.21f);
        loreRect.anchorMax = new Vector2(0.94f, 0.36f);
        loreRect.offsetMin = loreRect.offsetMax = Vector2.zero;
        reelLore = loreGo.GetComponent<Text>();
        reelLore.font = font;
        reelLore.fontSize = 20;
        reelLore.color = new Color(0.85f, 0.83f, 0.78f);
        reelLore.alignment = TextAnchor.UpperCenter;
        reelLore.lineSpacing = 1.3f;

        // Page dots (4)
        dotButtons = new Button[StoryPaths.Length];
        for (int i = 0; i < StoryPaths.Length; i++)
        {
            int idx = i;
            var dotGo = new GameObject($"Dot_{i}",
                typeof(RectTransform), typeof(Image), typeof(Button));
            dotGo.transform.SetParent(panel.transform, false);
            var dRect = dotGo.GetComponent<RectTransform>();
            float dotX = 0.5f + (i - (StoryPaths.Length - 1) * 0.5f) * 0.065f;
            dRect.anchorMin = dRect.anchorMax = new Vector2(dotX, 0.175f);
            dRect.sizeDelta = new Vector2(10f, 10f);
            var dImg = dotGo.GetComponent<Image>();
            dImg.color = new Color(0.4f, 0.4f, 0.4f, 0.6f);
            var dBtn = dotGo.GetComponent<Button>();
            dBtn.onClick.AddListener(() => ShowSlide(idx));
            dotButtons[i] = dBtn;
        }

        // Skip (top-right)
        var skipGo = new GameObject("SkipBtn",
            typeof(RectTransform), typeof(Image), typeof(Button), typeof(Text));
        skipGo.transform.SetParent(panel.transform, false);
        var skipRect = skipGo.GetComponent<RectTransform>();
        skipRect.anchorMin = new Vector2(0.72f, 0.91f);
        skipRect.anchorMax = new Vector2(0.98f, 0.98f);
        skipRect.offsetMin = skipRect.offsetMax = Vector2.zero;
        var skipImg = skipGo.GetComponent<Image>();
        skipImg.color = new Color(1f, 1f, 1f, 0.08f);
        var skipText = skipGo.GetComponent<Text>();
        skipText.font = font;
        skipText.text = "Skip";
        skipText.fontSize = 18;
        skipText.color = new Color(0.7f, 0.7f, 0.7f);
        skipText.alignment = TextAnchor.MiddleCenter;
        skipGo.GetComponent<Button>().onClick.AddListener(HandleSkip);

        // Next button (bottom)
        AddButton(panel.transform, "Next →", font,
            new Vector2(0.5f, 0.082f), HandleNextSlide,
            new Color(0.88f, 0.74f, 0.28f), new Color(0.05f, 0.04f, 0.02f),
            width: 200f, height: 52f);

        return cg;
    }

    // ── Coroutine helpers ────────────────────────────────────────────────

    private IEnumerator CrossFade(CanvasGroup from, CanvasGroup to, float duration)
    {
        float t = 0f;
        to.alpha = 0f;
        to.interactable = false;
        to.blocksRaycasts = false;

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);
            from.alpha = 1f - p;
            to.alpha = p;
            yield return null;
        }

        from.alpha = 0f;
        from.interactable = false;
        from.blocksRaycasts = false;

        to.alpha = 1f;
        to.interactable = true;
        to.blocksRaycasts = true;
    }

    // ── Low-level UI builders ────────────────────────────────────────────

    private static GameObject MakePanel(Transform parent, string name, Color bg)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        go.GetComponent<Image>().color = bg;
        return go;
    }

    private static Image MakeImage(Transform parent, string name, Color color,
        Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        var img = go.GetComponent<Image>();
        img.color = color;
        return img;
    }

    private static void AddLabel(Transform parent, string name, Font font,
        int size, Color color, Vector2 anchorCenter, string text,
        float width, float height, FontStyle style)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = anchorCenter;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(width, height);
        var t = go.GetComponent<Text>();
        t.font = font;
        t.fontSize = size;
        t.color = color;
        t.text = text;
        t.alignment = TextAnchor.MiddleCenter;
        t.fontStyle = style;
    }

    private static Button AddButton(Transform parent, string label, Font font,
        Vector2 anchorCenter, UnityEngine.Events.UnityAction onClick,
        Color bgColor, Color textColor, float width = 260f, float height = 58f)
    {
        var go = new GameObject(label.Replace(" ", "") + "Btn",
            typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = anchorCenter;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(width, height);
        go.GetComponent<Image>().color = bgColor;
        var btn = go.GetComponent<Button>();
        btn.onClick.AddListener(onClick);

        var textGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
        textGo.transform.SetParent(go.transform, false);
        var tRect = textGo.GetComponent<RectTransform>();
        tRect.anchorMin = Vector2.zero; tRect.anchorMax = Vector2.one;
        tRect.offsetMin = tRect.offsetMax = Vector2.zero;
        var t = textGo.GetComponent<Text>();
        t.font = font;
        t.fontSize = 22;
        t.fontStyle = FontStyle.Bold;
        t.color = textColor;
        t.text = label;
        t.alignment = TextAnchor.MiddleCenter;

        return btn;
    }

    private static void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            var esGo = new GameObject("EventSystem",
                typeof(EventSystem), typeof(StandaloneInputModule));
        }
    }
}