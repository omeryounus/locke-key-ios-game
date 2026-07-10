using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Circular glass button feel: press scale, hover glow, soft shadow, haptics.
/// </summary>
[DisallowMultipleComponent]
public class UIButtonFeedback : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerEnterHandler, IPointerClickHandler
{
    [SerializeField] private float pressedScale = 0.88f;
    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private float animSpeed = 16f;
    [SerializeField] private bool hapticOnPress = true;

    private RectTransform rect;
    private Graphic graphic;
    private Image glow;
    private Vector3 baseScale = Vector3.one;
    private Color baseColor = Color.white;
    private bool pressed;
    private bool hovered;
    private Coroutine pulseRoutine;

    private void Awake()
    {
        rect = transform as RectTransform;
        graphic = GetComponent<Graphic>();
        baseScale = transform.localScale;
        if (graphic != null)
            baseColor = graphic.color;
        EnsureGlassChrome();
    }

    private void EnsureGlassChrome()
    {
        // Soft drop shadow
        var shadow = GetComponent<Shadow>() ?? gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.5f);
        shadow.effectDistance = new Vector2(0f, -3.5f);

        // Outer glow ring (hover)
        if (transform.Find("HoverGlow") == null)
        {
            var go = new GameObject("HoverGlow", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);
            go.transform.SetAsFirstSibling();
            glow = go.GetComponent<Image>();
            glow.color = new Color(GameSettings.AccentColor.r, GameSettings.AccentColor.g, GameSettings.AccentColor.b, 0f);
            glow.raycastTarget = false;
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = new Vector2(-4f, -4f);
            r.offsetMax = new Vector2(4f, 4f);
        }
        else
            glow = transform.Find("HoverGlow").GetComponent<Image>();

        // Circular glass fill if Image present
        if (graphic is Image img)
        {
            // Prefer semi-transparent dark glass
            if (img.color.a > 0.85f)
                img.color = new Color(0.08f, 0.09f, 0.13f, 0.55f);
            baseColor = img.color;
        }
    }

    private void OnEnable()
    {
        pressed = false;
        hovered = false;
        transform.localScale = baseScale;
        if (graphic != null) graphic.color = baseColor;
    }

    private void Update()
    {
        if (rect == null) return;
        float targetMul = pressed ? pressedScale : (hovered ? hoverScale : 1f);
        transform.localScale = Vector3.Lerp(transform.localScale, baseScale * targetMul, Time.unscaledDeltaTime * animSpeed);

        if (glow != null)
        {
            float a = pressed ? 0.35f : (hovered ? 0.28f : 0.08f);
            var c = GameSettings.AccentColor;
            glow.color = Color.Lerp(glow.color, new Color(c.r, c.g, c.b, a), Time.unscaledDeltaTime * 10f);
        }
    }

    public void OnPointerEnter(PointerEventData eventData) => hovered = true;

    public void OnPointerDown(PointerEventData eventData)
    {
        pressed = true;
        if (graphic != null)
            graphic.color = new Color(baseColor.r * 0.85f, baseColor.g * 0.85f, baseColor.b * 0.85f, baseColor.a);
        if (hapticOnPress)
            GameHaptics.TriggerHapticLight();
    }

    public void OnPointerUp(PointerEventData eventData) => Release();
    public void OnPointerExit(PointerEventData eventData)
    {
        hovered = false;
        Release();
    }

    public void OnPointerClick(PointerEventData eventData) { }

    private void Release()
    {
        if (!pressed) return;
        pressed = false;
        if (graphic != null)
            graphic.color = baseColor;
    }

    public void PulseHighlight(float intensity = 1.12f, float duration = 0.18f)
    {
        if (pulseRoutine != null) StopCoroutine(pulseRoutine);
        pulseRoutine = StartCoroutine(PulseRoutine(intensity, duration));
    }

    private IEnumerator PulseRoutine(float intensity, float duration)
    {
        var t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            var k = Mathf.Sin((t / duration) * Mathf.PI);
            transform.localScale = baseScale * Mathf.Lerp(1f, intensity, k);
            yield return null;
        }
        pulseRoutine = null;
    }

    public static UIButtonFeedback Ensure(GameObject go)
    {
        if (go == null) return null;
        var fb = go.GetComponent<UIButtonFeedback>();
        if (fb == null)
            fb = go.AddComponent<UIButtonFeedback>();
        return fb;
    }
}
