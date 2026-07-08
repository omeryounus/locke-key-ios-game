using UnityEngine;

/// <summary>
/// Tutorial pickup for the ordinary house key on a hall table.
/// </summary>
public class HouseKeyPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private SpriteRenderer keyRenderer;

    private bool collected;

    public bool CanInteract => !collected;

    public string InteractionHint =>
        collected ? string.Empty : "House key — tap Interact to pick up";

    private void Awake()
    {
        if (playerInventory == null)
            playerInventory = FindFirstObjectByType<PlayerInventory>();
    }

    public void Interact()
    {
        if (collected || playerInventory == null) return;

        playerInventory.PickupHouseKey();
        collected = true;

        if (keyRenderer != null)
            keyRenderer.enabled = false;

        Debug.Log("You found the house key.");
    }
}