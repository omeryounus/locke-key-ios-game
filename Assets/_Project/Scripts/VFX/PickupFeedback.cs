using UnityEngine;

/// <summary>
/// Key pickup feedback: haptic, SFX, camera pulse, toast.
/// </summary>
public static class PickupFeedback
{
    public static void PlayKeyPickup(string message)
    {
        GameHaptics.KeyPickup();
        var audio = Object.FindFirstObjectByType<GameAudioController>();
        audio?.PlayKeyPickup();
        Object.FindFirstObjectByType<CameraFollow2D>()?.Pulse(0.22f, 0.28f);
        Object.FindFirstObjectByType<CameraFollow2D>()?.Shake(0.06f, 0.18f);
        Object.FindFirstObjectByType<GameplayHUD>()?.ShowToast(message, 3.2f);
        Object.FindFirstObjectByType<KeySlotHUD>()?.FlashDiscovered();
        Object.FindFirstObjectByType<InventoryStripHUD>()?.PlayCollectPop();
        var player = Object.FindFirstObjectByType<PlayerController>();
        var pos = player != null ? player.transform.position : Vector3.zero;
        Object.FindFirstObjectByType<ParticleVFXController>()?.PlayMemoryBurst(pos);
    }
}