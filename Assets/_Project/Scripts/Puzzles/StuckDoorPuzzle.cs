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
    [SerializeField] private float unlockDuration = 0.9f;

    private bool animating;
    private Vector3 doorBasePos;

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
        // The sheet handles State A (no/wrong key) and State B (correct key) internally.
        if (GrokUIFlowManager.Instance != null)
        {
            GrokUIFlowManager.Instance.ShowLock(
                def: LockDefinition.FoyerStairDoor,
                onSuccess: () => StartCoroutine(UnlockSequence()));
            return;
        }

        // Fallback: legacy behaviour (no FlowManager, e.g. unit tests).
        if (playerInventory == null || !playerInventory.HasHouseKey)
        {
            StartCoroutine(RattleDoor());
            FindFirstObjectByType<GameAudioController>()?.PlayDoorRattle();
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
    }

    private IEnumerator RattleDoor()
    {
        animating = true;
        var elapsed = 0f;
        while (elapsed < 0.35f)
        {
            elapsed += Time.deltaTime;
            var shake = Mathf.Sin(elapsed * 42f) * 0.04f;
            transform.position = doorBasePos + Vector3.right * shake;
            yield return null;
        }

        transform.position = doorBasePos;
        animating = false;
    }

    private IEnumerator UnlockSequence()
    {
        animating = true;
        var startColor = doorRenderer != null ? doorRenderer.color : Color.white;
        var elapsed = 0f;

        while (elapsed < unlockDuration)
        {
            elapsed += Time.deltaTime;
            var t = elapsed / unlockDuration;
            transform.position = doorBasePos + Vector3.right * Mathf.Lerp(0f, 0.18f, t);

            if (doorRenderer != null)
            {
                doorRenderer.color = Color.Lerp(startColor, new Color(0.55f, 0.42f, 0.3f, 0.45f), t);
            }

            if (warmLeakLight != null)
                warmLeakLight.intensity = Mathf.Lerp(0f, 0.75f, t);

            yield return null;
        }

        if (doorCollider != null)
            doorCollider.enabled = false;

        MarkAsSolved();
        animating = false;
        Debug.Log("Warm light spills from the library beyond.");
    }
}