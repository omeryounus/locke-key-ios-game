using UnityEngine;

/// <summary>
/// Beat 3 — three-push bookshelf that reveals the Ghost Key alcove.
/// </summary>
public class CollapsedBookshelfPuzzle : PuzzleBase
{
    [SerializeField] private Collider2D blockingCollider;
    [SerializeField] private Transform shelfTransform;
    [SerializeField] private SpriteRenderer shelfRenderer;
    [SerializeField] private Transform debrisParent;
    [SerializeField] private float pushDistance = 0.85f;
    [SerializeField] private int pushesRequired = 3;

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

        if (debrisParent == null)
        {
            var debrisGo = new GameObject("BookshelfDebris");
            debrisGo.transform.SetParent(transform);
            debrisParent = debrisGo.transform;
        }
    }

    protected override void TrySolve()
    {
        pushesDone++;
        SpawnDebrisBurst();

        if (shelfTransform != null)
            shelfTransform.position += Vector3.right * pushDistance;

        if (pushesDone < pushesRequired)
        {
            Debug.Log($"Dust and books tumble free ({pushesRequired - pushesDone} pushes left).");
            return;
        }

        if (blockingCollider != null)
            blockingCollider.enabled = false;

        if (shelfRenderer != null)
            shelfRenderer.color = new Color(0.35f, 0.28f, 0.2f, 0.55f);

        MarkAsSolved();
        Debug.Log("A hidden alcove opens — something glimmers inside.");
    }

    public override void RestoreSolvedState()
    {
        base.RestoreSolvedState();
        pushesDone = pushesRequired;
        if (blockingCollider != null)
            blockingCollider.enabled = false;
        if (shelfTransform != null)
            shelfTransform.position += Vector3.right * (pushDistance * pushesRequired);
        if (shelfRenderer != null)
            shelfRenderer.color = new Color(0.35f, 0.28f, 0.2f, 0.55f);
    }

    private void SpawnDebrisBurst()
    {
        for (var i = 0; i < 3; i++)
        {
            var bit = new GameObject($"Debris_{pushesDone}_{i}", typeof(SpriteRenderer));
            bit.transform.SetParent(debrisParent);
            bit.transform.position = shelfTransform.position + new Vector3(Random.Range(-0.2f, 0.6f), Random.Range(-0.3f, 0.2f), 0f);
            bit.transform.localScale = Vector3.one * Random.Range(0.08f, 0.14f);
            var sr = bit.GetComponent<SpriteRenderer>();
            sr.color = new Color(0.45f, 0.32f, 0.2f, 0.8f);
            sr.sortingOrder = 3;
            Destroy(bit, 2.5f);
        }
    }
}