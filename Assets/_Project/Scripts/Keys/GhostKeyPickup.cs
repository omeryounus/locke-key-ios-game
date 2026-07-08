using UnityEngine;

/// <summary>
/// Grants the Ghost Key after Puzzle 1 is complete.
/// </summary>
public class GhostKeyPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private KeyManager keyManager;
    [SerializeField] private SpriteRenderer keyRenderer;
    [SerializeField] private ChapterBeatDirector beatDirector;

    [SerializeField] private string pickupId = "ghost_key";

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
        if (beatDirector == null)
            beatDirector = FindFirstObjectByType<ChapterBeatDirector>();

        eventBus = Resources.Load<EventBus>("EventBus");
        if (eventBus != null)
            eventBus.OnPuzzleSolved += HandlePuzzleSolved;

        SetVisible(false);
    }

    public void RestoreFromSave(ChapterSaveManager save)
    {
        if (save == null) return;

        if (save.IsPickupCollected(pickupId))
        {
            collected = true;
            SetVisible(false);
            return;
        }

        if (save.IsPuzzleSolved("chapter1_bookshelf"))
        {
            isAvailable = true;
            SetVisible(true);
            if (GetComponent<InteractGlow>() == null)
                gameObject.AddComponent<InteractGlow>();
        }
    }

    private void OnDestroy()
    {
        if (eventBus != null)
            eventBus.OnPuzzleSolved -= HandlePuzzleSolved;
    }

    private void HandlePuzzleSolved(PuzzleBase puzzle)
    {
        if (puzzle == null || puzzle.puzzleID != "chapter1_bookshelf" || collected)
            return;

        isAvailable = true;
        SetVisible(true);
        if (GetComponent<InteractGlow>() == null)
            gameObject.AddComponent<InteractGlow>();
        FindFirstObjectByType<ParticleVFXController>()?.PlayGhostRevealBurst(transform.position);
        Debug.Log("The Ghost Key gleams in the revealed alcove...");
    }

    public void Interact()
    {
        if (!isAvailable || collected || keyManager == null) return;

        keyManager.GrantGhostKey();
        collected = true;
        beatDirector?.NotifyGhostKeyCollected();
        SetVisible(false);
        PickupFeedback.PlayKeyPickup("Ghost Key claimed — the HUD pulses with pale light.");
        ChapterSaveManager.Instance?.RecordPickupCollected(pickupId);
    }

    private void SetVisible(bool visible)
    {
        if (keyRenderer != null)
            keyRenderer.enabled = visible;
    }
}