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
    private float nextBlockedHintTime;

    private void Awake()
    {
        if (beatDirector == null)
            beatDirector = FindFirstObjectByType<ChapterBeatDirector>();

        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() == null)
            return;

        TryEscape();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() == null)
            return;

        TryEscape();
    }

    private void TryEscape()
    {
        if (used) return;

        var beat = beatDirector != null ? beatDirector : FindFirstObjectByType<ChapterBeatDirector>();
        if (beat == null || beat.CurrentBeat != ChapterBeatDirector.Beat.EchoEncounter)
            return;

        var echoManager = FindFirstObjectByType<EchoEncounterManager>();
        if (echoManager == null || !echoManager.CanEscape)
        {
            if (Time.unscaledTime >= nextBlockedHintTime)
            {
                nextBlockedHintTime = Time.unscaledTime + 2f;
                FindFirstObjectByType<GameplayHUD>()?.ShowGuidanceToast(
                    "Break the Echo's gaze in the arch before you run.", 2.8f);
            }
            return;
        }

        used = true;
        echoManager.ClearEncounter();

        beat?.NotifyEchoEscaped();
        ChapterSaveManager.Instance?.RecordEchoCleared();
        FindFirstObjectByType<GameplayHUD>()?.ShowGuidanceToast(
            "Safe — for now. Find the Head Key beyond the passage.", 3.5f);
    }
}
