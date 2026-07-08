using UnityEngine;

/// <summary>
/// Drives Chapter 1 storyboard beats: arrival tutorial, library flow, echo tension.
/// </summary>
public class ChapterBeatDirector : MonoBehaviour
{
    public enum Beat
    {
        Arrival,
        StuckDoor,
        Library,
        GhostKeyUse,
        EchoEncounter,
        Aftermath
    }

    [SerializeField] private CameraFollow2D cameraFollow;
    [SerializeField] private GameplayHUD hud;
    [SerializeField] private TouchGameplayController gameplay;

    private Beat currentBeat = Beat.Arrival;
    private EventBus eventBus;
    private bool playerHasMoved;
    private bool houseKeyCollected;

    public Beat CurrentBeat => currentBeat;

    private void Awake()
    {
        eventBus = Resources.Load<EventBus>("EventBus");
        if (cameraFollow == null)
            cameraFollow = FindFirstObjectByType<CameraFollow2D>();
        if (hud == null)
            hud = FindFirstObjectByType<GameplayHUD>();
        if (gameplay == null)
            gameplay = FindFirstObjectByType<TouchGameplayController>();

        if (eventBus != null)
        {
            eventBus.OnPuzzleSolved += HandlePuzzleSolved;
            eventBus.OnGhostPhaseEnded += HandleGhostPhaseEnded;
            eventBus.OnEchoCaught += NotifyEchoCaught;
        }

        ApplyBeat(Beat.Arrival, announce: false);
        cameraFollow?.BeginArrivalIntro();
    }

    private void OnDestroy()
    {
        if (eventBus == null) return;
        eventBus.OnPuzzleSolved -= HandlePuzzleSolved;
        eventBus.OnGhostPhaseEnded -= HandleGhostPhaseEnded;
        eventBus.OnEchoCaught -= NotifyEchoCaught;
    }

    private void Update()
    {
        if (currentBeat != Beat.Arrival || playerHasMoved || gameplay == null)
            return;

        if (Mathf.Abs(gameplay.MoveInput) > 0.01f)
        {
            playerHasMoved = true;
            cameraFollow?.EndArrivalIntro();
            hud?.SetControlVisibility(interact: true, jump: true);
            hud?.ShowToast("Tap Interact near glowing objects.", 3.5f);
        }
    }

    public void NotifyHouseKeyCollected()
    {
        houseKeyCollected = true;
        if (currentBeat == Beat.Arrival)
            AdvanceTo(Beat.StuckDoor);
    }

    public void NotifyGhostKeyCollected()
    {
        AdvanceTo(Beat.GhostKeyUse);
    }

    private void HandlePuzzleSolved(PuzzleBase puzzle)
    {
        if (puzzle == null) return;

        switch (puzzle.puzzleID)
        {
            case "chapter1_stuck_door":
                AdvanceTo(Beat.Library);
                break;
            case "chapter1_bookshelf":
                hud?.ShowToast("Claim the Ghost Key from the alcove.", 4f);
                break;
            case "chapter1_sealed_door":
                AdvanceTo(Beat.EchoEncounter);
                break;
        }
    }

    private void HandleGhostPhaseEnded()
    {
        if (currentBeat == Beat.GhostKeyUse)
            AdvanceTo(Beat.EchoEncounter);
    }

    public void NotifyEchoEscaped()
    {
        if (currentBeat == Beat.EchoEncounter)
            AdvanceTo(Beat.Aftermath);
    }

    public void NotifyEchoCaught()
    {
        if (currentBeat != Beat.EchoEncounter) return;
        hud?.ShowToast("The Echo found you — hide or run!", 3f);
        eventBus?.SetTension(0.98f);
    }

    public void RestoreFromSave(int beatIndex)
    {
        var beat = (Beat)Mathf.Clamp(beatIndex, 0, (int)Beat.Aftermath);
        houseKeyCollected = beatIndex >= (int)Beat.StuckDoor;
        playerHasMoved = beatIndex > (int)Beat.Arrival;
        ApplyBeat(beat, announce: false);

        if (beatIndex > (int)Beat.Arrival)
            cameraFollow?.EndArrivalIntro();
    }

    private void AdvanceTo(Beat beat)
    {
        if ((int)beat <= (int)currentBeat && beat != Beat.Arrival)
            return;

        ApplyBeat(beat, announce: true);
        ChapterSaveManager.Instance?.RecordBeat((int)beat);
    }

    private void ApplyBeat(Beat beat, bool announce)
    {
        currentBeat = beat;

        switch (beat)
        {
            case Beat.Arrival:
                hud?.SetControlVisibility(interact: false, jump: false, useKey: false);
                hud?.ShowToast("Keyhouse — move toward the glinting key.", 4f);
                break;
            case Beat.StuckDoor:
                hud?.SetControlVisibility(useKey: false);
                if (announce)
                    hud?.ShowToast("Unlock the stuck door with the house key.", 3.5f);
                break;
            case Beat.Library:
                if (announce)
                    hud?.ShowToast("Push the collapsed bookshelf to reveal the alcove.", 4f);
                break;
            case Beat.GhostKeyUse:
                hud?.SetControlVisibility(useKey: true);
                if (announce)
                    hud?.ShowToast("Sealed door ahead — tap Use Key to phase through.", 4f);
                break;
            case Beat.EchoEncounter:
                hud?.SetControlVisibility(useKey: true);
                if (announce)
                    hud?.ShowToast("The Echo is drawn to you — hide behind the arch or run.", 4.5f);
                eventBus?.SetTension(0.9f);
                break;
            case Beat.Aftermath:
                if (announce)
                    hud?.ShowToast("You escaped the Echo... for now.", 3.5f);
                eventBus?.SetTension(0.2f);
                break;
        }
    }
}