using UnityEngine;

/// <summary>
/// Puzzle 4 — use the Head Key on a family portrait to glimpse Rendell's memory.
/// </summary>
public class MemoryFragmentPuzzle : PuzzleBase
{
    [SerializeField] private SpriteRenderer portraitRenderer;
    [SerializeField] private UIManager uiManager;

    public override string InteractionHint =>
        isSolved
            ? string.Empty
            : "Family portrait — use the Head Key (Interact)";

    protected override void Awake()
    {
        base.Awake();
        puzzleID = "chapter1_memory_fragment";
        requiresSpecificKey = true;
        requiredKeyType = KeyType.Head;

        if (uiManager == null)
            uiManager = FindFirstObjectByType<UIManager>();
    }

    public override void RestoreSolvedState()
    {
        base.RestoreSolvedState();
        if (portraitRenderer != null)
            portraitRenderer.color = new Color(0.75f, 0.55f, 0.95f, 1f);
    }

    protected override void TrySolve()
    {
        var headKey = FindFirstObjectByType<HeadKey>();
        if (headKey == null || !headKey.CanActivate())
        {
            OnPuzzleFailed();
            return;
        }

        headKey.Activate();

        var touchController = FindFirstObjectByType<TouchGameplayController>();
        if (touchController != null)
            touchController.SetInputLocked(true);

        var mindscapeGo = new GameObject("HeadKeyMindscape");
        var mindscapePanel = mindscapeGo.AddComponent<HeadKeyMindscapePanel>();
        mindscapePanel.Initialize(uiManager);
    }

    public void SolveFromUI()
    {
        if (portraitRenderer != null)
            portraitRenderer.color = new Color(0.75f, 0.55f, 0.95f, 1f);

        MarkAsSolved();
    }
}