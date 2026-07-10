using UnityEngine;

/// <summary>
/// Tutorial pickup: House Key. Open the front door with Interact.
/// Named consistently as House Key (not Anywhere) so the solution is obvious.
/// </summary>
public class HouseKeyPickup : SaveablePickup, IInteractable
{
    public const string KeyId = "house";

    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private SpriteRenderer keyRenderer;
    [SerializeField] private ChapterBeatDirector beatDirector;
    [SerializeField] private float glintScale = 0.12f;
    [SerializeField] private float glintSpeed = 3.2f;
    private Vector3 baseScale;
    private float glintPhase;

    public bool CanInteract => !Collected;

    public string InteractionHint =>
        Collected ? string.Empty : "House Key — tap Interact to pick up";

    protected override void Awake()
    {
        pickupId = "house_key";
        base.Awake();

        if (playerInventory == null)
            playerInventory = FindFirstObjectByType<PlayerInventory>();
        if (beatDirector == null)
            beatDirector = FindFirstObjectByType<ChapterBeatDirector>();
        if (keyRenderer == null)
            keyRenderer = GetComponent<SpriteRenderer>();

        baseScale = transform.localScale;
        if (GetComponent<InteractGlow>() == null)
            gameObject.AddComponent<InteractGlow>();
    }

    private void Update()
    {
        if (Collected) return;

        glintPhase += Time.deltaTime * glintSpeed;
        var pulse = 1f + Mathf.Sin(glintPhase) * glintScale;
        transform.localScale = baseScale * pulse;

        if (keyRenderer != null)
        {
            var alpha = 0.82f + Mathf.Sin(glintPhase * 1.4f) * 0.18f;
            var c = keyRenderer.color;
            keyRenderer.color = new Color(c.r, c.g, c.b, alpha);
        }
    }

    public void Interact()
    {
        if (Collected) return;

        // Already have it (save restore) — just clean up visual.
        if (playerInventory != null && playerInventory.HasHouseKey)
        {
            MarkCollected();
            return;
        }

        if (ChapterSaveManager.Instance?.HasKeyDiscovered(KeyId) == true
            || ChapterSaveManager.Instance?.HasKeyDiscovered("anywhere") == true)
        {
            // Legacy saves used "anywhere" for this key.
            playerInventory?.PickupHouseKey();
            MarkCollected();
            beatDirector?.NotifyHouseKeyCollected();
            return;
        }

        // Immediate pickup — no multi-step sheet that confuses the tutorial.
        // Optional discovery sheet only if we want spectacle; prefer clarity.
        AddKeyToInventory();
    }

    private void AddKeyToInventory()
    {
        if (playerInventory == null) return;

        playerInventory.PickupHouseKey();
        MarkCollected();
        beatDirector?.NotifyHouseKeyCollected();

        var save = ChapterSaveManager.Instance;
        save?.RecordKeyDiscovered(KeyId);
        save?.RecordEquippedKey(KeyId);
        // Keep map systems that still key off "anywhere" working for older builds.
        save?.RecordKeyDiscovered("anywhere");
        save?.RecordEquippedKey("anywhere");
        save?.SaveNow();

        PickupFeedback.PlayKeyPickup("House Key collected — use it on the front door.");
        FindFirstObjectByType<GameplayHUD>()?.ShowToast(
            "House Key collected. Walk to the door and tap Interact.", 4f);
    }

    protected override void ApplyCollectedVisuals()
    {
        if (keyRenderer != null)
            keyRenderer.enabled = false;
    }
}
