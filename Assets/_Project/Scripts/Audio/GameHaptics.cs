using UnityEngine;

/// <summary>
/// Light iOS haptics for key gameplay beats.
/// </summary>
public static class GameHaptics
{
    public static void KeyPickup() => PlayLight();
    public static void Unlock() => PlayLight();
    public static void PhaseStart() => PlayMedium();
    public static void EchoContact() => PlayHeavy();
    public static void ColdPhase() => PlayMedium();

    private static void PlayLight()
    {
#if UNITY_IOS && !UNITY_EDITOR
        Handheld.Vibrate();
#endif
    }

    private static void PlayMedium()
    {
#if UNITY_IOS && !UNITY_EDITOR
        Handheld.Vibrate();
#endif
    }

    private static void PlayHeavy()
    {
#if UNITY_IOS && !UNITY_EDITOR
        Handheld.Vibrate();
#endif
    }
}