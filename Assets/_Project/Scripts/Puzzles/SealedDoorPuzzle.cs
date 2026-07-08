using UnityEngine;

/// <summary>
/// Chapter 1 tutorial puzzle — requires Ghost Key to phase through a sealed door.
/// </summary>
public class SealedDoorPuzzle : PuzzleBase
{
    [SerializeField] private Collider2D doorCollider;
    [SerializeField] private GameObject passageTrigger;
    [SerializeField] private SpriteRenderer doorRenderer;

    public override string InteractionHint =>
        isSolved
            ? string.Empty
            : "Sealed door — use the Ghost Key (Interact)";

    protected override void Awake()
    {
        base.Awake();
        puzzleID = "chapter1_sealed_door";
        requiresSpecificKey = true;
        requiredKeyType = KeyType.Ghost;
    }

    protected override void TrySolve()
    {
        var ghostKey = FindFirstObjectByType<GhostKey>();
        if (ghostKey == null || !ghostKey.CanActivate())
        {
            OnPuzzleFailed();
            return;
        }

        ghostKey.Activate();

        if (doorCollider != null)
            doorCollider.enabled = false;

        if (passageTrigger != null)
            passageTrigger.SetActive(true);

        if (doorRenderer != null)
            doorRenderer.color = new Color(0.3f, 0.8f, 0.5f, 0.35f);

        MarkAsSolved();
    }
}