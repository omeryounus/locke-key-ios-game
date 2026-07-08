using UnityEngine;

/// <summary>
/// Grants the Head Key after the sealed door puzzle is solved.
/// </summary>
public class HeadKeyPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private KeyManager keyManager;
    [SerializeField] private SpriteRenderer keyRenderer;

    private bool isAvailable;
    private bool collected;
    private EventBus eventBus;

    public bool CanInteract => isAvailable && !collected;

    public string InteractionHint =>
        !isAvailable || collected
            ? string.Empty
            : "Head Key — tap Interact to claim";

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
        if (puzzle == null || puzzle.puzzleID != "chapter1_sealed_door" || collected)
            return;

        isAvailable = true;
        SetVisible(true);
        Debug.Log("The Head Key glimmers beyond the sealed passage...");
    }

    public void Interact()
    {
        if (!isAvailable || collected || keyManager == null) return;

        keyManager.GrantHeadKey();
        collected = true;
        SetVisible(false);
    }

    private void SetVisible(bool visible)
    {
        if (keyRenderer != null)
            keyRenderer.enabled = visible;
    }
}