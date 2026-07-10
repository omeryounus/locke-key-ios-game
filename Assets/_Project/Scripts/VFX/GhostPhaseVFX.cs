using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Screen vignette + player transparency during Ghost Key phase.
/// </summary>
public class GhostPhaseVFX : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private SpriteRenderer playerRenderer;
    [SerializeField] private Color ghostTint = new(0.35f, 0.95f, 0.65f, 0.5f); // green spirit
    [SerializeField] private float fadeSpeed = 5f;

    private EventBus eventBus;
    private Image vignette;
    private CanvasGroup vignetteGroup;
    private Color normalPlayerColor = Color.white;
    private bool active;
    private Coroutine fadeRoutine;

    private void Awake()
    {
        if (player == null)
            player = FindFirstObjectByType<PlayerController>();
        if (playerRenderer == null && player != null)
            playerRenderer = player.GetComponent<SpriteRenderer>();

        if (playerRenderer != null)
            normalPlayerColor = playerRenderer.color;

        eventBus = Resources.Load<EventBus>("EventBus");
        if (eventBus != null)
        {
            eventBus.OnGhostPhaseStarted += EnableVfx;
            eventBus.OnGhostPhaseEnded += DisableVfx;
        }

        BuildVignette();
        SetVignetteAlpha(0f);
    }

    private void OnDestroy()
    {
        if (eventBus == null) return;
        eventBus.OnGhostPhaseStarted -= EnableVfx;
        eventBus.OnGhostPhaseEnded -= DisableVfx;
    }

    private void EnableVfx()
    {
        active = true;
        if (vignette != null)
            vignette.gameObject.SetActive(true);

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeVignette(0.62f));
    }

    private void DisableVfx()
    {
        active = false;
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeOutAndHide());

        // Let PlayerSpriteAnimator own final color if present.
        if (playerRenderer != null && (player == null || player.GetComponent<PlayerSpriteAnimator>() == null))
            playerRenderer.color = normalPlayerColor;
    }

    private void Update()
    {
        if (!active || playerRenderer == null) return;
        // Soft ethereal flicker (animator also tints; this reinforces if animator missing)
        if (player != null && player.GetComponent<PlayerSpriteAnimator>() != null) return;
        var flicker = 0.48f + Mathf.Sin(Time.time * 8f) * 0.12f;
        playerRenderer.color = new Color(ghostTint.r, ghostTint.g, ghostTint.b, flicker);
    }

    private IEnumerator FadeVignette(float target)
    {
        var start = vignetteGroup != null ? vignetteGroup.alpha : 0f;
        var t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * fadeSpeed;
            SetVignetteAlpha(Mathf.Lerp(start, target, Mathf.SmoothStep(0f, 1f, t)));
            yield return null;
        }

        SetVignetteAlpha(target);
        fadeRoutine = null;
    }

    private IEnumerator FadeOutAndHide()
    {
        yield return FadeVignette(0f);
        if (vignette != null)
            vignette.gameObject.SetActive(false);
    }

    private void SetVignetteAlpha(float a)
    {
        if (vignetteGroup != null)
            vignetteGroup.alpha = a;
        else if (vignette != null)
        {
            var c = vignette.color;
            vignette.color = new Color(c.r, c.g, c.b, a);
        }
    }

    private void BuildVignette()
    {
        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        var go = new GameObject("GhostPhaseVignette", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        go.transform.SetParent(canvas.transform, false);
        go.transform.SetAsLastSibling();
        vignette = go.GetComponent<Image>();
        vignette.color = new Color(0.02f, 0.05f, 0.1f, 1f);
        vignette.raycastTarget = false;
        vignetteGroup = go.GetComponent<CanvasGroup>();
        vignetteGroup.blocksRaycasts = false;
        vignetteGroup.interactable = false;
        vignetteGroup.alpha = 0f;

        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        go.SetActive(false);
    }
}
