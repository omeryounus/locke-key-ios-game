using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Portrait viewport (393×852) centered in landscape letterbox — matches ux_landscape_device_frame.
/// </summary>
public static class LockeUILayout
{
    public struct FlowCanvas
    {
        public Canvas Canvas;
        public RectTransform Viewport;
        public Font Font;
    }

    public static FlowCanvas CreateFlowCanvas(string name, int sortingOrder)
    {
        var rootGo = new GameObject(name,
            typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
        var rootCanvas = rootGo.GetComponent<Canvas>();
        rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        rootCanvas.sortingOrder = sortingOrder;
        rootCanvas.pixelPerfect = false;

        var rootRect = rootGo.GetComponent<RectTransform>();
        Stretch(rootRect);

        // Letterbox fill
        var letterbox = new GameObject("Letterbox",
            typeof(RectTransform), typeof(Image));
        letterbox.transform.SetParent(rootGo.transform, false);
        Stretch(letterbox.GetComponent<RectTransform>());
        letterbox.GetComponent<Image>().color = LockeKeyUITheme.LKInk;
        letterbox.GetComponent<Image>().raycastTarget = false;

        // Centered portrait viewport — sized before any UI is parented (avoids 0×0 on iOS startup).
        var viewportGo = new GameObject("Viewport", typeof(RectTransform));
        viewportGo.transform.SetParent(rootGo.transform, false);
        var viewport = viewportGo.GetComponent<RectTransform>();
        viewport.anchorMin = viewport.anchorMax = new Vector2(0.5f, 0.5f);
        viewport.pivot = new Vector2(0.5f, 0.5f);
        ApplyViewportSize(viewport);
        viewportGo.AddComponent<LockeViewportFitter>();

        var contentGo = new GameObject("Content",
            typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        contentGo.transform.SetParent(viewport, false);
        Stretch(contentGo.GetComponent<RectTransform>());

        var contentCanvas = contentGo.GetComponent<Canvas>();
        contentCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        contentCanvas.overrideSorting = true;
        contentCanvas.sortingOrder = sortingOrder;
        contentCanvas.pixelPerfect = false;

        var scaler = contentGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(LockeKeyUITheme.RefWidth, LockeKeyUITheme.RefHeight);
        scaler.matchWidthOrHeight = 1f;

        LayoutRebuilder.ForceRebuildLayoutImmediate(viewport);
        Canvas.ForceUpdateCanvases();

        return new FlowCanvas
        {
            Canvas = contentCanvas,
            Viewport = viewport,
            Font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
        };
    }

    public static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    public static Vector2 ComputeViewportSize(int screenWidth, int screenHeight)
    {
        if (screenWidth <= 0 || screenHeight <= 0)
        {
            screenWidth = Mathf.Max(screenWidth, (int)LockeKeyUITheme.RefWidth);
            screenHeight = Mathf.Max(screenHeight, (int)LockeKeyUITheme.RefHeight);
        }

        float targetAspect = LockeKeyUITheme.RefWidth / LockeKeyUITheme.RefHeight;
        float screenAspect = (float)screenWidth / screenHeight;

        if (screenAspect > targetAspect)
        {
            float h = screenHeight;
            return new Vector2(h * targetAspect, h);
        }

        float w = screenWidth;
        return new Vector2(w, w / targetAspect);
    }

    public static void ApplyViewportSize(RectTransform viewport)
    {
        if (viewport == null) return;
        viewport.sizeDelta = ComputeViewportSize(Screen.width, Screen.height);
    }

    public static Transform GetContentRoot(FlowCanvas flow)
    {
        var content = flow.Viewport != null ? flow.Viewport.Find("Content") : null;
        return content != null ? content : flow.Viewport;
    }
}

/// <summary>
/// Keeps viewport at 393:852 aspect, centered on screen (landscape letterbox).
/// </summary>
public class LockeViewportFitter : MonoBehaviour
{
    private RectTransform rect;
    private Vector2Int lastScreen;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        Apply();
    }

    private void OnEnable() => Apply();

    private void OnRectTransformDimensionsChange() => Apply();

    private void LateUpdate()
    {
        var size = new Vector2Int(Screen.width, Screen.height);
        if (size.x == lastScreen.x && size.y == lastScreen.y) return;
        lastScreen = size;
        Apply();
    }

    private void Apply()
    {
        if (rect == null) return;
        LockeUILayout.ApplyViewportSize(rect);
        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
    }
}