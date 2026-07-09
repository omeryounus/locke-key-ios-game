/// <summary>
/// Data bag passed to GrokUIFlowManager.ShowLock().
/// Describes which key is required, which hotspot is being solved,
/// and which room to unlock on success.
/// </summary>
[System.Serializable]
public class LockDefinition
{
    /// <summary>Key catalog id required to unlock this door (e.g. "anywhere").</summary>
    public string requiredKeyId;

    /// <summary>
    /// Hotspot id written to solvedHotspotIds on success
    /// (e.g. "foyer_stair_door"). Must match design doc IDs exactly.
    /// </summary>
    public string hotspotId;

    /// <summary>Room id written to unlockedRoomIds on success (e.g. "wellhouse").</summary>
    public string unlockRoomId;

    // ── Pre-built definitions ────────────────────────────────────────────

    /// <summary>The stair door in the Foyer — requires the Anywhere Key.</summary>
    public static readonly LockDefinition FoyerStairDoor = new()
    {
        requiredKeyId = "anywhere",
        hotspotId     = "foyer_stair_door",
        unlockRoomId  = "wellhouse"
    };
}
