using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Beat 2 — stuck foyer door with rattle feedback and unlock animation.
/// </summary>
public class StuckDoorPuzzle : PuzzleBase
{
    [SerializeField] private Collider2D doorCollider;
    [SerializeField] private SpriteRenderer doorRenderer;
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private Light2D warmLeakLight;
    [SerializeField] private float unlockDuration = 0.95f;

    private bool animating;
    private Vector3 doorBasePos;

    public override bool CanInteract => !isSolved && !animating;

    public override string InteractionHint =>
        isSolved
            ? string.Empty
            : playerInventory != null && playerInventory.HasHouseKey
                ? "Stuck door — tap Interact to unlock"
                : "Stuck door — locked (find the house key)";

    protected override void Awake()
    {
        base.Awake();
        puzzleID = "chapter1_stuck_door";
        requiresSpecificKey = false;
        doorBasePos = transform.position;

        if (playerInventory == null)
            playerInventory = FindFirstObjectByType<PlayerInventory>();
        if (warmLeakLight == null)
        {
            var lightGo = new GameObject("WarmLeakLight");
            lightGo.transform.SetParent(transform);
            lightGo.transform.localPosition = new Vector3(0.35f, 0.2f, 0f);
            warmLeakLight = lightGo.AddComponent<Light2D>();
            warmLeakLight.lightType = Light2D.LightType.Point;
            warmLeakLight.color = new Color(1f, 0.72f, 0.4f);
            warmLeakLight.intensity = 0f;
            warmLeakLight.pointLightOuterRadius = 2.5f;
        }
    }

    public override void Interact()
    {
        if (isSolved || animating) return;

        // If FlowManager present: always open the S5 lock sheet.
        if (GrokUIFlowManager.Instance != null)
        {
            GrokUIFlowManager.Instance.ShowLock(
                def: LockDefinition.FoyerStairDoor,
                onSuccess: () => StartCoroutine(UnlockSequence()));
            return;
        }

        if (playerInventory == null || !playerInventory.HasHouseKey)
        {
            StartCoroutine(RattleDoor());
            FindFirstObjectByType<GameAudioController>()?.PlayDoorRattle();
            FindFirstObjectByType<GameplayHUD>()?.ShowToast("The door won't budge without a key.", 2.5f);
            return;
        }

        StartCoroutine(UnlockSequence());
    }

    protected override void TrySolve() { }

    public override void RestoreSolvedState()
    {
        base.RestoreSolvedState();
        if (doorCollider != null)
            doorCollider.enabled = false;
        if (warmLeakLight != null)
            warmLeakLight.intensity = 0.75f;
        transform.position = doorBasePos + Vector3.right * 0.18f;
        if (doorRenderer != null)
            doorRenderer.color = new Color(0.55f, 0.42f, 0.3f, 0.45f);
    }

    private IEnumerator RattleDoor()
    {
        animating = true;
        GameHaptics.TriggerHapticLight();
        FindFirstObjectByType<CameraFollow2D>()?.Pulse(0.1f, 0.2f);

        var elapsed = 0f;
        while (elapsed < 0.4f)
        {
            elapsed += Time.deltaTime;
            var shake = Mathf.Sin(elapsed * 48f) * 0.05f * (1f - elapsed / 0.4f);
            transform.position = doorBasePos + Vector3.right * shake;
            if (doorRenderer != null)
            {
                var c = doorRenderer.color;
                doorRenderer.color = new Color(c.r, c.g * 0.95f, c.b * 0.9f, c.a);
            }

            yield return null;
        }

        transform.position = doorBasePos;
        animating = false;
    }

    private IEnumerator UnlockSequence()
    {
        animating = true;
        FindFirstObjectByType<GameAudioController>()?.PlayDoorUnlock();
        GameHaptics.Unlock();
        FindFirstObjectByType<CameraFollow2D>()?.Pulse(0.18f, 0.35f);

        var startColor = doorRenderer != null ? doorRenderer.color : Color.white;
        var elapsed = 0f;

        while (elapsed < unlockDuration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.SmoothStep(0f, 1f, elapsed / unlockDuration);
            // Ease-out slide with soft settle
            var slide = Mathf.Lerp(0f, 0.2f, t) - Mathf.Sin(t * Mathf.PI) * 0.02f;
            transform.position = doorBasePos + Vector3.right * slide;

            if (doorRenderer != null)
                doorRenderer.color = Color.Lerp(startColor, new Color(0.55f, 0.42f, 0.3f, 0.45f), t);

            if (warmLeakLight != null)
                warmLeakLight.intensity = Mathf.Lerp(0f, 0.85f, t);

            yield return null;
        }

        if (doorCollider != null)
            doorCollider.enabled = false;

        FindFirstObjectByType<GameplayHUD>()?.ShowToast("Warm light spills from the library beyond.", 3.2f);
        MarkAsSolved();
        animating = false;
    }
}
