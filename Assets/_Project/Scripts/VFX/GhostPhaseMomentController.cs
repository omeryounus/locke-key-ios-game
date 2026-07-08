using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Signature Ghost Key beat: desaturation, muffled audio, translucency, door shimmer, whisper.
/// </summary>
public class GhostPhaseMomentController : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private SpriteRenderer playerRenderer;
    [SerializeField] private SealedDoorPuzzle sealedDoor;
    [SerializeField] private Color ghostTint = new(0.72f, 0.9f, 1f, 0.38f);

    private EventBus eventBus;
    private GameAudioController audio;
    private Image desaturateOverlay;
    private Image shimmerOverlay;
    private Color normalPlayerColor = Color.white;
    private Coroutine shimmerRoutine;

    private void Awake()
    {
        if (player == null)
            player = FindFirstObjectByType<PlayerController>();
        if (playerRenderer == null && player != null)
            playerRenderer = player.GetComponent<SpriteRenderer>();
        if (sealedDoor == null)
            sealedDoor = FindFirstObjectByType<SealedDoorPuzzle>();

        if (playerRenderer != null)
            normalPlayerColor = playerRenderer.color;

        audio = FindFirstObjectByType<GameAudioController>();
        eventBus = Resources.Load<EventBus>("EventBus");

        if (eventBus != null)
        {
            eventBus.OnGhostPhaseStarted += BeginMoment;
            eventBus.OnGhostPhaseEnded += EndMoment;
        }

        BuildOverlays();
        HideOverlays();
    }

    private void OnDestroy()
    {
        if (eventBus == null) return;
        eventBus.OnGhostPhaseStarted -= BeginMoment;
        eventBus.OnGhostPhaseEnded -= EndMoment;
    }

    private void BeginMoment()
    {
        if (desaturateOverlay != null)
            desaturateOverlay.gameObject.SetActive(true);
        if (shimmerOverlay != null)
            shimmerOverlay.gameObject.SetActive(true);

        if (playerRenderer != null)
            playerRenderer.color = ghostTint;

        audio?.SetMuffled(true);
        GameHaptics.ColdPhase();
        sealedDoor?.BeginShimmer();

        if (shimmerRoutine != null)
            StopCoroutine(shimmerRoutine);
        shimmerRoutine = StartCoroutine(PulseShimmer());
    }

    private void EndMoment()
    {
        HideOverlays();
        audio?.SetMuffled(false);

        if (playerRenderer != null)
            playerRenderer.color = normalPlayerColor;

        sealedDoor?.EndShimmer();
        audio?.PlayEchoWhisper();
        GameHaptics.ColdPhase();

        var hud = FindFirstObjectByType<GameplayHUD>();
        hud?.ShowToast("Something cold followed you through...", 3.5f);
    }

    private IEnumerator PulseShimmer()
    {
        while (shimmerOverlay != null && shimmerOverlay.gameObject.activeSelf)
        {
            shimmerOverlay.color = new Color(0.55f, 0.95f, 0.85f, 0.15f + Mathf.PingPong(Time.time * 2f, 0.2f));
            yield return null;
        }
    }

    private void HideOverlays()
    {
        if (desaturateOverlay != null)
            desaturateOverlay.gameObject.SetActive(false);
        if (shimmerOverlay != null)
            shimmerOverlay.gameObject.SetActive(false);
        if (shimmerRoutine != null)
        {
            StopCoroutine(shimmerRoutine);
            shimmerRoutine = null;
        }
    }

    private void BuildOverlays()
    {
        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        desaturateOverlay = CreateOverlay(canvas.transform, "GhostDesaturate",
            new Color(0.12f, 0.16f, 0.22f, 0.42f));
        shimmerOverlay = CreateOverlay(canvas.transform, "GhostShimmer",
            new Color(0.55f, 0.95f, 0.85f, 0.12f));
    }

    private static Image CreateOverlay(Transform parent, string name, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        go.SetActive(false);
        return image;
    }
}