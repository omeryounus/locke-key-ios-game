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

        var rootRect = rootGo.GetComponent<RectTransform>();
        Stretch(rootRect);

        // Letterbox fill
        var letterbox = new GameObject("Letterbox",
            typeof(RectTransform), typeof(Image));
        letterbox.transform.SetParent(rootGo.transform, false);
        Stretch(letterbox.GetComponent<RectTransform>());
        letterbox.GetComponent<Image>().color = LockeKeyUITheme.LKInk;
        letterbox.GetComponent<Image>().raycastTarget = false;

        // Centered portrait viewport
        var viewportGo = new GameObject("Viewport", typeof(RectTransform));
        viewportGo.transform.SetParent(rootGo.transform, false);
        var viewport = viewportGo.GetComponent<RectTransform>();
        viewport.anchorMin = viewport.anchorMax = new Vector2(0.5f, 0.5f);
        viewport.pivot = new Vector2(0.5f, 0.5f);
        viewportGo.AddComponent<LockeViewportFitter>();

        var contentGo = new GameObject("Content",
            typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        contentGo.transform.SetParent(viewport, false);
        Stretch(contentGo.GetComponent<RectTransform>());

        var contentCanvas = contentGo.GetComponent<Canvas>();
        contentCanvas.overrideSorting = true;
        contentCanvas.sortingOrder = sortingOrder;

        var scaler = contentGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(LockeKeyUITheme.RefWidth, LockeKeyUITheme.RefHeight);
        scaler.matchWidthOrHeight = 1f;

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
}

/// <summary>
/// Keeps viewport at 393:852 aspect, centered on screen (landscape letterbox).
/// </summary>
public class LockeViewportFitter : MonoBehaviour
{
    private RectTransform rect;
    private Vector2Int lastScreen;

    private void Awake() => rect = GetComponent<RectTransform>();

    private void Update()
    {
        if (Screen.width == lastScreen.x && Screen.height == lastScreen.y) return;
        lastScreen = new Vector2Int(Screen.width, Screen.height);
        Apply();
    }

    private void Apply()
    {
        float targetAspect = LockeKeyUITheme.RefWidth / LockeKeyUITheme.RefHeight;
        float screenAspect = (float)Screen.width / Screen.height;

        float w, h;
        if (screenAspect > targetAspect)
        {
            h = Screen.height;
            w = h * targetAspect;
        }
        else
        {
            w = Screen.width;
            h = w / targetAspect;
        }

        rect.sizeDelta = new Vector2(w, h);
    }
}