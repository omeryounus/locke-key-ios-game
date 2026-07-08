using UnityEngine;

/// <summary>
/// Non-magical items for tutorial puzzles (house keys, letters, etc.).
/// </summary>
public class PlayerInventory : MonoBehaviour
{
    public bool HasHouseKey { get; private set; }

    public void PickupHouseKey()
    {
        HasHouseKey = true;
        Debug.Log("Inventory: House key acquired.");
    }

    public void RestoreHouseKey(bool hasKey) => HasHouseKey = hasKey;
}