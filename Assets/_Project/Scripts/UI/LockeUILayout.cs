using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Portrait viewport (393×852) centered in landscape letterbox — matches ux_landscape_device_frame.
/// Uses a single root overlay canvas (no nested canvases — avoids iOS/URP render failures).
/// Letterbox bars only cover the side/top/bottom margins so the viewport center stays transparent
/// and the game camera remains visible during gameplay.
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
        rootCanvas.vertexColorAlwaysGammaSpace = true;

        var rootRect = rootGo.GetComponent<RectTransform>();
        Stretch(rootRect);

        var leftBar = CreateLetterboxBar(rootGo.transform, "LetterboxLeft");
        var rightBar = CreateLetterboxBar(rootGo.transform, "LetterboxRight");
        var topBar = CreateLetterboxBar(rootGo.transform, "LetterboxTop");
        var bottomBar = CreateLetterboxBar(rootGo.transform, "LetterboxBottom");

        var viewportGo = new GameObject("Viewport", typeof(RectTransform));
        viewportGo.transform.SetParent(rootGo.transform, false);
        var viewport = viewportGo.GetComponent<RectTransform>();
        viewport.anchorMin = viewport.anchorMax = new Vector2(0.5f, 0.5f);
        viewport.pivot = new Vector2(0.5f, 0.5f);
        ApplyViewportSize(viewport);

        var fitter = viewportGo.AddComponent<LockeViewportFitter>();
        fitter.Configure(leftBar, rightBar, topBar, bottomBar);

        var contentGo = new GameObject("Content", typeof(RectTransform));
        contentGo.transform.SetParent(viewport, false);
        var contentRect = contentGo.GetComponent<RectTransform>();
        contentRect.anchorMin = contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.pivot = new Vector2(0.5f, 0.5f);
        contentRect.sizeDelta = new Vector2(LockeKeyUITheme.RefWidth, LockeKeyUITheme.RefHeight);
        SyncContentScale(viewport, contentRect);
        ApplyLetterboxBars(viewport, leftBar, rightBar, topBar, bottomBar);

        LayoutRebuilder.ForceRebuildLayoutImmediate(viewport);
        Canvas.ForceUpdateCanvases();

        return new FlowCanvas
        {
            Canvas = rootCanvas,
            Viewport = viewport,
            Font = GetUIFont()
        };
    }

    private static RectTransform CreateLetterboxBar(Transform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = LockeKeyUITheme.LKInk;
        go.GetComponent<Image>().raycastTarget = false;
        return go.GetComponent<RectTransform>();
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

    public static void ApplyLetterboxBars(RectTransform viewport,
        RectTransform left, RectTransform right, RectTransform top, RectTransform bottom)
    {
        if (viewport == null) return;

        var vpSize = viewport.sizeDelta;
        if (vpSize.x <= 1f || vpSize.y <= 1f)
            vpSize = ComputeViewportSize(Screen.width, Screen.height);

        float halfW = vpSize.x * 0.5f;
        float halfH = vpSize.y * 0.5f;
        float screenAspect = (float)Screen.width / Mathf.Max(1, Screen.height);
        float targetAspect = LockeKeyUITheme.RefWidth / LockeKeyUITheme.RefHeight;
        bool sideBars = screenAspect > targetAspect;

        if (left != null)
        {
            left.gameObject.SetActive(sideBars);
            if (sideBars)
            {
                left.anchorMin = Vector2.zero;
                left.anchorMax = new Vector2(0.5f, 1f);
                left.offsetMin = Vector2.zero;
                left.offsetMax = new Vector2(-halfW, 0f);
            }
        }

        if (right != null)
        {
            right.gameObject.SetActive(sideBars);
            if (sideBars)
            {
                right.anchorMin = new Vector2(0.5f, 0f);
                right.anchorMax = Vector2.one;
                right.offsetMin = new Vector2(halfW, 0f);
                right.offsetMax = Vector2.zero;
            }
        }

        if (top != null)
        {
            top.gameObject.SetActive(!sideBars);
            if (!sideBars)
            {
                top.anchorMin = new Vector2(0f, 0.5f);
                top.anchorMax = Vector2.one;
                top.offsetMin = new Vector2(0f, halfH);
                top.offsetMax = Vector2.zero;
            }
        }

        if (bottom != null)
        {
            bottom.gameObject.SetActive(!sideBars);
            if (!sideBars)
            {
                bottom.anchorMin = Vector2.zero;
                bottom.anchorMax = new Vector2(1f, 0.5f);
                bottom.offsetMin = Vector2.zero;
                bottom.offsetMax = new Vector2(0f, -halfH);
            }
        }
    }

    public static void SyncContentScale(RectTransform viewport, RectTransform content)
    {
        if (viewport == null || content == null) return;

        var vpSize = viewport.sizeDelta;
        if (vpSize.y <= 1f)
            vpSize = ComputeViewportSize(Screen.width, Screen.height);

        float scale = vpSize.y / LockeKeyUITheme.RefHeight;
        content.localScale = Vector3.one * Mathf.Max(scale, 0.01f);
    }

    public static Transform GetContentRoot(FlowCanvas flow)
    {
        var content = flow.Viewport != null ? flow.Viewport.Find("Content") : null;
        return content != null ? content : flow.Viewport;
    }

    public static Font GetUIFont()
    {
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return font;
    }
}

/// <summary>
/// Keeps viewport at 393:852 aspect, centered on screen (landscape letterbox).
/// </summary>
public class LockeViewportFitter : MonoBehaviour
{
    private RectTransform rect;
    private RectTransform letterboxLeft;
    private RectTransform letterboxRight;
    private RectTransform letterboxTop;
    private RectTransform letterboxBottom;
    private Vector2Int lastScreen;

    public void Configure(RectTransform left, RectTransform right, RectTransform top, RectTransform bottom)
    {
        letterboxLeft = left;
        letterboxRight = right;
        letterboxTop = top;
        letterboxBottom = bottom;
    }

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        Apply();
    }

    private void OnEnable() => Apply();

    private void Start() => Apply();

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

        var content = rect.Find("Content") as RectTransform;
        if (content != null)
            LockeUILayout.SyncContentScale(rect, content);

        LockeUILayout.ApplyLetterboxBars(rect, letterboxLeft, letterboxRight, letterboxTop, letterboxBottom);

        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        Canvas.ForceUpdateCanvases();
    }
}