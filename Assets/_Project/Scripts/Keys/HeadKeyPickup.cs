using UnityEngine;

/// <summary>
/// Grants the Head Key after the sealed door puzzle is solved.
/// </summary>
public class HeadKeyPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private KeyManager keyManager;
    [SerializeField] private SpriteRenderer keyRenderer;

    [SerializeField] private string pickupId = "head_key";
    private const string KeyId = "head";

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

    public void RestoreFromSave(ChapterSaveManager save)
    {
        if (save == null) return;

        if (save.IsPickupCollected(pickupId) || save.HasKeyDiscovered(KeyId))
        {
            collected = true;
            SetVisible(false);
            return;
        }

        if (save.IsPuzzleSolved("chapter1_sealed_door"))
        {
            isAvailable = true;
            SetVisible(true);
        }
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

        if (ChapterSaveManager.Instance?.HasKeyDiscovered(KeyId) == true)
        {
            CompletePickup(equipIt: false);
            return;
        }

        if (GrokUIFlowManager.Instance != null)
        {
            GrokUIFlowManager.Instance.ShowDiscovery(
                keyId: KeyId,
                onAdded: () => CompletePickup(equipIt: false),
                onAddedAndEquipped: () => CompletePickup(equipIt: true));
        }
        else
            CompletePickup(equipIt: true);
    }

    private void CompletePickup(bool equipIt)
    {
        keyManager.GrantHeadKeySilent();
        collected = true;
        SetVisible(false);

        var save = ChapterSaveManager.Instance;
        save?.RecordKeyDiscovered(KeyId);
        if (equipIt) save?.RecordEquippedKey(KeyId);
        save?.RecordPickupCollected(pickupId);
        save?.SaveNow();

        PickupFeedback.PlayKeyPickup("Head Key added to key ring.");
    }

    private void SetVisible(bool visible)
    {
        if (keyRenderer != null)
            keyRenderer.enabled = visible;
    }
}