using UnityEngine;

/// <summary>
/// Beat 4 — sealed door opens only while the Ghost Key phase is active (Use Key, not Interact).
/// Solved only after the player actually crosses the passage trigger.
/// </summary>
public class SealedDoorPuzzle : PuzzleBase
{
    [SerializeField] private Collider2D doorCollider;
    [SerializeField] private GameObject passageTrigger;
    [SerializeField] private SpriteRenderer doorRenderer;
    [SerializeField] private float useKeyRange = 3f;

    private Transform player;
    private bool passageOpen;
    private bool playerCrossed;
    private bool shimmering;
    private Color doorBaseColor = Color.white;
    private Color shimmerColor = new(0.45f, 0.95f, 0.82f, 0.55f);

    public override bool CanInteract => !isSolved;

    public override string InteractionHint =>
        isSolved
            ? string.Empty
            : "Sealed door — tap Use Key when nearby";

    protected override void Awake()
    {
        base.Awake();
        puzzleID = "chapter1_sealed_door";
        requiresSpecificKey = false;
        player = FindFirstObjectByType<PlayerController>()?.transform;

        if (doorRenderer != null)
            doorBaseColor = doorRenderer.color;

        if (passageTrigger != null)
        {
            passageTrigger.SetActive(false);
            var zone = passageTrigger.GetComponent<SealedDoorPassageZone>();
            if (zone == null)
                zone = passageTrigger.AddComponent<SealedDoorPassageZone>();
            zone.Bind(this);
        }
    }

    private void OnEnable()
    {
        if (eventBus != null)
        {
            eventBus.OnGhostPhaseStarted += HandleGhostPhaseStarted;
            eventBus.OnGhostPhaseEnded += HandleGhostPhaseEnded;
        }
    }

    private void OnDisable()
    {
        if (eventBus == null) return;
        eventBus.OnGhostPhaseStarted -= HandleGhostPhaseStarted;
        eventBus.OnGhostPhaseEnded -= HandleGhostPhaseEnded;
    }

    public override void Interact()
    {
        if (isSolved) return;
        var hud = FindFirstObjectByType<GameplayHUD>();
        var hasGhost = FindFirstObjectByType<KeyManager>()?.ownedKeys
            .Exists(k => k.abilityType == KeyManager.KeyAbilityType.GhostPhase) == true;
        if (!hasGhost)
            hud?.ShowToast("Sealed by old magic. Something that phases might pass...", 3f);
        else
            hud?.ShowToast("Stand close and tap Use Key to phase through.", 3f);
        FindFirstObjectByType<GameAudioController>()?.PlayDoorRattle();
        GameHaptics.TriggerHapticLight();
    }

    protected override void TrySolve() { }

    public bool IsPlayerInRange()
    {
        if (player == null) return false;
        return Vector2.Distance(player.position, transform.position) <= useKeyRange;
    }

    public void NotifyPlayerCrossed()
    {
        if (!passageOpen || isSolved) return;
        playerCrossed = true;
    }

    public void BeginShimmer() => shimmering = true;

    public void EndShimmer()
    {
        shimmering = false;
        if (doorRenderer != null && !isSolved)
            doorRenderer.color = doorBaseColor;
    }

    private void Update()
    {
        if (!shimmering || doorRenderer == null) return;
        var pulse = 0.45f + Mathf.PingPong(Time.time * 2.4f, 0.35f);
        doorRenderer.color = new Color(shimmerColor.r, shimmerColor.g, shimmerColor.b, pulse);
    }

    private void HandleGhostPhaseStarted()
    {
        if (isSolved || player == null || !IsPlayerInRange())
            return;

        passageOpen = true;
        playerCrossed = false;

        if (doorCollider != null)
            doorCollider.enabled = false;

        shimmering = true;
        if (doorRenderer != null)
            doorRenderer.color = shimmerColor;

        if (passageTrigger != null)
            passageTrigger.SetActive(true);

        FindFirstObjectByType<CameraFollow2D>()?.Pulse(0.25f, 0.4f);
        FindFirstObjectByType<GameplayHUD>()?.ShowToast("The seal softens — step through now!", 2.8f);
    }

    private void HandleGhostPhaseEnded()
    {
        if (!passageOpen || isSolved)
            return;

        if (playerCrossed)
        {
            MarkAsSolved();
            if (doorRenderer != null)
                doorRenderer.color = new Color(0.3f, 0.55f, 0.42f, 0.35f);
            GameHaptics.Unlock();
            FindFirstObjectByType<GameplayHUD>()?.ShowToast("You pass through the sealed door.", 3f);
            FindFirstObjectByType<CameraFollow2D>()?.Pulse(0.2f, 0.3f);
            return;
        }

        EndShimmer();
        ResealDoor();
        FindFirstObjectByType<GameplayHUD>()?.ShowToast("The seal snaps shut — phase all the way through.", 3.2f);
        FindFirstObjectByType<GameAudioController>()?.PlayDoorRattle();
        GameHaptics.TriggerHapticStall();
        FindFirstObjectByType<CameraFollow2D>()?.Pulse(0.15f, 0.22f);
    }

    public override void RestoreSolvedState()
    {
        base.RestoreSolvedState();
        passageOpen = true;
        playerCrossed = true;
        if (doorCollider != null)
            doorCollider.enabled = false;
        if (doorRenderer != null)
            doorRenderer.color = new Color(0.3f, 0.55f, 0.42f, 0.35f);
        if (passageTrigger != null)
            passageTrigger.SetActive(true);
    }

    private void ResealDoor()
    {
        passageOpen = false;
        playerCrossed = false;

        if (doorCollider != null)
            doorCollider.enabled = true;

        if (doorRenderer != null)
            doorRenderer.color = doorBaseColor;

        if (passageTrigger != null)
            passageTrigger.SetActive(false);
    }
}