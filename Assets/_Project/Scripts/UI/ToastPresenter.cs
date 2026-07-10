using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Animated toast with fade + slide for production HUD messaging.
/// </summary>
public class ToastPresenter : MonoBehaviour
{
    private Text toastText;
    private CanvasGroup group;
    private RectTransform rect;
    private Coroutine routine;
    private Vector2 restPos;
    private float slideUp = 18f;

    public void Bind(Text text, CanvasGroup canvasGroup = null)
    {
        toastText = text;
        if (text == null) return;

        rect = text.GetComponent<RectTransform>();
        if (rect != null)
            restPos = rect.anchoredPosition;

        group = canvasGroup;
        if (group == null)
        {
            group = text.GetComponent<CanvasGroup>();
            if (group == null)
                group = text.gameObject.AddComponent<CanvasGroup>();
        }

        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;
        text.gameObject.SetActive(true);
    }

    public void Show(string message, float duration = 3f)
    {
        if (toastText == null || string.IsNullOrEmpty(message)) return;

        toastText.text = message;
        toastText.gameObject.SetActive(true);

        if (routine != null)
            StopCoroutine(routine);
        routine = StartCoroutine(ShowRoutine(duration));
    }

    private IEnumerator ShowRoutine(float duration)
    {
        if (group != null) group.alpha = 0f;
        if (rect != null)
            rect.anchoredPosition = restPos + Vector2.down * slideUp;

        var fadeIn = 0.22f;
        var t = 0f;
        while (t < fadeIn)
        {
            t += Time.unscaledDeltaTime;
            var k = Mathf.SmoothStep(0f, 1f, t / fadeIn);
            if (group != null) group.alpha = k;
            if (rect != null)
                rect.anchoredPosition = Vector2.Lerp(restPos + Vector2.down * slideUp, restPos, k);
            yield return null;
        }

        if (group != null) group.alpha = 1f;
        if (rect != null) rect.anchoredPosition = restPos;

        yield return new WaitForSecondsRealtime(Mathf.Max(0.4f, duration - fadeIn * 2f));

        t = 0f;
        var fadeOut = 0.28f;
        while (t < fadeOut)
        {
            t += Time.unscaledDeltaTime;
            var k = Mathf.SmoothStep(1f, 0f, t / fadeOut);
            if (group != null) group.alpha = k;
            if (rect != null)
                rect.anchoredPosition = Vector2.Lerp(restPos, restPos + Vector2.up * (slideUp * 0.35f), 1f - k);
            yield return null;
        }

        if (group != null) group.alpha = 0f;
        routine = null;
    }
}
