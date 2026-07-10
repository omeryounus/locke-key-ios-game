using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Tutorial door: find the house key, then Interact once to unlock.
/// No multi-step lock UI — the solution must be obvious.
/// </summary>
public class StuckDoorPuzzle : PuzzleBase
{
    [SerializeField] private Collider2D doorCollider;
    [SerializeField] private SpriteRenderer doorRenderer;
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private Light2D warmLeakLight;
    [SerializeField] private float unlockDuration = 0.85f;

    private bool animating;
    private Vector3 doorBasePos;

    public override bool CanInteract => !isSolved && !animating;

    public override string InteractionHint =>
        isSolved
            ? string.Empty
            : playerInventory != null && playerInventory.HasHouseKey
                ? "Front door — tap Interact to unlock with the House Key"
                : "Front door — locked. Find the House Key first";

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

        // Simple rule: have house key → open. No key ring / equip maze.
        if (playerInventory != null && playerInventory.HasHouseKey)
        {
            StartCoroutine(UnlockSequence());
            return;
        }

        StartCoroutine(RattleDoor());
        FindFirstObjectByType<GameAudioController>()?.PlayDoorRattle();
        FindFirstObjectByType<GameplayHUD>()?.ShowToast(
            "It's locked. Look for a house key nearby.", 3f);
    }

    protected override void TrySolve() { }

    public override void RestoreSolvedState()
    {
        base.RestoreSolvedState();
        if (doorCollider != null)
            doorCollider.enabled = false;
        if (warmLeakLight != null)
            warmLeakLight.intensity = 0.75f;
        transform.position = doorBasePos + Vector3.right * 0.22f;
        if (doorRenderer != null)
            doorRenderer.color = new Color(0.55f, 0.42f, 0.3f, 0.45f);
        RecordMapProgress();
    }

    private IEnumerator RattleDoor()
    {
        animating = true;
        GameHaptics.TriggerHapticLight();
        var elapsed = 0f;
        while (elapsed < 0.35f)
        {
            elapsed += Time.deltaTime;
            var shake = Mathf.Sin(elapsed * 50f) * 0.05f * (1f - elapsed / 0.35f);
            transform.position = doorBasePos + Vector3.right * shake;
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
        FindFirstObjectByType<CameraFollow2D>()?.Pulse(0.16f, 0.3f);
        FindFirstObjectByType<CameraFollow2D>()?.Shake(0.12f, 0.35f);
        FindFirstObjectByType<ParticleVFXController>()?.PlayMemoryBurst(transform.position);
        FindFirstObjectByType<GameplayHUD>()?.ShowToast("House Key turns… the door opens.", 2.8f);

        var startColor = doorRenderer != null ? doorRenderer.color : Color.white;
        var elapsed = 0f;

        while (elapsed < unlockDuration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.SmoothStep(0f, 1f, elapsed / unlockDuration);
            transform.position = doorBasePos + Vector3.right * Mathf.Lerp(0f, 0.22f, t);

            if (doorRenderer != null)
                doorRenderer.color = Color.Lerp(startColor, new Color(0.55f, 0.42f, 0.3f, 0.4f), t);

            if (warmLeakLight != null)
                warmLeakLight.intensity = Mathf.Lerp(0f, 0.85f, t);

            yield return null;
        }

        if (doorCollider != null)
            doorCollider.enabled = false;

        RecordMapProgress();
        FindFirstObjectByType<GameplayHUD>()?.ShowToast(
            "Path open — explore the library ahead.", 3.2f);
        MarkAsSolved();
        animating = false;
    }

    private static void RecordMapProgress()
    {
        var save = ChapterSaveManager.Instance;
        if (save == null) return;
        // Keep map progression in sync (wellhouse unlock after foyer door).
        save.RecordHotspotSolved("foyer_stair_door");
        save.RecordRoomUnlocked("wellhouse");
        save.SaveNow();
    }
}
