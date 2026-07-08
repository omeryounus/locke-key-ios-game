using UnityEngine;

/// <summary>
/// Beat 1 — glinting house key near the exterior planter.
/// </summary>
public class HouseKeyPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private SpriteRenderer keyRenderer;
    [SerializeField] private ChapterBeatDirector beatDirector;
    [SerializeField] private float glintScale = 0.12f;
    [SerializeField] private float glintSpeed = 3.2f;

    private bool collected;
    private Vector3 baseScale;
    private float glintPhase;

    public bool CanInteract => !collected;

    public string InteractionHint =>
        collected ? string.Empty : "Glinting house key — tap Interact";

    private void Awake()
    {
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
        if (collected) return;

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
        if (collected || playerInventory == null) return;

        playerInventory.PickupHouseKey();
        collected = true;
        beatDirector?.NotifyHouseKeyCollected();

        if (keyRenderer != null)
            keyRenderer.enabled = false;

        PickupFeedback.PlayKeyPickup("House key collected — return to the entrance.");
    }
}