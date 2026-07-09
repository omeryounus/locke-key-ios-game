using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// S0 Splash + S1 Story Reel — targets ux_s0_splash / ux_s1_story_reel (programmatic uGUI).
/// </summary>
[DefaultExecutionOrder(-100)]
public class TitleScreenController : MonoBehaviour
{
    private static TitleScreenController activeInstance;

    private static readonly string[] StoryLore =
    {
        "You return to Keyhouse after years away.\nSomething is wrong with the doors.",
        "The first key found beneath the floorboards.\nA rusted thing with too much weight.",
        "The wellhouse hums at night.\nAn echo of something that should not be.",
        "The black door at the end of the hall\nhas no keyhole. It never did."
    };

    private static readonly string[] StoryPaths =
    {
        ArtPaths.Story01, ArtPaths.Story02, ArtPaths.Story03, ArtPaths.Story04,
    };

    private Font font;
    private CanvasGroup splashGroup;
    private CanvasGroup reelGroup;
    private Image reelImage;
    private Text reelLore;
    private Button[] dotButtons;
    private Button nextSlideBtn;
    private int currentSlide;

    private static bool HasCompletedOnboarding =>
        ChapterSaveManager.HasCompletedOnboardingOnDisk();

    private void Awake()
    {
        if (activeInstance != null && activeInstance != this)
        {
            Debug.LogWarning("[TitleScreen] Duplicate TitleScreenController disabled.");
            enabled = false;
            return;
        }

        activeInstance = this;
        StartCoroutine(InitializeRoutine());
    }

    private void OnDestroy()
    {
        if (activeInstance == this)
            activeInstance = null;
    }

    private IEnumerator InitializeRoutine()
    {
        if (!enabled) yield break;

        for (int i = 0; i < 60 && (Screen.width <= 0 || Screen.height <= 0); i++)
            yield return null;

        EnsureEventSystem();
        BuildCanvas();

        if (splashGroup == null || reelGroup == null)
        {
            Debug.LogError($"[TitleScreen] Primary UI build failed; using emergency canvas. screen={Screen.width}x{Screen.height}");
            BuildEmergencyCanvas();
        }

        if (splashGroup == null)
            yield break;

        Debug.Log($"[TitleScreen] Ready. screen={Screen.width}x{Screen.height}, font={(font != null)}");

        if (GameBootContext.OpenStoryReelOnStart)
        {
            GameBootContext.OpenStoryReelOnStart = false;
            StartCoroutine(CrossFade(splashGroup, reelGroup, 0.35f));
            ShowSlide(0);
        }
        else if (HasCompletedOnboarding)
            ShowSplashReturnState();
        else
            ShowSplashFirstTime();
    }

    private void ShowSplashFirstTime()
    {
        splashGroup.alpha = 1f;
        splashGroup.interactable = splashGroup.blocksRaycasts = true;
        reelGroup.alpha = 0f;
        reelGroup.interactable = reelGroup.blocksRaycasts = false;
    }

    private void ShowSplashReturnState()
    {
        splashGroup.alpha = 1f;
        splashGroup.interactable = splashGroup.blocksRaycasts = true;
        reelGroup.alpha = 0f;
        reelGroup.interactable = reelGroup.blocksRaycasts = false;
    }

    private void HandleEnterKeyhouse()
    {
        StartCoroutine(CrossFade(splashGroup, reelGroup, 0.35f));
        ShowSlide(0);
    }

    private void HandleContinue()
    {
        if (!ChapterSaveManager.HasContinuableSaveOnDisk()) return;
        GameBootContext.OpenMapOnStart = true;
        SceneManager.LoadScene(SceneNames.Chapter1);
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
        ChapterSaveManager.RecordOnboardingCompleteOnDisk();
        GameBootContext.OpenMapOnStart = true;
        SceneManager.LoadScene(SceneNames.Chapter1);
    }

    private void ShowSlide(int index)
    {
        currentSlide = index;
        var sprite = Resources.Load<Sprite>(StoryPaths[index]);
        if (reelImage != null) reelImage.sprite = sprite;
        if (reelLore != null) reelLore.text = StoryLore[index];

        if (nextSlideBtn != null)
        {
            bool last = index >= StoryPaths.Length - 1;
            var label = nextSlideBtn.transform.Find("Label")?.GetComponent<Text>();
            if (label != null) label.text = last ? "Enter Keyhouse" : "Next";
        }

        for (int i = 0; i < dotButtons.Length; i++)
        {
            var img = dotButtons[i].GetComponent<Image>();
            if (img != null)
                img.color = i == index ? LockeKeyUITheme.LKGold : LockeKeyUITheme.CaptionText;
        }
    }

    private void BuildCanvas()
    {
        var existing = GameObject.Find("TitleCanvas");
        if (existing != null)
        {
            Debug.LogWarning("[TitleScreen] TitleCanvas already exists; skipping rebuild.");
            return;
        }

        var flow = LockeUILayout.CreateFlowCanvas("TitleCanvas", 200);
        font = flow.Font ?? LockeUILayout.GetUIFont();
        var parent = LockeUILayout.GetContentRoot(flow);
        splashGroup = BuildSplash(parent);
        reelGroup = BuildReel(parent);
        Canvas.ForceUpdateCanvases();
    }

    private CanvasGroup BuildSplash(Transform root)
    {
        var panel = MakePanel(root, "Splash", LockeKeyUITheme.LKInk);
        var cg = panel.AddComponent<CanvasGroup>();

        var bgImg = panel.GetComponent<Image>();
        var bgSprite = Resources.Load<Sprite>(ArtPaths.BgFoyerPortrait);
        if (bgSprite != null) { bgImg.sprite = bgSprite; bgImg.color = Color.white; }
        else bgImg.color = LockeKeyUITheme.LKInk;

        LockeUIComponents.CreateScrim(panel.transform);
        var scrim = panel.transform.Find("Scrim")?.GetComponent<Image>();
        if (scrim != null) scrim.color = new Color(0f, 0f, 0f, 0.45f);

        LockeUIComponents.AddText(panel.transform, "GameTitle", font, LockeKeyUITheme.DisplaySize + 2,
            FontStyle.Bold, LockeKeyUITheme.LKGold, new Vector2(0.5f, 0.72f),
            "LOCKE & KEY", new Vector2(LockeKeyUITheme.RefWidth - 40f, 68f), TextAnchor.MiddleCenter);

        LockeUIComponents.AddText(panel.transform, "ChapterLabel", font, LockeKeyUITheme.TitleSize,
            FontStyle.Italic, LockeKeyUITheme.BodyText, new Vector2(0.5f, 0.63f),
            "Chapter 1 — Keyhouse", new Vector2(LockeKeyUITheme.RefWidth - 60f, 36f), TextAnchor.MiddleCenter);

        bool hasOnboarding = HasCompletedOnboarding;
        if (!hasOnboarding)
        {
            LockeUIComponents.CreatePrimaryButton(panel.transform, font, "Enter Keyhouse",
                new Vector2(0.5f, 0.28f), HandleEnterKeyhouse);
        }
        else
        {
            bool canContinue = ChapterSaveManager.HasContinuableSaveOnDisk();
            var continueBtn = LockeUIComponents.CreatePrimaryButton(panel.transform, font, "Continue",
                new Vector2(0.5f, 0.33f), HandleContinue);
            continueBtn.interactable = canContinue;
            LockeUIComponents.CreateSecondaryButton(panel.transform, font, "New Game",
                new Vector2(0.5f, 0.22f), HandleNewGame, 220f);
            var summary = ChapterSaveManager.ReadSaveSummaryFromDisk();
            if (!string.IsNullOrEmpty(summary))
                LockeUIComponents.AddText(panel.transform, "Progress", font, LockeKeyUITheme.CaptionSize + 4,
                    FontStyle.Normal, LockeKeyUITheme.CaptionText, new Vector2(0.5f, 0.13f),
                    summary, new Vector2(LockeKeyUITheme.RefWidth - 60f, 28f), TextAnchor.MiddleCenter);
        }

        return cg;
    }

    private CanvasGroup BuildReel(Transform root)
    {
        var panel = MakePanel(root, "StoryReel", LockeKeyUITheme.LKInk);
        var cg = panel.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.interactable = cg.blocksRaycasts = false;

        var imageGo = new GameObject("SlideImage", typeof(RectTransform), typeof(Image));
        imageGo.transform.SetParent(panel.transform, false);
        var slideRect = imageGo.GetComponent<RectTransform>();
        slideRect.anchorMin = new Vector2(0.05f, 0.36f);
        slideRect.anchorMax = new Vector2(0.95f, 0.92f);
        slideRect.offsetMin = slideRect.offsetMax = Vector2.zero;
        LockeUIComponents.CreateWoodFrame(panel.transform, slideRect);
        reelImage = imageGo.GetComponent<Image>();
        reelImage.preserveAspect = true;

        reelLore = LockeUIComponents.AddText(panel.transform, "Lore", font, LockeKeyUITheme.BodySize + 2,
            FontStyle.Normal, LockeKeyUITheme.BodyText, new Vector2(0.5f, 0.28f),
            "", new Vector2(LockeKeyUITheme.RefWidth - 48f, 80f), TextAnchor.UpperCenter);

        dotButtons = new Button[StoryPaths.Length];
        for (int i = 0; i < StoryPaths.Length; i++)
        {
            int idx = i;
            var dotGo = new GameObject($"Dot_{i}", typeof(RectTransform), typeof(Image), typeof(Button));
            dotGo.transform.SetParent(panel.transform, false);
            var dRect = dotGo.GetComponent<RectTransform>();
            float dotX = 0.5f + (i - (StoryPaths.Length - 1) * 0.5f) * 0.065f;
            dRect.anchorMin = dRect.anchorMax = new Vector2(dotX, 0.175f);
            dRect.sizeDelta = new Vector2(10f, 10f);
            dotGo.GetComponent<Image>().color = LockeKeyUITheme.CaptionText;
            dotGo.GetComponent<Button>().onClick.AddListener(() => ShowSlide(idx));
            dotButtons[i] = dotGo.GetComponent<Button>();
        }

        LockeUIComponents.CreateSecondaryButton(panel.transform, font, "Skip",
            new Vector2(0.86f, 0.94f), HandleSkip, 100f);

        nextSlideBtn = LockeUIComponents.CreatePrimaryButton(panel.transform, font, "Next",
            new Vector2(0.5f, 0.082f), HandleNextSlide, 200f);

        return cg;
    }

    private IEnumerator CrossFade(CanvasGroup from, CanvasGroup to, float duration)
    {
        float t = 0f;
        to.alpha = 0f;
        to.interactable = to.blocksRaycasts = false;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);
            from.alpha = 1f - p;
            to.alpha = p;
            yield return null;
        }

        from.alpha = 0f;
        from.interactable = from.blocksRaycasts = false;
        to.alpha = 1f;
        to.interactable = to.blocksRaycasts = true;
    }

    private static GameObject MakePanel(Transform parent, string name, Color bg)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        LockeUILayout.Stretch(go.GetComponent<RectTransform>());
        go.GetComponent<Image>().color = bg;
        return go;
    }

    private void BuildEmergencyCanvas()
    {
        if (GameObject.Find("TitleCanvas") != null)
            GameObject.Destroy(GameObject.Find("TitleCanvas"));

        var go = new GameObject("TitleCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;

        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(LockeKeyUITheme.RefWidth, LockeKeyUITheme.RefHeight);
        scaler.matchWidthOrHeight = 0.5f;
        LockeUILayout.Stretch(go.GetComponent<RectTransform>());

        font = LockeUILayout.GetUIFont();
        var panel = MakePanel(go.transform, "Splash", LockeKeyUITheme.LKInk);
        splashGroup = panel.AddComponent<CanvasGroup>();

        LockeUIComponents.AddText(panel.transform, "GameTitle", font, LockeKeyUITheme.DisplaySize + 2,
            FontStyle.Bold, LockeKeyUITheme.LKGold, new Vector2(0.5f, 0.65f),
            "LOCKE & KEY", new Vector2(LockeKeyUITheme.RefWidth - 40f, 68f), TextAnchor.MiddleCenter);

        LockeUIComponents.CreatePrimaryButton(panel.transform, font, "Enter Keyhouse",
            new Vector2(0.5f, 0.35f), HandleEnterKeyhouse);

        reelGroup = splashGroup;
        Canvas.ForceUpdateCanvases();
    }

    private static void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }
}