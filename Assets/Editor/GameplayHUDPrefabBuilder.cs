using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Builds the authored GameplayHUD prefab with safe-area support.
/// </summary>
public static class GameplayHUDPrefabBuilder
{
    private const string PrefabPath = "Assets/_Project/Prefabs/UI/GameplayHUD.prefab";
    private const string ResourcesPrefabPath = "Assets/_Project/Resources/UI/GameplayHUD.prefab";

    [MenuItem("LockeKey/Build/Gameplay HUD Prefab")]
    public static void BuildFromMenu()
    {
        BuildPrefab();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Gameplay HUD prefab written to {PrefabPath}");
    }

    /// <summary>
    /// Entry point for: Unity -batchmode -executeMethod GameplayHUDPrefabBuilder.BuildPrefabBatch
    /// </summary>
    public static void BuildPrefabBatch()
    {
        BuildPrefab();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        if (Application.isBatchMode)
            EditorApplication.Exit(0);
    }

    public static void BuildPrefab()
    {
        var root = new GameObject("GameplayHUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler),
            typeof(GraphicRaycaster), typeof(SafeAreaFitter), typeof(GameplayHUDBindings));

        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        var bindings = root.GetComponent<GameplayHUDBindings>();
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var panelColor = new Color(0.05f, 0.06f, 0.1f, 0.72f);
        var buttonColor = new Color(0.14f, 0.16f, 0.24f, 0.92f);
        var accentColor = new Color(0.55f, 0.75f, 0.95f, 1f);
        var iconLibrary = UIIconLibrary.LoadDefault();

        bindings.keyStatusIcon = CreateStatusIcon(root.transform, "KeyStatusIcon", new Vector2(24f, -20f), 40f);
        var keySlotGo = new GameObject("KeySlot", typeof(RectTransform), typeof(Image), typeof(KeySlotHUD));
        keySlotGo.transform.SetParent(root.transform, false);
        var keySlotRect = keySlotGo.GetComponent<RectTransform>();
        keySlotRect.anchorMin = new Vector2(0f, 1f);
        keySlotRect.anchorMax = new Vector2(0f, 1f);
        keySlotRect.pivot = new Vector2(0f, 1f);
        keySlotRect.anchoredPosition = new Vector2(180f, -12f);
        keySlotRect.sizeDelta = new Vector2(72f, 72f);
        bindings.keySlotImage = keySlotGo.GetComponent<Image>();
        bindings.keySlotImage.preserveAspect = true;
        bindings.keySlotHud = keySlotGo.GetComponent<KeySlotHUD>();

        bindings.keyStatusText = CreateText(root.transform, "KeyStatus", font, 24, TextAnchor.UpperLeft,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(260f, -24f), new Vector2(860f, 36f), accentColor);
        bindings.houseKeyIcon = CreateStatusIcon(root.transform, "HouseKeyIcon", new Vector2(24f, -60f), 32f);
        bindings.houseKeyText = CreateText(root.transform, "HouseKeyStatus", font, 20, TextAnchor.UpperLeft,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(64f, -64f), new Vector2(860f, 32f), Color.white);
        bindings.hintText = CreateText(root.transform, "Hint", font, 22, TextAnchor.UpperLeft,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -104f), new Vector2(1200f, 72f),
            new Color(0.85f, 0.82f, 0.75f, 1f));
        bindings.toastText = CreateText(root.transform, "Toast", font, 24, TextAnchor.UpperCenter,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -160f), new Vector2(1100f, 48f),
            new Color(1f, 0.85f, 0.55f, 1f));
        bindings.toastText.gameObject.SetActive(false);

        var controlBar = CreatePanel(root.transform, "ControlBar", panelColor,
            new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, 140f));

        bindings.leftButton = CreateHoldButton(controlBar.transform, "Left", iconLibrary?.moveLeft, font, buttonColor, accentColor,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(24f, 20f), new Vector2(180f, 100f));
        bindings.rightButton = CreateHoldButton(controlBar.transform, "Right", iconLibrary?.moveRight, font, buttonColor, accentColor,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(220f, 20f), new Vector2(180f, 100f));
        bindings.jumpButton = CreateTapButton(controlBar.transform, "Jump", iconLibrary?.jump, font, buttonColor, accentColor,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(416f, 20f), new Vector2(180f, 100f));
        bindings.interactButton = CreateTapButton(controlBar.transform, "Interact", iconLibrary?.interact, font, buttonColor, accentColor,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-416f, 20f), new Vector2(180f, 100f));
        bindings.useKeyButton = CreateTapButton(controlBar.transform, "Use Key", iconLibrary?.useKey, font, buttonColor, accentColor,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-216f, 20f), new Vector2(180f, 100f));

        bindings.memoryOverlay = BuildMemoryOverlay(root.transform, font, panelColor, accentColor, out bindings.memoryPanelImage,
            out bindings.memoryBodyText);
        bindings.memoryOverlay.SetActive(false);

        EnsureFolder("Assets/_Project/Prefabs/UI");
        EnsureFolder("Assets/_Project/Resources/UI");

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        PrefabUtility.SaveAsPrefabAsset(root, ResourcesPrefabPath);
        Object.DestroyImmediate(root);
    }

    private static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            var parent = System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/');
            var leaf = System.IO.Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
                EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }

    private static GameObject BuildMemoryOverlay(Transform parent, Font font, Color panelColor, Color accentColor,
        out Image panelImage, out Text bodyText)
    {
        var overlay = CreatePanel(parent, "MemoryOverlay", new Color(0.02f, 0.02f, 0.05f, 0.82f),
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        overlay.SetActive(false);

        var panel = CreatePanel(overlay.transform, "MemoryPanel", panelColor,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-520f, -280f), new Vector2(1040f, 560f));

        var panelImageGo = new GameObject("MemoryPanelArt", typeof(RectTransform), typeof(Image));
        panelImageGo.transform.SetParent(panel.transform, false);
        panelImage = panelImageGo.GetComponent<Image>();
        panelImage.preserveAspect = true;
        panelImage.color = Color.white;
        var panelImageRect = panelImageGo.GetComponent<RectTransform>();
        panelImageRect.anchorMin = Vector2.zero;
        panelImageRect.anchorMax = Vector2.one;
        panelImageRect.offsetMin = Vector2.zero;
        panelImageRect.offsetMax = Vector2.zero;

        CreateText(panel.transform, "Title", font, 34, TextAnchor.UpperCenter,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -36f), new Vector2(960f, 48f), accentColor);
        bodyText = CreateText(panel.transform, "Body", font, 24, TextAnchor.UpperLeft,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(36f, -110f), new Vector2(960f, 340f),
            new Color(0.92f, 0.9f, 0.86f, 1f));

        CreateTapButton(panel.transform, "Close", null, font, new Color(0.2f, 0.22f, 0.32f, 1f), accentColor,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-120f, 28f), new Vector2(240f, 64f));

        return overlay;
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
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
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
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        var go = new GameObject(label + "Button", typeof(RectTransform), typeof(Image), typeof(HoldButton));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.color = icon != null ? Color.white : bg;
        if (icon != null)
            image.sprite = icon;

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