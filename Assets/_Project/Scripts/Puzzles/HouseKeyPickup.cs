using UnityEngine;

/// <summary>
/// Beat 1 — glinting house key near the exterior planter.
/// </summary>
public class HouseKeyPickup : SaveablePickup, IInteractable
{
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private SpriteRenderer keyRenderer;
    [SerializeField] private ChapterBeatDirector beatDirector;
    [SerializeField] private float glintScale = 0.12f;
    [SerializeField] private float glintSpeed = 3.2f;
    private Vector3 baseScale;
    private float glintPhase;

    public bool CanInteract => !Collected;

    public string InteractionHint =>
        Collected ? string.Empty : "Glinting house key — tap Interact";

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

        // Guard: already discovered (e.g. save restore) — don't show sheet again.
        if (ChapterSaveManager.Instance?.HasKeyDiscovered("anywhere") == true)
        {
            MarkCollected();
            return;
        }

        // S4 Key Discovery sheet — player confirms before key is added.
        if (GrokUIFlowManager.Instance != null)
        {
            GrokUIFlowManager.Instance.ShowDiscovery(
                keyId: "anywhere",
                onAdded: () =>
                {
                    AddKeyToInventory(equipIt: false);
                },
                onAddedAndEquipped: () =>
                {
                    AddKeyToInventory(equipIt: true);
                });
        }
        else
        {
            // Fallback if FlowManager not present (e.g. unit test scene).
            AddKeyToInventory(equipIt: true);
        }
    }

    private void AddKeyToInventory(bool equipIt)
    {
        if (playerInventory == null) return;

        playerInventory.PickupHouseKey();
        MarkCollected();
        beatDirector?.NotifyHouseKeyCollected();

        var save = ChapterSaveManager.Instance;
        save?.RecordKeyDiscovered("anywhere");
        if (equipIt) save?.RecordEquippedKey("anywhere");
        save?.SaveNow();

        PickupFeedback.PlayKeyPickup("Anywhere Key added to key ring.");
    }

    protected override void ApplyCollectedVisuals()
    {
        if (keyRenderer != null)
            keyRenderer.enabled = false;
    }
}