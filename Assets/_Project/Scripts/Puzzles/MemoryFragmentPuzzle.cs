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

    protected override void TrySolve()
    {
        var headKey = FindFirstObjectByType<HeadKey>();
        if (headKey == null || !headKey.CanActivate())
        {
            OnPuzzleFailed();
            return;
        }

        headKey.Activate();

        if (portraitRenderer != null)
            portraitRenderer.color = new Color(0.75f, 0.55f, 0.95f, 1f);

        uiManager?.ShowMemoryFragment(
            "Rendell at the Black Door",
            "The portrait exhales cold light. You are standing in a hallway that was never built.\n\n" +
            "Rendell Locke turns toward you — or toward someone who isn't there yet.\n\n" +
            "\"The house remembers everything,\" he says. \"Even the things we tried to bury.\"",
            panelIndex: 1);

        MarkAsSolved();
    }
}