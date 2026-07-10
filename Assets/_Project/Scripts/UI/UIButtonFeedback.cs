using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Production touch feedback: press scale, color tint, optional haptic.
/// Attach to any button / hold control for consistent mobile feel.
/// </summary>
[DisallowMultipleComponent]
public class UIButtonFeedback : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private float pressedScale = 0.92f;
    [SerializeField] private float animSpeed = 18f;
    [SerializeField] private float pressedDim = 0.82f;
    [SerializeField] private bool hapticOnPress = true;
    [SerializeField] private bool hapticOnClick = false;

    private RectTransform rect;
    private Graphic graphic;
    private Vector3 baseScale = Vector3.one;
    private Color baseColor = Color.white;
    private bool pressed;
    private float scaleVel;
    private Coroutine restoreRoutine;

    private void Awake()
    {
        rect = transform as RectTransform;
        graphic = GetComponent<Graphic>();
        baseScale = transform.localScale;
        if (graphic != null)
            baseColor = graphic.color;
    }

    private void OnEnable()
    {
        pressed = false;
        transform.localScale = baseScale;
        if (graphic != null)
            graphic.color = baseColor;
    }

    private void OnDisable()
    {
        pressed = false;
        transform.localScale = baseScale;
        if (graphic != null)
            graphic.color = baseColor;
    }

    private void Update()
    {
        if (rect == null) return;
        var target = pressed ? baseScale * pressedScale : baseScale;
        transform.localScale = Vector3.Lerp(transform.localScale, target, Time.unscaledDeltaTime * animSpeed);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pressed = true;
        if (graphic != null)
            graphic.color = new Color(baseColor.r * pressedDim, baseColor.g * pressedDim,
                baseColor.b * pressedDim, baseColor.a);

        if (hapticOnPress)
            GameHaptics.TriggerHapticLight();
    }

    public void OnPointerUp(PointerEventData eventData) => Release();

    public void OnPointerExit(PointerEventData eventData) => Release();

    public void OnPointerClick(PointerEventData eventData)
    {
        if (hapticOnClick)
            GameHaptics.TriggerHapticLight();
    }

    private void Release()
    {
        if (!pressed) return;
        pressed = false;
        if (graphic != null)
            graphic.color = baseColor;
    }

    /// <summary>One-shot highlight pulse (e.g. interactable in range).</summary>
    public void PulseHighlight(float intensity = 1.12f, float duration = 0.18f)
    {
        if (restoreRoutine != null)
            StopCoroutine(restoreRoutine);
        restoreRoutine = StartCoroutine(PulseRoutine(intensity, duration));
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

        transform.localScale = pressed ? baseScale * pressedScale : baseScale;
        restoreRoutine = null;
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
