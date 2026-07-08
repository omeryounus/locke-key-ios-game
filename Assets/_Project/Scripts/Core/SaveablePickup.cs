using UnityEngine;

/// <summary>
/// Pickup that registers a stable ID with <see cref="ChapterSaveManager"/>.
/// </summary>
public abstract class SaveablePickup : MonoBehaviour
{
    [SerializeField] protected string pickupId;

    public string PickupId => pickupId;

    protected bool Collected { get; private set; }

    protected virtual void Awake()
    {
        if (string.IsNullOrEmpty(pickupId))
            pickupId = gameObject.name;
    }

    public virtual void RestoreFromSave(ChapterSaveManager save)
    {
        if (save == null || !save.IsPickupCollected(pickupId)) return;

        Collected = true;
        ApplyCollectedVisuals();
    }

    protected void MarkCollected()
    {
        Collected = true;
        ChapterSaveManager.Instance?.RecordPickupCollected(pickupId);
        ApplyCollectedVisuals();
    }

    protected abstract void ApplyCollectedVisuals();
}