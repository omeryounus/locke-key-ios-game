using UnityEngine;

/// <summary>
/// Grants the Ghost Key after Puzzle 1 is complete.
/// </summary>
public class GhostKeyPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private KeyManager keyManager;
    [SerializeField] private SpriteRenderer keyRenderer;

    private bool isAvailable;
    private bool collected;
    private EventBus eventBus;

    public bool CanInteract => isAvailable && !collected;

    public string InteractionHint =>
        !isAvailable
            ? string.Empty
            : collected
                ? string.Empty
                : "Ghost Key — tap Interact to claim";

    private void Awake()
    {
        if (keyManager == null)
            keyManager = FindFirstObjectByType<KeyManager>();

        eventBus = Resources.Load<EventBus>("EventBus");
        if (eventBus != null)
            eventBus.OnPuzzleSolved += HandlePuzzleSolved;

        SetVisible(false);
    }

    private void OnDestroy()
    {
        if (eventBus != null)
            eventBus.OnPuzzleSolved -= HandlePuzzleSolved;
    }

    private void HandlePuzzleSolved(PuzzleBase puzzle)
    {
        if (puzzle == null || puzzle.puzzleID != "chapter1_stuck_door" || collected)
            return;

        isAvailable = true;
        SetVisible(true);
        Debug.Log("The Ghost Key has manifested in the foyer...");
    }

    public void Interact()
    {
        if (!isAvailable || collected || keyManager == null) return;

        keyManager.GrantGhostKey();
        collected = true;
        SetVisible(false);
    }

    private void SetVisible(bool visible)
    {
        if (keyRenderer != null)
            keyRenderer.enabled = visible;
    }
}