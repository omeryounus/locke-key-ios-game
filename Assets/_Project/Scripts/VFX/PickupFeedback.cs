using UnityEngine;

/// <summary>
/// Lightweight pickup feedback (log + optional device haptic on iOS builds).
/// </summary>
public static class PickupFeedback
{
    public static void PlayKeyPickup(string message)
    {
        Debug.Log(message);
#if UNITY_IOS && !UNITY_EDITOR
        Handheld.Vibrate();
#endif
    }
}