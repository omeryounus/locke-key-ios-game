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
    private Color doorBaseColor = Color.white;

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
        Debug.Log("The door is sealed by old magic. Stand close and tap Use Key.");
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

    private void HandleGhostPhaseStarted()
    {
        if (isSolved || player == null || !IsPlayerInRange())
            return;

        passageOpen = true;
        playerCrossed = false;

        if (doorCollider != null)
            doorCollider.enabled = false;

        if (doorRenderer != null)
            doorRenderer.color = new Color(0.35f, 0.75f, 0.55f, 0.4f);

        if (passageTrigger != null)
            passageTrigger.SetActive(true);

        Debug.Log("You phase through the sealed door...");
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
            return;
        }

        ResealDoor();
        Debug.Log("The sealed door snaps shut — you must phase all the way through.");
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