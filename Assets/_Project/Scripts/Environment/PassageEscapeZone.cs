using UnityEngine;

/// <summary>
/// Safe passage exit during the Echo encounter.
/// Clears Echo and advances Aftermath — does NOT auto-end Chapter 1.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PassageEscapeZone : MonoBehaviour
{
    [SerializeField] private ChapterBeatDirector beatDirector;
    private bool used;

    private void Awake()
    {
        if (beatDirector == null)
            beatDirector = FindFirstObjectByType<ChapterBeatDirector>();

        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (used) return;
        if (other.GetComponent<PlayerController>() == null)
            return;

        var beat = beatDirector != null ? beatDirector : FindFirstObjectByType<ChapterBeatDirector>();
        // Only meaningful during Echo encounter
        if (beat != null &&
            beat.CurrentBeat != ChapterBeatDirector.Beat.EchoEncounter &&
            beat.CurrentBeat != ChapterBeatDirector.Beat.GhostKeyUse)
        {
            // Still clear stragglers
        }

        used = true;
        var echoManager = FindFirstObjectByType<EchoEncounterManager>();
        if (echoManager != null)
            echoManager.ClearEncounter();
        else
        {
            var echoes = FindObjectsByType<EchoEntity>(FindObjectsSortMode.None);
            foreach (var echo in echoes)
                Destroy(echo.gameObject);
        }

        beat?.NotifyEchoEscaped();
        ChapterSaveManager.Instance?.RecordEchoCleared();
        FindFirstObjectByType<GameplayHUD>()?.ShowToast(
            "Safe — for now. Find the Head Key beyond the passage.", 3.5f);
    }
}
