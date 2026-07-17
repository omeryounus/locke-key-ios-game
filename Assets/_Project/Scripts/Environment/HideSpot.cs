using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Foreground cover where the player can break Echo line-of-sight (Beat 5).
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class HideSpot : MonoBehaviour
{
    private static readonly HashSet<Collider2D> HiddenPlayers = new();

    private readonly HashSet<Collider2D> localOccupants = new();

    public static bool IsPlayerHidden => HiddenPlayers.Count > 0;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        HiddenPlayers.Clear();
    }

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnDisable()
    {
        ClearLocalOccupants();
    }

    private void OnDestroy()
    {
        ClearLocalOccupants();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponent<PlayerController>();
        if (player == null) return;

        var col = player.GetComponent<Collider2D>();
        if (col == null || !localOccupants.Add(col)) return;

        HiddenPlayers.Add(col);
        if (HiddenPlayers.Count == 1)
        {
            var beat = FindFirstObjectByType<ChapterBeatDirector>();
            if (beat != null && beat.CurrentBeat == ChapterBeatDirector.Beat.EchoEncounter)
            {
                FindFirstObjectByType<EchoEncounterManager>()?.MarkHideSpotUsed();
                FindFirstObjectByType<GameplayHUD>()?.ShowGuidanceToast(
                    "The Echo lost your trail. Run for the passage.", 2.8f);
            }
            GameHaptics.TriggerHapticLight();
            FindFirstObjectByType<GameAudioController>()?.SetMuffled(true);
            Resources.Load<EventBus>("EventBus")?.HideEntered();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var player = other.GetComponent<PlayerController>();
        if (player == null) return;

        var col = player.GetComponent<Collider2D>();
        if (col == null) return;

        RemoveOccupant(col);
        if (HiddenPlayers.Count == 0)
        {
            FindFirstObjectByType<GameAudioController>()?.SetMuffled(false);
            Resources.Load<EventBus>("EventBus")?.HideExited();
        }
    }

    private void ClearLocalOccupants()
    {
        foreach (var col in localOccupants)
            HiddenPlayers.Remove(col);
        localOccupants.Clear();
    }

    private void RemoveOccupant(Collider2D col)
    {
        if (!localOccupants.Remove(col)) return;
        HiddenPlayers.Remove(col);
    }
}
