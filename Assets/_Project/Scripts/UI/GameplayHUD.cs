using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Runtime-built uGUI canvas for iOS touch controls and status readouts.
/// </summary>
public class GameplayHUD : MonoBehaviour
{
    [SerializeField] private TouchGameplayController gameplay;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private UIIconLibrary iconLibrary;
    [SerializeField] private GameObject hudPrefab;
    [SerializeField] private bool preferAuthoredPrefab = true;

    private Text keyStatusText;
    private Text houseKeyText;
    private Text hintText;
    private Text toastText;
    private Image keyStatusIcon;
    private Image keySlotImage;
    private KeySlotHUD keySlotHud;
    private Image memoryPanelImage;
    private Image houseKeyIcon;
    private GameObject memoryOverlay;
    private Text memoryBodyText;
    private float toastTimer;
    private GameObject leftButton;
    private GameObject rightButton;
    private GameObject jumpButton;
    private GameObject interactButton;
    private GameObject useKeyButton;

    private void Awake()
    {
        if (gameplay == null)
            gameplay = FindFirstObjectByType<TouchGameplayController>();

        if (uiManager == null)
            uiManager = FindFirstObjectByType<UIManager>();

        if (iconLibrary == null)
            iconLibrary = UIIconLibrary.LoadDefault();

        if (uiManager != null)
            uiManager.BindHUD(this);

        if (!TryLoadAuthoredHud())
            BuildCanvas();
    }

    private void Update()
    {
        RefreshStatus();
        TickToast();
    }

    public void ShowToast(string message, float duration = 3f)
    {
        if (toastText == null) return;
        toastText.text = message;
        toastText.gameObject.SetActive(true);
        toastTimer = duration;
    }

    public void ShowMemoryOverlay(string title, string body, int panelIndex = 1)
    {
        if (memoryOverlay == null) return;
        memoryOverlay.SetActive(true);

        var titleText = memoryOverlay.transform.Find("Title")?.GetComponent<Text>();
        if (titleText != null)
            titleText.text = title;

        if (memoryBodyText != null)
            memoryBodyText.text = body;

        if (memoryPanelImage != null)
        {
            var panels = MemoryPanelLibrary.LoadDefault();
            memoryPanelImage.sprite = panels != null ? panels.GetPanel(panelIndex) : null;
            memoryPanelImage.enabled = memoryPanelImage.sprite != null;
        }

        FindFirstObjectByType<ParticleVFXController>()?.PlayMemoryBurst(Vector3.zero);
    }

    public void FlashKeyDiscovered()
    {
        keySlotHud?.FlashDiscovered();
    }

    public void HideMemoryOverlay()
    {
        if (memoryOverlay != null)
            memoryOverlay.SetActive(false);
    }

    public void SetControlVisibility(bool? move = null, bool? jump = null, bool? interact = null, bool? useKey = null)
    {
        if (move.HasValue)
        {
            if (leftButton != null) leftButton.SetActive(move.Value);
            if (rightButton != null) rightButton.SetActive(move.Value);
        }

        if (jump.HasValue && jumpButton != null)
            jumpButton.SetActive(jump.Value);
        if (interact.HasValue && interactButton != null)
            interactButton.SetActive(interact.Value);
        if (useKey.HasValue && useKeyButton != null)
            useKeyButton.SetActive(useKey.Value);
    }

    private void TickToast()
    {
        if (toastTimer <= 0f || toastText == null) return;

        toastTimer -= Time.deltaTime;
        if (toastTimer <= 0f)
            toastText.gameObject.SetActive(false);
    }

    private void RefreshStatus()
    {
        if (gameplay == null) return;

        if (keyStatusText != null)
            keyStatusText.text = gameplay.GetKeyStatusLabel();

        if (houseKeyText != null)
            houseKeyText.text = gameplay.GetHouseKeyLabel();

        if (hintText != null)
            hintText.text = gameplay.GetHintLabel();

        if (keyStatusIcon != null && iconLibrary != null)
            keyStatusIcon.sprite = ResolveActiveKeyIcon();

        keySlotHud?.Refresh();

        if (houseKeyIcon != null && iconLibrary != null)
        {
            houseKeyIcon.gameObject.SetActive(gameplay.HasHouseKey);
            houseKeyIcon.sprite = iconLibrary.houseKeyIcon;
        }
    }

    private Sprite ResolveActiveKeyIcon()
    {
        if (iconLibrary == null || gameplay == null) return null;

        var label = gameplay.GetKeyStatusLabel();
        if (label.Contains("Ghost")) return iconLibrary.ghostKeyIcon;
        if (label.Contains("Head")) return iconLibrary.headKeyIcon;
        return null;
    }

    private bool TryLoadAuthoredHud()
    {
        if (!preferAuthoredPrefab) return false;

        var prefab = hudPrefab != null
            ? hudPrefab
            : Resources.Load<GameObject>("UI/GameplayHUD");

        if (prefab == null) return false;

        EnsureEventSystem();

        var instance = Instantiate(prefab);
        instance.name = "GameplayCanvas";
        var bindings = instance.GetComponent<GameplayHUDBindings>();
        if (bindings == null) return false;

        keyStatusText = bindings.keyStatusText;
        houseKeyText = bindings.houseKeyText;
        hintText = bindings.hintText;
        toastText = bindings.toastText;
        keyStatusIcon = bindings.keyStatusIcon;
        keySlotImage = bindings.keySlotImage;
        keySlotHud = bindings.keySlotHud;
        memoryPanelImage = bindings.memoryPanelImage;
        houseKeyIcon = bindings.houseKeyIcon;
        memoryOverlay = bindings.memoryOverlay;
        memoryBodyText = bindings.memoryBodyText;
        leftButton = bindings.leftButton;
        rightButton = bindings.rightButton;
        jumpButton = bindings.jumpButton;
        interactButton = bindings.interactButton;
        useKeyButton = bindings.useKeyButton;

        WirePrefabButtons();
        SetControlVisibility(interact: false, jump: false, useKey: false);
        return true;
    }

    private void WirePrefabButtons()
    {
        if (leftButton != null)
            WireHoldButton(leftButton, () => gameplay?.SetMoveInput(-1f), () => gameplay?.SetMoveInput(0f));
        if (rightButton != null)
            WireHoldButton(rightButton, () => gameplay?.SetMoveInput(1f), () => gameplay?.SetMoveInput(0f));
        if (jumpButton != null)
            WireTapButton(jumpButton, () => gameplay?.RequestJump());
        if (interactButton != null)
            WireTapButton(interactButton, () => gameplay?.RequestInteract());
        if (useKeyButton != null)
            WireTapButton(useKeyButton, () => gameplay?.RequestUseKey());

        if (memoryOverlay != null)
        {
            var close = memoryOverlay.transform.Find("MemoryPanel/CloseButton");
            if (close != null)
                WireTapButton(close.gameObject, () =>
                {
                    HideMemoryOverlay();
                    uiManager?.CloseMemoryView();
                });
        }
    }

    private static void WireTapButton(GameObject buttonGo, UnityEngine.Events.UnityAction onTap)
    {
        var button = buttonGo.GetComponent<Button>();
        if (button == null) return;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(onTap);
    }

    private static void WireHoldButton(GameObject buttonGo, UnityEngine.Events.UnityAction onDown,
        UnityEngine.Events.UnityAction onUp)
    {
        var hold = buttonGo.GetComponent<HoldButton>();
        if (hold == null)
            hold = buttonGo.AddComponent<HoldButton>();
        hold.onDown.RemoveAllListeners();
        hold.onUp.RemoveAllListeners();
        hold.onDown.AddListener(onDown);
        hold.onUp.AddListener(onUp);
    }

    private void BuildCanvas()
    {
        if (FindFirstObjectByType<Canvas>() != null)
            return;

        EnsureEventSystem();

        var canvasGo = new GameObject("GameplayCanvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasGo.AddComponent<CanvasScaler>();
        canvasGo.AddComponent<GraphicRaycaster>();

        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var panelColor = new Color(0.05f, 0.06f, 0.1f, 0.72f);
        var buttonColor = new Color(0.14f, 0.16f, 0.24f, 0.92f);
        var accentColor = new Color(0.55f, 0.75f, 0.95f, 1f);

        keyStatusIcon = CreateStatusIcon(canvasGo.transform, "KeyStatusIcon",
            new Vector2(24f, -20f), 40f);

        var keySlotGo = new GameObject("KeySlot", typeof(RectTransform), typeof(Image), typeof(KeySlotHUD));
        keySlotGo.transform.SetParent(canvasGo.transform, false);
        var keySlotRect = keySlotGo.GetComponent<RectTransform>();
        keySlotRect.anchorMin = new Vector2(0f, 1f);
        keySlotRect.anchorMax = new Vector2(0f, 1f);
        keySlotRect.pivot = new Vector2(0f, 1f);
        keySlotRect.anchoredPosition = new Vector2(180f, -12f);
        keySlotRect.sizeDelta = new Vector2(72f, 72f);
        keySlotImage = keySlotGo.GetComponent<Image>();
        keySlotImage.preserveAspect = true;
        keySlotHud = keySlotGo.GetComponent<KeySlotHUD>();

        keyStatusText = CreateText(canvasGo.transform, "KeyStatus", font, 24, TextAnchor.UpperLeft,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(260f, -24f), new Vector2(860f, 36f), accentColor);

        houseKeyIcon = CreateStatusIcon(canvasGo.transform, "HouseKeyIcon",
            new Vector2(24f, -60f), 32f);
        houseKeyText = CreateText(canvasGo.transform, "HouseKeyStatus", font, 20, TextAnchor.UpperLeft,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(64f, -64f), new Vector2(860f, 32f), Color.white);

        hintText = CreateText(canvasGo.transform, "Hint", font, 22, TextAnchor.UpperLeft,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -104f), new Vector2(1200f, 72f),
            new Color(0.85f, 0.82f, 0.75f, 1f));

        toastText = CreateText(canvasGo.transform, "Toast", font, 24, TextAnchor.UpperCenter,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -160f), new Vector2(1100f, 48f),
            new Color(1f, 0.85f, 0.55f, 1f));
        toastText.gameObject.SetActive(false);

        var controlBar = CreatePanel(canvasGo.transform, "ControlBar", panelColor,
            new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, 140f));

        leftButton = CreateHoldButton(controlBar.transform, "Left", iconLibrary?.moveLeft, font, buttonColor, accentColor,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(24f, 20f), new Vector2(180f, 100f),
            () => gameplay?.SetMoveInput(-1f), () => gameplay?.SetMoveInput(0f));

        rightButton = CreateHoldButton(controlBar.transform, "Right", iconLibrary?.moveRight, font, buttonColor, accentColor,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(220f, 20f), new Vector2(180f, 100f),
            () => gameplay?.SetMoveInput(1f), () => gameplay?.SetMoveInput(0f));

        jumpButton = CreateTapButton(controlBar.transform, "Jump", iconLibrary?.jump, font, buttonColor, accentColor,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(416f, 20f), new Vector2(180f, 100f),
            () => gameplay?.RequestJump());

        interactButton = CreateTapButton(controlBar.transform, "Interact", iconLibrary?.interact, font, buttonColor, accentColor,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-416f, 20f), new Vector2(180f, 100f),
            () => gameplay?.RequestInteract());

        useKeyButton = CreateTapButton(controlBar.transform, "Use Key", iconLibrary?.useKey, font, buttonColor, accentColor,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-216f, 20f), new Vector2(180f, 100f),
            () => gameplay?.RequestUseKey());

        SetControlVisibility(interact: false, jump: false, useKey: false);

        memoryOverlay = BuildMemoryOverlay(canvasGo.transform, font, panelColor, accentColor);
        memoryOverlay.SetActive(false);
    }

    private static Image CreateStatusIcon(Transform parent, string name, Vector2 anchoredPos, float size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.preserveAspect = true;
        image.color = Color.white;
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = new Vector2(size, size);
        return image;
    }

    private GameObject BuildMemoryOverlay(Transform parent, Font font, Color panelColor, Color accentColor)
    {
        var overlay = CreatePanel(parent, "MemoryOverlay", new Color(0.02f, 0.02f, 0.05f, 0.82f),
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        overlay.SetActive(false);

        var panel = CreatePanel(overlay.transform, "MemoryPanel", panelColor,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-520f, -280f), new Vector2(1040f, 560f));

        var panelImageGo = new GameObject("MemoryPanelArt", typeof(RectTransform), typeof(Image));
        panelImageGo.transform.SetParent(panel.transform, false);
        memoryPanelImage = panelImageGo.GetComponent<Image>();
        memoryPanelImage.preserveAspect = true;
        memoryPanelImage.color = Color.white;
        var panelImageRect = panelImageGo.GetComponent<RectTransform>();
        panelImageRect.anchorMin = Vector2.zero;
        panelImageRect.anchorMax = Vector2.one;
        panelImageRect.offsetMin = Vector2.zero;
        panelImageRect.offsetMax = Vector2.zero;

        CreateText(panel.transform, "Title", font, 34, TextAnchor.UpperCenter,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -36f), new Vector2(960f, 48f), accentColor);

        memoryBodyText = CreateText(panel.transform, "Body", font, 24, TextAnchor.UpperLeft,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(36f, -110f), new Vector2(960f, 340f),
            new Color(0.92f, 0.9f, 0.86f, 1f));

        CreateTapButton(panel.transform, "Close", null, font, new Color(0.2f, 0.22f, 0.32f, 1f), accentColor,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-120f, 28f), new Vector2(240f, 64f),
            () =>
            {
                HideMemoryOverlay();
                uiManager?.CloseMemoryView();
            });

        return overlay;
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
        var image = go.GetComponent<Image>();
        image.color = color;
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(anchorMin.x < 0.5f ? 0f : 1f, 0f);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = sizeDelta;
        return go;
    }

    private static Text CreateText(Transform parent, string name, Font font, int fontSize, TextAnchor alignment,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var text = go.GetComponent<Text>();
        text.font = font;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = anchorMin;
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = sizeDelta;
        return text;
    }

    private static GameObject CreateTapButton(Transform parent, string label, Sprite icon, Font font, Color bg, Color textColor,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta, UnityEngine.Events.UnityAction onTap)
    {
        var go = new GameObject(label + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.color = bg;
        if (icon != null)
        {
            image.sprite = icon;
            image.color = Color.white;
        }

        var button = go.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onTap);

        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(anchorMin.x < 0.5f ? 0f : 1f, 0f);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = sizeDelta;

        if (icon == null)
        {
            CreateText(go.transform, "Label", font, 22, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, textColor).text = label;
        }

        return go;
    }

    private static GameObject CreateHoldButton(Transform parent, string label, Sprite icon, Font font, Color bg, Color textColor,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta,
        UnityEngine.Events.UnityAction onDown, UnityEngine.Events.UnityAction onUp)
    {
        var go = new GameObject(label + "Button", typeof(RectTransform), typeof(Image), typeof(HoldButton));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.color = icon != null ? Color.white : bg;
        if (icon != null)
            image.sprite = icon;

        var hold = go.GetComponent<HoldButton>();
        hold.onDown.AddListener(onDown);
        hold.onUp.AddListener(onUp);

        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(anchorMin.x < 0.5f ? 0f : 1f, 0f);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = sizeDelta;

        if (icon == null)
        {
            CreateText(go.transform, "Label", font, 22, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, textColor).text = label;
        }

        return go;
    }
}