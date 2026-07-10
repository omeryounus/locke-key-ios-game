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
    private Vector3 basePos;
    private float bobPhase;

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

        basePos = transform.position;
        SetVisible(false);
    }

    private void Update()
    {
        if (!isAvailable || collected) return;
        bobPhase += Time.deltaTime * 2.4f;
        transform.position = basePos + Vector3.up * (Mathf.Sin(bobPhase) * 0.07f);
        transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(bobPhase * 0.85f) * 7f);
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
        if (GetComponent<InteractGlow>() == null)
            gameObject.AddComponent<InteractGlow>();
        FindFirstObjectByType<GameplayHUD>()?.ShowToast("The Head Key glimmers beyond the passage.", 3.5f);
        FindFirstObjectByType<CameraFollow2D>()?.Pulse(0.18f, 0.28f);
        GameHaptics.KeyPickup();
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