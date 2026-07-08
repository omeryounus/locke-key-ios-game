using UnityEngine;

/// <summary>
/// Puzzle 1 — unlock with the ordinary house key before magical keys appear.
/// </summary>
public class StuckDoorPuzzle : PuzzleBase
{
    [SerializeField] private Collider2D doorCollider;
    [SerializeField] private SpriteRenderer doorRenderer;
    [SerializeField] private PlayerInventory playerInventory;

    public override string InteractionHint =>
        isSolved
            ? string.Empty
            : playerInventory != null && playerInventory.HasHouseKey
                ? "Stuck door — tap Interact to unlock"
                : "Stuck door — you need the house key";

    protected override void Awake()
    {
        base.Awake();
        puzzleID = "chapter1_stuck_door";
        requiresSpecificKey = false;

        if (playerInventory == null)
            playerInventory = FindFirstObjectByType<PlayerInventory>();
    }

    protected override bool HasRequiredKey()
    {
        return playerInventory != null && playerInventory.HasHouseKey;
    }

    protected override void TrySolve()
    {
        if (doorCollider != null)
            doorCollider.enabled = false;

        if (doorRenderer != null)
            doorRenderer.color = new Color(0.45f, 0.35f, 0.28f, 0.5f);

        MarkAsSolved();
    }
}