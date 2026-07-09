using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Master overlay coordinator for the S2-S8 screen catalog.
///
/// Public API (called by pickups, doors, HUD buttons):
///   ShowChapterMap()
///   HideChapterMap()
///   ShowDiscovery(keyId, onAdded, onAddedAndEquipped)
///   ShowLock(def, onSuccess)
///   ShowKeyRing()
///   ShowToast(message)
///   bool IsEquipped(keyId)
/// </summary>
public class GrokUIFlowManager : MonoBehaviour
{
    public static GrokUIFlowManager Instance { get; private set; }

    // ── shared canvas ────────────────────────────────────────────────────
    private Font font;

    // ── overlay groups ───────────────────────────────────────────────────
    private CanvasGroup mapGroup;
    private CanvasGroup discoveryGroup;
    private CanvasGroup lockGroup;
    private CanvasGroup ringGroup;
    private CanvasGroup toastGroup;

    // ── S2 map live refs ─────────────────────────────────────────────────
    private GameObject foyerCheckmark;
    private Button wellhouseBtn;
    private GameObject wellhouseLock;
    private Text keyCountLabel;
    private RectTransform mapProgressFill;

    // ── S5 lock ↔ S6 ring return ──────────────────────────────────────────
    private bool ringOpenedFromLock;

    // ── S4 discovery live refs ────────────────────────────────────────────
    private Image discoveryKeyImage;
    private Text discoveryKeyName;
    private Text discoveryKeyDesc;
    private Button addEquipBtn;
    private Button addOnlyBtn;

    // ── S5 lock live refs ─────────────────────────────────────────────────
    private Text lockEquippedLabel;
    private Button lockTryBtn;
    private LockDefinition activeLockDef;
    private Action activeLockSuccess;

    // ── S6 ring live refs ─────────────────────────────────────────────────
    private Image[] ringSlotImages;
    private Text ringDetailName;
    private Text ringDetailDesc;
    private Button ringEquipBtn;
    private string selectedKeyId;

    // ── toast ─────────────────────────────────────────────────────────────
    private Text toastText;
    private Coroutine toastCoroutine;

    // ────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        BuildCanvas();

        // Consume boot flag
        if (GameBootContext.OpenMapOnStart)
        {
            GameBootContext.Reset();
            ShowChapterMap();
        }
    }

    // ════════════════════════════════════════════════════════════════════
    // Public API
    // ════════════════════════════════════════════════════════════════════

    public void ShowChapterMap()
    {
        RefreshMap();
        SetOverlay(mapGroup, true);
    }

    public void HideChapterMap()
    {
        SetOverlay(mapGroup, false);
        EnsureGameplayVisible();
    }

    public void ShowDiscovery(string keyId,
        Action onAdded, Action onAddedAndEquipped)
    {
        var sprite = Resources.Load<Sprite>(ArtPaths.KeySpriteForId(keyId));
        discoveryKeyImage.sprite = sprite;
        discoveryKeyImage.color  = sprite != null ? Color.white
                                                  : new Color(0.25f, 0.25f, 0.25f);
        discoveryKeyName.text = ArtPaths.KeyDisplayName(keyId);
        discoveryKeyDesc.text = KeyDescription(keyId);

        // Wire buttons for this specific discovery
        addEquipBtn.onClick.RemoveAllListeners();
        addEquipBtn.onClick.AddListener(() =>
        {
            SetOverlay(discoveryGroup, false);
            onAddedAndEquipped?.Invoke();
        });

        addOnlyBtn.onClick.RemoveAllListeners();
        addOnlyBtn.onClick.AddListener(() =>
        {
            SetOverlay(discoveryGroup, false);
            onAdded?.Invoke();
        });

        SetOverlay(discoveryGroup, true);
    }

    public void ShowLock(LockDefinition def, Action onSuccess)
    {
        activeLockDef    = def;
        activeLockSuccess = onSuccess;
        ringOpenedFromLock = false;
        RefreshLockState();
        SetOverlay(lockGroup, true);
    }

    public void ShowKeyRing(bool fromLock = false)
    {
        ringOpenedFromLock = fromLock;
        RefreshRing();
        SetOverlay(ringGroup, true);
    }

    public void ShowToast(string message)
    {
        if (toastCoroutine != null) StopCoroutine(toastCoroutine);
        toastText.text = message;
        toastGroup.alpha = 1f;
        toastGroup.blocksRaycasts = false;
        toastCoroutine = StartCoroutine(FadeOutToast(2.2f));
    }

    public bool IsEquipped(string keyId) =>
        ChapterSaveManager.Instance?.Data.equippedKeyId == keyId;

    // ════════════════════════════════════════════════════════════════════
    // Refresh helpers
    // ════════════════════════════════════════════════════════════════════

    private void RefreshMap()
    {
        var save = ChapterSaveManager.Instance;
        if (save == null) return;

        bool foyerSolved = save.IsHotspotSolved("foyer_stair_door");
        bool wellhouseUnlocked = save.IsRoomUnlocked("wellhouse");

        // Foyer checkmark
        if (foyerCheckmark != null)
            foyerCheckmark.SetActive(foyerSolved);

        // Wellhouse lock / button
        if (wellhouseBtn != null)
            wellhouseBtn.interactable = wellhouseUnlocked;
        if (wellhouseLock != null)
            wellhouseLock.SetActive(!wellhouseUnlocked);

        // Key count + progression bar
        int discovered = save.Data.discoveredKeyIds?.Count ?? 0;
        if (keyCountLabel != null)
            keyCountLabel.text = $"{discovered} / {KeyCatalog.Count} keys";

        if (mapProgressFill != null)
        {
            float pct = KeyCatalog.Count > 0 ? (float)discovered / KeyCatalog.Count : 0f;
            mapProgressFill.anchorMax = new Vector2(Mathf.Clamp01(pct), 1f);
        }
    }

    private void RefreshLockState()
    {
        if (activeLockDef == null) return;

        bool correct = IsEquipped(activeLockDef.requiredKeyId);
        string reqName = ArtPaths.KeyDisplayName(activeLockDef.requiredKeyId);

        if (lockEquippedLabel != null)
        {
            if (correct)
            {
                lockEquippedLabel.text = $"Equipped: {reqName} ✓";
                lockEquippedLabel.color = LockeKeyUITheme.Success;
            }
            else
            {
                lockEquippedLabel.text = $"Requires: {reqName}";
                lockEquippedLabel.color = LockeKeyUITheme.LKIron;
            }
        }

        if (lockTryBtn != null)
        {
            lockTryBtn.interactable = correct;
            var img = lockTryBtn.GetComponent<Image>();
            if (img) img.color = correct ? LockeKeyUITheme.LKGold : new Color(0.35f, 0.35f, 0.35f, 0.4f);
        }
    }

    private void RefreshRing()
    {
        var save = ChapterSaveManager.Instance;
        if (save == null || ringSlotImages == null) return;

        for (int i = 0; i < KeyCatalog.Count && i < ringSlotImages.Length; i++)
        {
            string kid = KeyCatalog.AllKeyIds[i];
            bool owned = save.HasKeyDiscovered(kid);
            var sprite = owned
                ? Resources.Load<Sprite>(ArtPaths.KeySpriteForId(kid))
                : Resources.Load<Sprite>(ArtPaths.UiKeySlotEmpty);
            ringSlotImages[i].sprite = sprite;
            ringSlotImages[i].color  = Color.white;

            // Gold ring on equipped slot
            var equipped = save.Data.equippedKeyId == kid && owned;
            var border = ringSlotImages[i].transform.Find("Ring")?.GetComponent<Image>();
            if (border) border.color = equipped
                ? LockeKeyUITheme.LKGold
                : new Color(0.3f, 0.3f, 0.3f, 0.35f);
        }
    }

    // ════════════════════════════════════════════════════════════════════
    // Canvas builder
    // ════════════════════════════════════════════════════════════════════

    private void BuildCanvas()
    {
        var flow = LockeUILayout.CreateFlowCanvas("GrokOverlayCanvas", 300);
        font = flow.Font;
        var root = LockeUILayout.GetContentRoot(flow);

        mapGroup       = BuildMapPanel(root);
        discoveryGroup = BuildDiscoveryPanel(root);
        lockGroup      = BuildLockPanel(root);
        ringGroup      = BuildRingPanel(root);
        toastGroup     = BuildToast(root);

        foreach (var cg in new[] { mapGroup, discoveryGroup, lockGroup, ringGroup })
            SetOverlay(cg, false);
    }

    // ── S2 Chapter Map ───────────────────────────────────────────────────

    private CanvasGroup BuildMapPanel(Transform root)
    {
        var panel = MakeFullPanel(root, "ChapterMap", LockeKeyUITheme.LKMoon);
        var cg = panel.AddComponent<CanvasGroup>();

        LockeUIComponents.AddText(panel.transform, "MapTitle", font, LockeKeyUITheme.DisplaySize - 2,
            FontStyle.Bold, LockeKeyUITheme.LKGold, new Vector2(0.5f, 0.92f),
            "Keyhouse — Chapter 1", new Vector2(LockeKeyUITheme.RefWidth - 40f, 42f), TextAnchor.MiddleCenter);

        var foyer = LockeUIComponents.CreateChapterCard(panel.transform, font, "Foyer",
            new Vector2(0.5f, 0.72f), true, ArtPaths.BgFoyerPortrait,
            () => LoadMapDestination(ChapterMapDestination.Foyer));
        foyerCheckmark = LockeUIComponents.AddText(foyer.cardBtn.transform, "Checkmark", font, 22,
            FontStyle.Bold, LockeKeyUITheme.Success, new Vector2(0.92f, 0.72f), "✓",
            new Vector2(28f, 28f), TextAnchor.MiddleCenter).gameObject;
        foyerCheckmark.SetActive(false);

        var well = LockeUIComponents.CreateChapterCard(panel.transform, font, "Wellhouse",
            new Vector2(0.5f, 0.52f), false, ArtPaths.BgWellhouse,
            () => LoadMapDestination(ChapterMapDestination.Wellhouse));
        wellhouseBtn = well.cardBtn;
        wellhouseLock = well.lockIcon;

        LockeUIComponents.CreateChapterCard(panel.transform, font, "The Black Door",
            new Vector2(0.5f, 0.32f), false, ArtPaths.BgBlackDoor,
            () => ShowToast("This door has no keyhole."));

        LockeUIComponents.AddText(panel.transform, "ProgressLabel", font, LockeKeyUITheme.CaptionSize,
            FontStyle.Normal, LockeKeyUITheme.CaptionText, new Vector2(0.12f, 0.16f),
            "Progression", new Vector2(120f, 24f), TextAnchor.MiddleLeft);

        keyCountLabel = LockeUIComponents.AddText(panel.transform, "KeyCount", font, LockeKeyUITheme.CaptionSize,
            FontStyle.Normal, LockeKeyUITheme.CaptionText, new Vector2(0.88f, 0.16f),
            $"0 / {KeyCatalog.Count} keys", new Vector2(140f, 24f), TextAnchor.MiddleRight);

        mapProgressFill = BuildMapProgressBar(panel.transform);

        LockeUIComponents.CreateSecondaryButton(panel.transform, font, "Close Map",
            new Vector2(0.5f, 0.04f), HideChapterMap, 240f);

        return cg;
    }

    // ── S4 Key Discovery ─────────────────────────────────────────────────

    private CanvasGroup BuildDiscoveryPanel(Transform root)
    {
        var scrimGo = new GameObject("DiscoveryScrim", typeof(RectTransform));
        scrimGo.transform.SetParent(root, false);
        StretchFull(scrimGo.GetComponent<RectTransform>());
        LockeUIComponents.CreateScrim(scrimGo.transform, () => SetOverlay(discoveryGroup, false));
        var cg = scrimGo.AddComponent<CanvasGroup>();

        LockeUIComponents.CreateBottomSheet(scrimGo.transform, 0.58f, out _, out var sheetImage);
        var sheet = sheetImage.transform;
        sheetImage.color = LockeKeyUITheme.SheetBottom;

        var keyImgGo = new GameObject("KeyImage", typeof(RectTransform), typeof(Image));
        keyImgGo.transform.SetParent(sheet, false);
        var kiRect = keyImgGo.GetComponent<RectTransform>();
        kiRect.anchorMin = new Vector2(0.28f, 0.58f);
        kiRect.anchorMax = new Vector2(0.72f, 0.92f);
        kiRect.offsetMin = kiRect.offsetMax = Vector2.zero;
        discoveryKeyImage = keyImgGo.GetComponent<Image>();
        discoveryKeyImage.preserveAspect = true;

        discoveryKeyName = LockeUIComponents.AddText(sheet, "KeyName", font, LockeKeyUITheme.TitleSize + 2,
            FontStyle.Bold, LockeKeyUITheme.LKGold, new Vector2(0.5f, 0.46f), "",
            new Vector2(320f, 36f), TextAnchor.MiddleCenter);

        discoveryKeyDesc = LockeUIComponents.AddText(sheet, "KeyDesc", font, LockeKeyUITheme.BodySize,
            FontStyle.Normal, LockeKeyUITheme.BodyText, new Vector2(0.5f, 0.30f), "",
            new Vector2(340f, 72f), TextAnchor.UpperCenter);
        discoveryKeyDesc.lineSpacing = 1.25f;

        addEquipBtn = LockeUIComponents.CreatePrimaryButton(sheet, font, "Add to Ring & Equip",
            new Vector2(0.5f, 0.14f), null, 300f);
        addOnlyBtn = LockeUIComponents.CreateSecondaryButton(sheet, font, "Add to Ring",
            new Vector2(0.5f, 0.05f), null, 300f);

        return cg;
    }

    // ── S5 Lock Puzzle ───────────────────────────────────────────────────

    private CanvasGroup BuildLockPanel(Transform root)
    {
        var scrimGo = new GameObject("LockScrim",
            typeof(RectTransform), typeof(Image), typeof(Button));
        scrimGo.transform.SetParent(root, false);
        StretchFull(scrimGo.GetComponent<RectTransform>());
        scrimGo.GetComponent<Image>().color = LockeKeyUITheme.OverlayScrim;
        scrimGo.GetComponent<Button>().onClick.AddListener(() =>
            SetOverlay(lockGroup, false));
        var cg = scrimGo.AddComponent<CanvasGroup>();

        var sheet = new GameObject("LockSheet",
            typeof(RectTransform), typeof(Image));
        sheet.transform.SetParent(scrimGo.transform, false);
        var sRect = sheet.GetComponent<RectTransform>();
        sRect.anchorMin = new Vector2(0f, 0f);
        sRect.anchorMax = new Vector2(1f, 0.44f);
        sRect.offsetMin = sRect.offsetMax = Vector2.zero;
        sheet.GetComponent<Image>().color = new Color(0.07f, 0.08f, 0.12f);

        LockeUIComponents.AddText(sheet.transform, "LockTitle", font, LockeKeyUITheme.TitleSize + 2,
            FontStyle.Bold, LockeKeyUITheme.LKGold, new Vector2(0.5f, 0.88f),
            "Locked Door", new Vector2(320f, 38f), TextAnchor.MiddleCenter);

        // Equipped status
        var eqGo = new GameObject("EquippedLabel",
            typeof(RectTransform), typeof(Text));
        eqGo.transform.SetParent(sheet.transform, false);
        var eRect = eqGo.GetComponent<RectTransform>();
        eRect.anchorMin = new Vector2(0.06f, 0.60f);
        eRect.anchorMax = new Vector2(0.94f, 0.76f);
        eRect.offsetMin = eRect.offsetMax = Vector2.zero;
        lockEquippedLabel = eqGo.GetComponent<Text>();
        lockEquippedLabel.font = font;
        lockEquippedLabel.fontSize = 20;
        lockEquippedLabel.alignment = TextAnchor.MiddleCenter;
        lockEquippedLabel.text = "No key equipped";
        lockEquippedLabel.color = LockeKeyUITheme.LKIron;

        LockeUIComponents.AddText(sheet.transform, "LockHint", font, LockeKeyUITheme.CaptionSize + 4,
            FontStyle.Normal, LockeKeyUITheme.CaptionText, new Vector2(0.5f, 0.50f),
            "Equip the correct key from your key ring, then tap Try Key.",
            new Vector2(320f, 44f), TextAnchor.MiddleCenter);

        lockTryBtn = LockeUIComponents.CreatePrimaryButton(sheet.transform, font, "Try Key",
            new Vector2(0.5f, 0.25f), HandleLockTry, 260f);
        lockTryBtn.interactable = false;
        lockTryBtn.GetComponent<Image>().color = new Color(0.35f, 0.35f, 0.35f, 0.4f);

        LockeUIComponents.CreateSecondaryButton(sheet.transform, font, "Open Key Ring",
            new Vector2(0.5f, 0.09f), () =>
            {
                SetOverlay(lockGroup, false);
                ShowKeyRing(fromLock: true);
            }, 220f);

        return cg;
    }

    private void HandleLockTry()
    {
        if (activeLockDef == null) return;
        if (!IsEquipped(activeLockDef.requiredKeyId)) { RefreshLockState(); return; }

        // Success
        var save = ChapterSaveManager.Instance;
        save?.RecordHotspotSolved(activeLockDef.hotspotId);
        if (!string.IsNullOrEmpty(activeLockDef.unlockRoomId))
            save?.RecordRoomUnlocked(activeLockDef.unlockRoomId);

        SetOverlay(lockGroup, false);
        activeLockSuccess?.Invoke();
        ShowToast("Door unlocked! Wellhouse now accessible.");
        ShowChapterMap();
    }

    // ── S6 Key Ring ───────────────────────────────────────────────────────

    private CanvasGroup BuildRingPanel(Transform root)
    {
        var scrimGo = new GameObject("RingScrim",
            typeof(RectTransform), typeof(Image), typeof(Button));
        scrimGo.transform.SetParent(root, false);
        StretchFull(scrimGo.GetComponent<RectTransform>());
        scrimGo.GetComponent<Image>().color = LockeKeyUITheme.OverlayScrim;
        scrimGo.GetComponent<Button>().onClick.AddListener(CloseKeyRing);
        var cg = scrimGo.AddComponent<CanvasGroup>();

        // Sheet: bottom 72%
        var sheet = new GameObject("RingSheet",
            typeof(RectTransform), typeof(Image));
        sheet.transform.SetParent(scrimGo.transform, false);
        var sRect = sheet.GetComponent<RectTransform>();
        sRect.anchorMin = new Vector2(0f, 0f);
        sRect.anchorMax = new Vector2(1f, 0.74f);
        sRect.offsetMin = sRect.offsetMax = Vector2.zero;
        sheet.GetComponent<Image>().color = new Color(0.06f, 0.07f, 0.10f);

        LockeUIComponents.AddText(sheet.transform, "RingTitle", font, LockeKeyUITheme.TitleSize + 2,
            FontStyle.Bold, LockeKeyUITheme.LKGold, new Vector2(0.5f, 0.94f),
            "Key Ring", new Vector2(300f, 36f), TextAnchor.MiddleCenter);

        int totalSlots = 15;
        ringSlotImages = new Image[totalSlots];
        int cols = 3;

        for (int i = 0; i < totalSlots; i++)
        {
            int col = i % cols;
            int row = i / cols;
            float cx = 0.18f + col * 0.32f;
            float cy = 0.75f - row * 0.175f;
            string keyId = i < KeyCatalog.Count ? KeyCatalog.AllKeyIds[i] : null;

            if (keyId == null)
            {
                var empty = new GameObject($"Slot_{i}", typeof(RectTransform), typeof(Image));
                empty.transform.SetParent(sheet.transform, false);
                var eRect = empty.GetComponent<RectTransform>();
                eRect.anchorMin = eRect.anchorMax = new Vector2(cx, cy);
                eRect.sizeDelta = new Vector2(LockeKeyUITheme.KeySlotSize, LockeKeyUITheme.KeySlotSize);
                ringSlotImages[i] = empty.GetComponent<Image>();
                ringSlotImages[i].color = new Color(0.12f, 0.13f, 0.18f, 0.5f);
                continue;
            }

            var (slotImg, _) = LockeUIComponents.CreateKeySlot(sheet.transform, new Vector2(cx, cy), keyId);
            ringSlotImages[i] = slotImg;
            var slotBtn = slotImg.GetComponent<Button>() ?? slotImg.gameObject.AddComponent<Button>();
            slotBtn.targetGraphic = slotImg;
            string kid = keyId;
            slotBtn.onClick.AddListener(() => { selectedKeyId = kid; RefreshRingDetail(); });
        }

        // Detail panel (right of grid) — name + desc + equip
        var detailGo = new GameObject("RingDetail",
            typeof(RectTransform), typeof(Image));
        detailGo.transform.SetParent(sheet.transform, false);
        var dRect2 = detailGo.GetComponent<RectTransform>();
        dRect2.anchorMin = new Vector2(0.0f, 0.0f);
        dRect2.anchorMax = new Vector2(1.0f, 0.12f);
        dRect2.offsetMin = dRect2.offsetMax = Vector2.zero;
        detailGo.GetComponent<Image>().color = new Color(0.10f, 0.11f, 0.16f);

        var dnGo = new GameObject("DetailName",
            typeof(RectTransform), typeof(Text));
        dnGo.transform.SetParent(detailGo.transform, false);
        var dnRect = dnGo.GetComponent<RectTransform>();
        dnRect.anchorMin = new Vector2(0.04f, 0.52f);
        dnRect.anchorMax = new Vector2(0.60f, 0.98f);
        dnRect.offsetMin = dnRect.offsetMax = Vector2.zero;
        ringDetailName = dnGo.GetComponent<Text>();
        ringDetailName.font = font;
        ringDetailName.fontSize = 18;
        ringDetailName.fontStyle = FontStyle.Bold;
        ringDetailName.color = new Color(0.88f, 0.84f, 0.76f);
        ringDetailName.text = "Select a key";
        ringDetailName.alignment = TextAnchor.UpperLeft;

        var ddGo = new GameObject("DetailDesc",
            typeof(RectTransform), typeof(Text));
        ddGo.transform.SetParent(detailGo.transform, false);
        var ddRect = ddGo.GetComponent<RectTransform>();
        ddRect.anchorMin = new Vector2(0.04f, 0.02f);
        ddRect.anchorMax = new Vector2(0.60f, 0.50f);
        ddRect.offsetMin = ddRect.offsetMax = Vector2.zero;
        ringDetailDesc = ddGo.GetComponent<Text>();
        ringDetailDesc.font = font;
        ringDetailDesc.fontSize = 14;
        ringDetailDesc.color = new Color(0.58f, 0.58f, 0.64f);
        ringDetailDesc.text = "";
        ringDetailDesc.alignment = TextAnchor.UpperLeft;

        ringEquipBtn = LockeUIComponents.CreatePrimaryButton(detailGo.transform, font, "Equip",
            new Vector2(0.80f, 0.5f), HandleEquipSelected, 110f);
        ringEquipBtn.interactable = false;

        LockeUIComponents.CreateSecondaryButton(sheet.transform, font, "Close",
            new Vector2(0.94f, 0.96f), CloseKeyRing, 72f);

        return cg;
    }

    private void RefreshRingDetail()
    {
        if (string.IsNullOrEmpty(selectedKeyId)) return;
        var save = ChapterSaveManager.Instance;
        bool owned = save?.HasKeyDiscovered(selectedKeyId) ?? false;

        ringDetailName.text = ArtPaths.KeyDisplayName(selectedKeyId);
        ringDetailDesc.text = owned ? KeyDescription(selectedKeyId) : "Not yet discovered.";
        ringEquipBtn.interactable = owned;

        RefreshRing();
    }

    private void HandleEquipSelected()
    {
        if (string.IsNullOrEmpty(selectedKeyId)) return;
        ChapterSaveManager.Instance?.RecordEquippedKey(selectedKeyId);
        ShowToast($"Equipped: {ArtPaths.KeyDisplayName(selectedKeyId)}");
        RefreshRing();
    }

    // ── S8 Toast ──────────────────────────────────────────────────────────

    private CanvasGroup BuildToast(Transform root)
    {
        toastText = LockeUIComponents.CreateToastHost(root, font, out var cg);
        return cg;
    }

    private IEnumerator FadeOutToast(float delay)
    {
        yield return new WaitForSeconds(delay);
        float t = 0f;
        while (t < 0.5f)
        {
            t += Time.deltaTime;
            toastGroup.alpha = 1f - (t / 0.5f);
            yield return null;
        }
        toastGroup.alpha = 0f;
    }

    // ── Utility ───────────────────────────────────────────────────────────

    private void LoadMapDestination(string destinationId)
    {
        if (destinationId == ChapterMapDestination.Wellhouse)
        {
            var save = ChapterSaveManager.Instance;
            if (save == null || !save.IsHotspotSolved("foyer_stair_door"))
            {
                ShowToast("Wellhouse is locked.");
                return;
            }
        }

        HideChapterMap();
        FindFirstObjectByType<ChapterRoomDirector>()?.LoadMapDestination(destinationId);
        EnsureGameplayVisible();
    }

    private static void EnsureGameplayVisible()
    {
        var cam = Camera.main;
        if (cam != null && !cam.enabled)
            cam.enabled = true;
    }

    private void CloseKeyRing()
    {
        SetOverlay(ringGroup, false);
        if (!ringOpenedFromLock || activeLockDef == null) return;

        ringOpenedFromLock = false;
        RefreshLockState();
        SetOverlay(lockGroup, true);
    }

    private static RectTransform BuildMapProgressBar(Transform parent)
    {
        var trackGo = new GameObject("ProgressTrack", typeof(RectTransform), typeof(Image));
        trackGo.transform.SetParent(parent, false);
        var trackRect = trackGo.GetComponent<RectTransform>();
        trackRect.anchorMin = new Vector2(0.08f, 0.125f);
        trackRect.anchorMax = new Vector2(0.92f, 0.125f);
        trackRect.pivot = new Vector2(0.5f, 0.5f);
        trackRect.sizeDelta = new Vector2(0f, 6f);
        trackGo.GetComponent<Image>().color = new Color(0.18f, 0.19f, 0.24f, 1f);

        var fillGo = new GameObject("ProgressFill", typeof(RectTransform), typeof(Image));
        fillGo.transform.SetParent(trackGo.transform, false);
        var fillRect = fillGo.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(0.08f, 1f);
        fillRect.offsetMin = fillRect.offsetMax = Vector2.zero;
        fillGo.GetComponent<Image>().color = LockeKeyUITheme.LKGold;
        return fillRect;
    }

    private static void SetOverlay(CanvasGroup cg, bool visible)
    {
        cg.alpha = visible ? 1f : 0f;
        cg.interactable = visible;
        cg.blocksRaycasts = visible;
    }

    private static GameObject MakeFullPanel(Transform parent, string name, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        StretchFull(go.GetComponent<RectTransform>());
        go.GetComponent<Image>().color = color;
        return go;
    }

    private static void StretchFull(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    private static void AddLabel(Transform parent, string name, Font font,
        int size, Color color, Vector2 anchorCenter,
        string text, float width, float height, FontStyle style)
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
        t.fontStyle = style;
        t.color = color;
        t.text = text;
        t.alignment = TextAnchor.MiddleCenter;
        t.resizeTextForBestFit = false;
    }

    private static Button AddButton(Transform parent, string label, Font font,
        Vector2 anchorCenter, Action onClick,
        Color bgColor, Color textColor, float width = 260f, float height = 58f)
    {
        var go = new GameObject(label.Replace(" ", "").Replace("✕", "Close") + "Btn",
            typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = anchorCenter;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(width, height);
        go.GetComponent<Image>().color = bgColor;
        var btn = go.GetComponent<Button>();
        if (onClick != null) btn.onClick.AddListener(() => onClick());

        var tGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
        tGo.transform.SetParent(go.transform, false);
        var tRect = tGo.GetComponent<RectTransform>();
        tRect.anchorMin = Vector2.zero; tRect.anchorMax = Vector2.one;
        tRect.offsetMin = tRect.offsetMax = Vector2.zero;
        var t = tGo.GetComponent<Text>();
        t.font = font;
        t.fontSize = 20;
        t.fontStyle = FontStyle.Bold;
        t.color = textColor;
        t.text = label;
        t.alignment = TextAnchor.MiddleCenter;
        return btn;
    }

    private static string KeyDescription(string keyId) => keyId switch
    {
        "anywhere"   => "Opens a door between any two points.\nUse near a wall to create a passage.",
        "head"       => "Unlocks the mind. Step inside a memory\nand witness what was hidden.",
        "mending"    => "Repairs broken objects and broken people.\nHandle with care.",
        "omega"      => "The last key. Its true purpose\nis not yet known.",
        "ghost"      => "Allows the bearer to pass through\nsolid matter for a short time.",
        "shadow"     => "Grants command over shadows.\nDarkness does as you ask.",
        "echo"       => "Calls an echo of something past.\nNot always something safe.",
        "matchstick" => "Burns without consuming the bearer.\nLight in total darkness.",
        "mirror"     => "Travel through any reflective surface.\nWatch what looks back.",
        "music_box"  => "Plays a song that stops time\nfor those who hear it.",
        "animal"     => "Transforms the bearer into a creature\nof their choosing.",
        "identity"   => "Reshapes who you are.\nThe self is more malleable than bone.",
        "alpha"      => "The first. The origin.\nAll other keys descend from this one.",
        _            => "A key of unknown purpose."
    };
}
