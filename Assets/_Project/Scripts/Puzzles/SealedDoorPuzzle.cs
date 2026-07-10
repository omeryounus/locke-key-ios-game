using UnityEngine;

/// <summary>
/// Ghost Key tutorial: equip/own Ghost Key, stand at the sealed door, tap Use Key,
/// then walk through while phasing. One clear magical solution.
/// </summary>
public class SealedDoorPuzzle : PuzzleBase
{
    [SerializeField] private Collider2D doorCollider;
    [SerializeField] private GameObject passageTrigger;
    [SerializeField] private SpriteRenderer doorRenderer;
    [SerializeField] private float useKeyRange = 3.2f;

    private Transform player;
    private bool passageOpen;
    private bool playerCrossed;
    private bool shimmering;
    private Color doorBaseColor = Color.white;
    private Color shimmerColor = new(0.45f, 0.95f, 0.82f, 0.55f);

    public override bool CanInteract => !isSolved;

    public override string InteractionHint
    {
        get
        {
            if (isSolved) return string.Empty;
            if (!HasGhostKey())
                return "Sealed door — needs the Ghost Key (clear the bookshelf)";
            return "Sealed door — stand here and tap Use Key to phase through";
        }
    }

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

        if (!HasGhostKey())
        {
            hud?.ShowToast("Solid magic seals this door. A Ghost Key might pass through…", 3.5f);
            FindFirstObjectByType<GameAudioController>()?.PlayDoorRattle();
            return;
        }

        if (!IsPlayerInRange())
        {
            hud?.ShowToast("Step closer, then tap Use Key.", 2.8f);
            return;
        }

        // Interact near door with Ghost Key = same as Use Key (removes confusion).
        var km = FindFirstObjectByType<KeyManager>();
        if (km != null)
        {
            // Ensure Ghost is active, then use it.
            var ghost = km.ownedKeys.Find(k => k.abilityType == KeyManager.KeyAbilityType.GhostPhase);
            if (ghost != null)
                km.SelectKey(ghost);
            km.UseActiveKey();
        }
        else
        {
            hud?.ShowToast("Tap Use Key to phase through the seal.", 3f);
        }
    }

    protected override void TrySolve() { }

    public bool IsPlayerInRange()
    {
        if (player == null)
            player = FindFirstObjectByType<PlayerController>()?.transform;
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
        if (isSolved || !IsPlayerInRange())
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
        FindFirstObjectByType<GameplayHUD>()?.ShowToast(
            "You are ethereal — walk through the sealed door now!", 3.2f);
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
        FindFirstObjectByType<GameplayHUD>()?.ShowToast(
            "The seal snapped shut — phase again and walk all the way through.", 3.5f);
        FindFirstObjectByType<GameAudioController>()?.PlayDoorRattle();
        GameHaptics.TriggerHapticStall();
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

    private static bool HasGhostKey()
    {
        var km = FindFirstObjectByType<KeyManager>();
        return km != null && km.ownedKeys.Exists(k => k.abilityType == KeyManager.KeyAbilityType.GhostPhase);
    }
}
