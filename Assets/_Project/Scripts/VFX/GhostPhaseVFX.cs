using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Screen-edge desaturation and player transparency during Ghost Key phase (Beat 4).
/// </summary>
public class GhostPhaseVFX : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private SpriteRenderer playerRenderer;
    [SerializeField] private Color ghostTint = new(0.65f, 0.85f, 1f, 0.45f);

    private EventBus eventBus;
    private Image vignette;
    private Color normalPlayerColor = Color.white;

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
        DisableVfx();
    }

    private void OnDestroy()
    {
        if (eventBus == null) return;
        eventBus.OnGhostPhaseStarted -= EnableVfx;
        eventBus.OnGhostPhaseEnded -= DisableVfx;
    }

    private void EnableVfx()
    {
        if (vignette != null)
            vignette.gameObject.SetActive(true);

        if (playerRenderer != null)
            playerRenderer.color = ghostTint;
    }

    private void DisableVfx()
    {
        if (vignette != null)
            vignette.gameObject.SetActive(false);

        if (playerRenderer != null)
            playerRenderer.color = normalPlayerColor;
    }

    private void BuildVignette()
    {
        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        var go = new GameObject("GhostPhaseVignette", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(canvas.transform, false);
        vignette = go.GetComponent<Image>();
        vignette.color = new Color(0.02f, 0.04f, 0.08f, 0.55f);
        vignette.raycastTarget = false;

        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        go.SetActive(false);
    }
}