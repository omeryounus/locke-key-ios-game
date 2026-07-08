using UnityEngine;

/// <summary>
/// Puzzle 2 — push a collapsed bookshelf out of the way to clear the foyer path.
/// </summary>
public class CollapsedBookshelfPuzzle : PuzzleBase
{
    [SerializeField] private Collider2D blockingCollider;
    [SerializeField] private Transform shelfTransform;
    [SerializeField] private SpriteRenderer shelfRenderer;
    [SerializeField] private float pushDistance = 1.25f;
    [SerializeField] private int pushesRequired = 2;

    private int pushesDone;

    public override string InteractionHint =>
        isSolved
            ? string.Empty
            : pushesDone == 0
                ? "Collapsed bookshelf — tap Interact to push"
                : $"Bookshelf — push again ({pushesRequired - pushesDone} left)";

    protected override void Awake()
    {
        base.Awake();
        puzzleID = "chapter1_bookshelf";
        requiresSpecificKey = false;
    }

    protected override void TrySolve()
    {
        pushesDone++;

        if (shelfTransform != null)
            shelfTransform.position += Vector3.right * pushDistance;

        if (pushesDone < pushesRequired)
        {
            Debug.Log($"You shove the bookshelf aside ({pushesRequired - pushesDone} pushes left).");
            return;
        }

        if (blockingCollider != null)
            blockingCollider.enabled = false;

        if (shelfRenderer != null)
            shelfRenderer.color = new Color(0.35f, 0.28f, 0.2f, 0.55f);

        MarkAsSolved();
    }
}