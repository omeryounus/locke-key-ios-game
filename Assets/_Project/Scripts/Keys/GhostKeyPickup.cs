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
    private const string KeyId = "ghost";

    private bool isAvailable;
    private bool collected;
    private EventBus eventBus;
    private Vector3 basePos;
    private float bobPhase;

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

        basePos = transform.position;
        SetVisible(false);
    }

    private void Update()
    {
        if (!isAvailable || collected) return;
        bobPhase += Time.deltaTime * 2.6f;
        transform.position = basePos + Vector3.up * (Mathf.Sin(bobPhase) * 0.08f);
        transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(bobPhase * 0.9f) * 8f);
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

        if (save.IsPuzzleSolved("chapter1_bookshelf") || save.IsGhostKeyRevealed)
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
        FindFirstObjectByType<GameplayHUD>()?.ShowToast("The Ghost Key gleams in the alcove.", 3.5f);
        FindFirstObjectByType<CameraFollow2D>()?.Pulse(0.2f, 0.3f);
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
        keyManager.GrantGhostKeySilent();
        collected = true;
        beatDirector?.NotifyGhostKeyCollected();
        SetVisible(false);

        var save = ChapterSaveManager.Instance;
        save?.RecordKeyDiscovered(KeyId);
        if (equipIt) save?.RecordEquippedKey(KeyId);
        save?.RecordPickupCollected(pickupId);
        save?.SaveNow();

        PickupFeedback.PlayKeyPickup("Ghost Key added to key ring.");
    }

    private void SetVisible(bool visible)
    {
        if (keyRenderer != null)
            keyRenderer.enabled = visible;
    }
}