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
    [SerializeField] private bool preferAuthoredPrefab;

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
    private Text roomTitleText;
    private ToastPresenter toastPresenter;
    private Image hintPanel;
    private Image interactButtonImage;
    private Color interactBaseColor = new(0.12f, 0.14f, 0.22f, 0.72f);
    private Color interactHotColor = new(0.85f, 0.72f, 0.28f, 0.95f);
    private float interactPulse;
    private float interactFlashTimer;
    private AccessibilitySettingsPanel settingsPanel;
    private Text worldTooltip;
    private Transform canvasContentRoot;

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

        EnsureToastPresenter();
        EnsureButtonFeedback();
        EnsureS3Header();
        EnsureModernOverlays();
        SyncRoomTitleFromScene();
        EnsureSaveDebugMenu();
        ApplyLeftHandedLayout();
    }

    private void EnsureModernOverlays()
    {
        var canvas = GameObject.Find("GameplayCanvas");
        if (canvas == null) return;
        var content = canvas.transform.Find("Viewport/Content");
        canvasContentRoot = content != null ? content : canvas.transform;
        var font = LockeUILayout.GetUIFont();

        // Horizontal top layout: inv (L) · title (bar) · minimap (R) · objective under title
        TopHudLayout.HideLegacyTopChrome(canvasContentRoot);
        var bar = canvasContentRoot.Find("HudBar");
        if (bar != null)
            TopHudLayout.StyleTitleBar(bar.gameObject);

        InventoryStripHUD.Ensure(canvasContentRoot, font);
        MiniMapHUD.Ensure(canvasContentRoot, font);
        ObjectiveTrackerHUD.Ensure(canvasContentRoot, font);
        settingsPanel = AccessibilitySettingsPanel.Ensure(canvasContentRoot, font);

        if (hintPanel != null)
            hintPanel.gameObject.SetActive(false);
        if (hintText != null)
            hintText.gameObject.SetActive(false);

        // Settings tucked under inventory — no overlap with title/minimap
        EnsureSettingsButton(canvasContentRoot, font);

        if (keySlotHud != null)
            keySlotHud.gameObject.SetActive(false);
        if (keySlotImage != null)
            keySlotImage.gameObject.SetActive(false);

        if (worldTooltip == null)
        {
            worldTooltip = CreateText(canvasContentRoot, "WorldTooltip", font, 13, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.42f),
                Vector2.zero, new Vector2(280f, 36f), Color.white);
            worldTooltip.gameObject.SetActive(false);
        }
    }

    private void EnsureSettingsButton(Transform root, Font font)
    {
        var existing = root.Find("SettingsBtn");
        if (existing != null)
        {
            // Park under inventory icon
            var r = existing.GetComponent<RectTransform>();
            if (r != null)
            {
                r.anchorMin = r.anchorMax = new Vector2(0f, 1f);
                r.pivot = new Vector2(0f, 1f);
                r.anchoredPosition = new Vector2(TopHudLayout.EdgePad, -(TopHudLayout.TopInset + TopHudLayout.InvPanelH + 6f));
                r.sizeDelta = new Vector2(32f, 28f);
            }
            return;
        }

        var btn = LockeUIComponents.CreateSecondaryButton(root, font, "⚙",
            new Vector2(0f, 1f), () => settingsPanel?.Show(), 32f);
        btn.name = "SettingsBtn";
        var rect = btn.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(TopHudLayout.EdgePad, -(TopHudLayout.TopInset + TopHudLayout.InvPanelH + 6f));
        rect.sizeDelta = new Vector2(32f, 28f);
        var img = btn.GetComponent<Image>();
        TopHudLayout.ApplyGlass(img);
    }

    public void FlashInteractButton(float duration = 1.5f)
    {
        interactFlashTimer = duration;
        if (interactButton != null)
            interactButton.SetActive(true);
    }

    public void ApplyLeftHandedLayout()
    {
        if (leftButton == null || rightButton == null) return;
        bool left = GameSettings.LeftHanded;
        // Keep wide gap between move and interact clusters.
        SetButtonAnchor(leftButton, left ? 0.90f : 0.10f);
        SetButtonAnchor(rightButton, left ? 0.72f : 0.28f);
        SetButtonAnchor(jumpButton, left ? 0.28f : 0.68f, yExtra: 38f);
        SetButtonAnchor(interactButton, left ? 0.10f : 0.88f);
        SetButtonAnchor(useKeyButton, left ? 0.10f : 0.88f, yExtra: 88f);
    }

    private static void SetButtonAnchor(GameObject go, float x, float yExtra = 22f)
    {
        if (go == null) return;
        var rect = go.GetComponent<RectTransform>();
        if (rect == null) return;
        rect.anchorMin = rect.anchorMax = new Vector2(x, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, yExtra);
    }

    public void SetWorldTooltip(string text)
    {
        if (worldTooltip == null) return;
        if (string.IsNullOrEmpty(text))
        {
            worldTooltip.gameObject.SetActive(false);
            return;
        }

        worldTooltip.text = text;
        worldTooltip.gameObject.SetActive(true);
    }

    private void EnsureToastPresenter()
    {
        if (toastText == null) return;
        toastPresenter = toastText.GetComponent<ToastPresenter>();
        if (toastPresenter == null)
            toastPresenter = toastText.gameObject.AddComponent<ToastPresenter>();
        toastPresenter.Bind(toastText);
    }

    private void EnsureButtonFeedback()
    {
        UIButtonFeedback.Ensure(leftButton);
        UIButtonFeedback.Ensure(rightButton);
        UIButtonFeedback.Ensure(jumpButton);
        UIButtonFeedback.Ensure(interactButton);
        UIButtonFeedback.Ensure(useKeyButton);

        if (interactButton != null)
        {
            interactButtonImage = interactButton.GetComponent<Image>();
            if (interactButtonImage != null)
                interactBaseColor = interactButtonImage.color;
        }
    }

    public void SetRoomTitle(string title)
    {
        if (roomTitleText != null)
            roomTitleText.text = title;
    }

    private void SyncRoomTitleFromScene()
    {
        var director = FindFirstObjectByType<ChapterRoomDirector>();
        if (director == null) return;

        var save = ChapterSaveManager.Instance;
        if (save != null && save.ActiveMapDestination == ChapterMapDestination.Wellhouse)
            SetRoomTitle(ChapterRoomLabels.ForMapDestination(ChapterMapDestination.Wellhouse));
        else
            SetRoomTitle(ChapterRoomLabels.ForRoomId(director.CurrentRoom));
    }

    private void EnsureSaveDebugMenu()
    {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
        return;
#endif
        if (FindFirstObjectByType<ChapterSaveDebugMenu>() != null)
            return;

        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        canvas.gameObject.AddComponent<ChapterSaveDebugMenu>().BindHud(this);
    }

    private void Update()
    {
        RefreshStatus();
        TickToast();
    }

    public void ShowToast(string message, float duration = 3f)
    {
        if (toastPresenter != null)
        {
            toastPresenter.Show(message, duration);
            toastTimer = 0f;
            return;
        }

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
        FindFirstObjectByType<InventoryStripHUD>()?.PlayCollectPop();
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
        {
            keyStatusIcon.sprite = ResolveActiveKeyIcon();
            keyStatusIcon.enabled = keyStatusIcon.sprite != null;
        }

        keySlotHud?.Refresh();

        if (houseKeyIcon != null && iconLibrary != null)
        {
            houseKeyIcon.gameObject.SetActive(gameplay.HasHouseKey);
            houseKeyIcon.sprite = iconLibrary.houseKeyIcon;
        }

        // Pulse Interact button when something is in range or tutorial flash.
        var interaction = gameplay.interaction;
        var canInteract = interaction != null
            && interaction.NearestInteractable != null
            && interaction.NearestInteractable.CanInteract;

        if (interactFlashTimer > 0f)
            interactFlashTimer -= Time.deltaTime;

        if (interactButtonImage != null && interactButton != null && interactButton.activeSelf)
        {
            if (canInteract || interactFlashTimer > 0f)
            {
                interactPulse += Time.deltaTime * 6f;
                var k = 0.55f + Mathf.Sin(interactPulse) * 0.45f;
                interactButtonImage.color = Color.Lerp(interactBaseColor, interactHotColor, k);
                interactButton.transform.localScale = Vector3.one * (1f + Mathf.Sin(interactPulse) * 0.08f);
            }
            else
            {
                interactButtonImage.color = Color.Lerp(interactButtonImage.color, interactBaseColor, Time.deltaTime * 8f);
                interactButton.transform.localScale = Vector3.Lerp(interactButton.transform.localScale, Vector3.one, Time.deltaTime * 10f);
            }
        }

        // Floating tooltip for nearest interactable
        if (canInteract && interaction.NearestInteractable != null)
        {
            var hint = interaction.NearestInteractable.InteractionHint;
            SetWorldTooltip(string.IsNullOrEmpty(hint) ? "Tap Interact" : hint);
        }
        else
        {
            SetWorldTooltip(null);
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
        if (bindings == null)
        {
            Destroy(instance);
            return false;
        }

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

    private void EnsureS3Header()
    {
        if (roomTitleText != null) return;

        var hudRoot = GameObject.Find("GameplayCanvas");
        if (hudRoot == null) return;

        var content = hudRoot.transform.Find("Viewport/Content");
        var parent = content != null ? content : hudRoot.transform;

        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var bar = LockeUIComponents.CreateHudBar(parent, font,
            ChapterRoomLabels.ForRoomId(ChapterRoomZone.RoomId.Foyer),
            () => GrokUIFlowManager.Instance?.ShowChapterMap(),
            () => GrokUIFlowManager.Instance?.ShowKeyRing());
        roomTitleText = bar.transform.Find("Title")?.GetComponent<Text>();
    }

    private void WirePrefabButtons()
    {
        if (leftButton != null)
            WireHoldButton(leftButton, () => gameplay?.SetMoveInput(-1f), () => gameplay?.SetMoveInput(0f));
        if (rightButton != null)
            WireHoldButton(rightButton, () => gameplay?.SetMoveInput(1f), () => gameplay?.SetMoveInput(0f));
        if (jumpButton != null)
            WireJumpHold(jumpButton);
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
        UIButtonFeedback.Ensure(buttonGo);
        ApplyButtonColors(button);
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
        UIButtonFeedback.Ensure(buttonGo);
    }

    private static void ApplyButtonColors(Button button)
    {
        if (button == null) return;
        var colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.97f, 0.88f, 1f);
        colors.pressedColor = new Color(0.85f, 0.82f, 0.7f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.5f, 0.5f, 0.55f, 0.55f);
        colors.fadeDuration = 0.08f;
        button.colors = colors;
        button.transition = Selectable.Transition.ColorTint;
    }

    private void BuildCanvas()
    {
        if (GameObject.Find("GameplayCanvas") != null)
            return;

        EnsureEventSystem();

        var flow = LockeUILayout.CreateFlowCanvas("GameplayCanvas", 100);
        var canvasRoot = LockeUILayout.GetContentRoot(flow);
        var font = flow.Font ?? LockeUILayout.GetUIFont();
        var panelColor = new Color(0.05f, 0.06f, 0.1f, 0.82f);
        var buttonColor = new Color(0.12f, 0.14f, 0.22f, 0.95f);
        var accentColor = LockeKeyUITheme.LKGold;
        var contentW = LockeKeyUITheme.HudContentWidth;

        var bar = LockeUIComponents.CreateHudBar(canvasRoot, font,
            ChapterRoomLabels.ForRoomId(ChapterRoomZone.RoomId.Foyer),
            () => GrokUIFlowManager.Instance?.ShowChapterMap(),
            () => GrokUIFlowManager.Instance?.ShowKeyRing());
        roomTitleText = bar.transform.Find("Title")?.GetComponent<Text>();

        // Legacy key slot / status icons removed from top chrome (inventory + minimap own those slots).
        keySlotImage = null;
        keySlotHud = null;
        keyStatusIcon = null;
        keyStatusText = null;
        houseKeyIcon = null;
        houseKeyText = null;

        // Legacy hint plate kept but hidden — ObjectiveTrackerHUD is the modern quest card.
        var hintPlate = CreatePanel(canvasRoot, "HintPlate", new Color(0.04f, 0.05f, 0.09f, 0f),
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, LockeKeyUITheme.ControlBarHeight + 8f),
            new Vector2(contentW, 52f));
        hintPlate.SetActive(false);
        hintPanel = hintPlate.GetComponent<Image>();
        hintText = CreateText(hintPlate.transform, "Hint", font, LockeKeyUITheme.BodySize, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(contentW - 16f, 48f), LockeKeyUITheme.BodyText);

        toastText = CreateText(canvasRoot, "Toast", font, LockeKeyUITheme.BodySize, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.68f), new Vector2(0.5f, 0.68f),
            Vector2.zero, new Vector2(contentW - 24f, 48f), LockeKeyUITheme.LKGold);
        toastText.gameObject.SetActive(true);
        var toastBg = toastText.gameObject.AddComponent<Outline>();
        toastBg.effectColor = new Color(0f, 0f, 0f, 0.65f);
        toastBg.effectDistance = new Vector2(1.2f, -1.2f);

        // Transparent floating control layer (no solid bar) — modern mobile adventure style.
        var controlBar = new GameObject("ControlBar", typeof(RectTransform));
        controlBar.transform.SetParent(canvasRoot, false);
        var barRect = controlBar.GetComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0f, 0f);
        barRect.anchorMax = new Vector2(1f, 0f);
        barRect.pivot = new Vector2(0.5f, 0f);
        barRect.sizeDelta = new Vector2(0f, 130f);
        barRect.anchoredPosition = Vector2.zero;

        // Circular glass buttons — wide gap between move (left) and actions (right)
        var glass = new Color(0.08f, 0.09f, 0.13f, 0.52f);
        float moveBtn = 78f;
        float y = 28f;

        leftButton = CreatePortraitHoldButton(controlBar.transform, "Left", iconLibrary?.moveLeft, font,
            glass, accentColor, 0.10f, y, moveBtn,
            () => gameplay?.SetMoveInput(-1f), () => gameplay?.SetMoveInput(0f));

        rightButton = CreatePortraitHoldButton(controlBar.transform, "Right", iconLibrary?.moveRight, font,
            glass, accentColor, 0.28f, y, moveBtn,
            () => gameplay?.SetMoveInput(1f), () => gameplay?.SetMoveInput(0f));

        // Gap across center of screen — actions start ~0.68
        jumpButton = CreatePortraitTapButton(controlBar.transform, "Jump", iconLibrary?.jump, font,
            glass, accentColor, 0.68f, y + 10f, 70f, () => gameplay?.RequestJump());

        interactButton = CreatePortraitTapButton(controlBar.transform, "Interact", iconLibrary?.interact, font,
            new Color(0.12f, 0.14f, 0.1f, 0.58f), accentColor, 0.88f, y, 80f, () => gameplay?.RequestInteract());

        useKeyButton = CreatePortraitTapButton(controlBar.transform, "UseKey", iconLibrary?.useKey, font,
            glass, accentColor, 0.88f, y + 88f, 64f, () => gameplay?.RequestUseKey());

        WireJumpHold(jumpButton);
        MakeCircularGlass(leftButton);
        MakeCircularGlass(rightButton);
        MakeCircularGlass(jumpButton);
        MakeCircularGlass(interactButton);
        MakeCircularGlass(useKeyButton);

        SetControlVisibility(interact: true, jump: true, useKey: false);

        memoryOverlay = BuildMemoryOverlay(canvasRoot, font, panelColor, accentColor);
        memoryOverlay.SetActive(false);

        EnsureToastPresenter();
        EnsureButtonFeedback();
        canvasContentRoot = canvasRoot;
    }

    private static void MakeCircularGlass(GameObject go)
    {
        if (go == null) return;
        var img = go.GetComponent<Image>();
        if (img != null)
            img.color = new Color(0.08f, 0.09f, 0.13f, 0.52f);

        var shadow = go.GetComponent<Shadow>() ?? go.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.55f);
        shadow.effectDistance = new Vector2(0f, -4f);

        // Soft edge (glass ring) — not a hard square border
        var outline = go.GetComponent<Outline>() ?? go.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 1f, 1f, 0.14f);
        outline.effectDistance = new Vector2(1.4f, -1.4f);

        UIButtonFeedback.Ensure(go);
    }

    private void WireJumpHold(GameObject jumpGo)
    {
        if (jumpGo == null) return;

        // Prefer hold events so variable jump cut works; clear tap-to-jump to avoid double fire.
        var button = jumpGo.GetComponent<Button>();
        if (button != null)
            button.onClick.RemoveAllListeners();

        var hold = jumpGo.GetComponent<HoldButton>();
        if (hold == null)
            hold = jumpGo.AddComponent<HoldButton>();
        hold.onDown.RemoveAllListeners();
        hold.onUp.RemoveAllListeners();
        hold.onDown.AddListener(() =>
        {
            gameplay?.RequestJump();
            gameplay?.player?.SetJumpHeld(true);
        });
        hold.onUp.AddListener(() => gameplay?.player?.SetJumpHeld(false));
        UIButtonFeedback.Ensure(jumpGo);
    }

    private static GameObject CreatePortraitTapButton(Transform parent, string label, Sprite icon, Font font,
        Color bg, Color textColor, float anchorX, float bottomInset, float size,
        UnityEngine.Events.UnityAction onTap)
    {
        return CreateTapButton(parent, label, icon, font, bg, textColor,
            new Vector2(anchorX, 0f), new Vector2(anchorX, 0f),
            new Vector2(0f, bottomInset), new Vector2(size, size), onTap);
    }

    private static GameObject CreatePortraitHoldButton(Transform parent, string label, Sprite icon, Font font,
        Color bg, Color textColor, float anchorX, float bottomInset, float size,
        UnityEngine.Events.UnityAction onDown, UnityEngine.Events.UnityAction onUp)
    {
        return CreateHoldButton(parent, label, icon, font, bg, textColor,
            new Vector2(anchorX, 0f), new Vector2(anchorX, 0f),
            new Vector2(0f, bottomInset), new Vector2(size, size), onDown, onUp);
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

        var button = go.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onTap);

        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = sizeDelta;

        StyleTouchButton(go.transform, label, icon, font, textColor);
        return go;
    }

    private static GameObject CreateHoldButton(Transform parent, string label, Sprite icon, Font font, Color bg, Color textColor,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta,
        UnityEngine.Events.UnityAction onDown, UnityEngine.Events.UnityAction onUp)
    {
        var go = new GameObject(label + "Button", typeof(RectTransform), typeof(Image), typeof(HoldButton));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.color = bg;

        var hold = go.GetComponent<HoldButton>();
        hold.onDown.AddListener(onDown);
        hold.onUp.AddListener(onUp);

        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = sizeDelta;

        StyleTouchButton(go.transform, label, icon, font, textColor);
        return go;
    }

    private static void StyleTouchButton(Transform button, string label, Sprite icon, Font font, Color textColor)
    {
        var outline = button.gameObject.GetComponent<Outline>() ?? button.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(LockeKeyUITheme.LKGold.r, LockeKeyUITheme.LKGold.g, LockeKeyUITheme.LKGold.b, 0.55f);
        outline.effectDistance = new Vector2(1.8f, -1.8f);

        // Soft inner plate so icons read clearly on dark bars
        var plate = new GameObject("InnerPlate", typeof(RectTransform), typeof(Image));
        plate.transform.SetParent(button, false);
        plate.transform.SetAsFirstSibling();
        var plateImg = plate.GetComponent<Image>();
        plateImg.color = new Color(1f, 1f, 1f, 0.06f);
        plateImg.raycastTarget = false;
        var plateRect = plate.GetComponent<RectTransform>();
        plateRect.anchorMin = new Vector2(0.08f, 0.08f);
        plateRect.anchorMax = new Vector2(0.92f, 0.92f);
        plateRect.offsetMin = plateRect.offsetMax = Vector2.zero;

        if (icon != null)
        {
            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(button, false);
            var iconImg = iconGo.GetComponent<Image>();
            iconImg.sprite = icon;
            iconImg.preserveAspect = true;
            iconImg.color = Color.white;
            iconImg.raycastTarget = false;

            var iconRect = iconGo.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.16f, 0.16f);
            iconRect.anchorMax = new Vector2(0.84f, 0.84f);
            iconRect.offsetMin = iconRect.offsetMax = Vector2.zero;
        }
        else
        {
            var shortLabel = label.Length > 4 ? label[..4] : label;
            CreateText(button, "Label", font, LockeKeyUITheme.CaptionSize + 1, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, textColor).text = shortLabel;
        }

        UIButtonFeedback.Ensure(button.gameObject);
        var btn = button.GetComponent<Button>();
        if (btn != null)
            ApplyButtonColors(btn);
    }

    private static void SetStretch(RectTransform r)
    {
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.offsetMin = r.offsetMax = Vector2.zero;
    }
}
