using UnityEngine;

/// <summary>
/// Chapter 1 critical path beats with clear objectives and end conditions.
/// Arrival → Door → Library → Ghost → Echo → Memory → Hidden → Complete
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
        Aftermath,      // after Echo escape — go get Head Key / memory
        MemorySolved,   // portrait done — hunt Hidden Key
        ChapterComplete
    }

    [SerializeField] private CameraFollow2D cameraFollow;
    [SerializeField] private GameplayHUD hud;
    [SerializeField] private TouchGameplayController gameplay;

    private Beat currentBeat = Beat.Arrival;
    private EventBus eventBus;
    private bool playerHasMoved;
    private bool houseKeyCollected;
    private bool memorySolved;
    private bool hiddenKeySolved;
    private bool echoEscaped;

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
            hud?.SetControlVisibility(move: true, interact: true, jump: true, useKey: false);
            hud?.ShowToast("Move with Left/Right. Interact near glowing objects.", 3.5f);
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

    public void NotifyMemorySolved()
    {
        memorySolved = true;
        if ((int)currentBeat < (int)Beat.MemorySolved)
            AdvanceTo(Beat.MemorySolved);
        TryFinishChapter();
    }

    public void NotifyHiddenKeySolved()
    {
        hiddenKeySolved = true;
        TryFinishChapter();
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
            case "chapter1_memory_fragment":
                NotifyMemorySolved();
                break;
            case "chapter1_hidden_key":
                NotifyHiddenKeySolved();
                break;
        }
    }

    private void HandleGhostPhaseEnded()
    {
        // First phase near sealed door can still be mid-solve; Echo is driven by sealed door solve.
    }

    public void NotifyEchoEscaped()
    {
        echoEscaped = true;
        if (currentBeat == Beat.EchoEncounter)
            AdvanceTo(Beat.Aftermath);
        TryFinishChapter();
    }

    public void NotifyEchoCaught()
    {
        if (currentBeat != Beat.EchoEncounter) return;
        hud?.ShowUrgentToast("The Echo caught you — try hiding in the arch!", 3f);
    }

    public void RestoreFromSave(int beatIndex)
    {
        var beat = (Beat)Mathf.Clamp(beatIndex, 0, (int)Beat.ChapterComplete);
        houseKeyCollected = beatIndex >= (int)Beat.StuckDoor;
        playerHasMoved = beatIndex > (int)Beat.Arrival;
        echoEscaped = beatIndex >= (int)Beat.Aftermath;
        memorySolved = beatIndex >= (int)Beat.MemorySolved;
        hiddenKeySolved = beatIndex >= (int)Beat.ChapterComplete;
        ApplyBeat(beat, announce: false);

        if (beatIndex > (int)Beat.Arrival)
            cameraFollow?.EndArrivalIntro();
    }

    private void TryFinishChapter()
    {
        // Chapter complete when: escaped Echo AND solved memory AND found hidden/mirror key
        // Soft path: if memory + hidden done after aftermath, finish.
        if (!echoEscaped && currentBeat != Beat.Aftermath && currentBeat != Beat.MemorySolved)
            return;

        if (memorySolved && hiddenKeySolved)
        {
            AdvanceTo(Beat.ChapterComplete);
            eventBus?.ChapterCompleted();
        }
        else if (memorySolved && !hiddenKeySolved && currentBeat == Beat.MemorySolved)
        {
            hud?.ShowToast("Ghost-phase the sealed passage wall — a key waits inside.", 3.5f);
        }
        else if (hiddenKeySolved && !memorySolved)
        {
            hud?.ShowToast("The Mirror Key is yours. Still — the portrait hums for the Head Key.", 3.5f);
        }
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
                hud?.SetControlVisibility(move: true, interact: true, jump: true, useKey: false);
                if (announce)
                    hud?.ShowGuidanceToast("Follow the trail to the House Key.", 2.8f);
                break;
            case Beat.StuckDoor:
                hud?.SetControlVisibility(move: true, interact: true, jump: true, useKey: false);
                if (announce)
                {
                    hud?.ShowGuidanceToast("Walk to the highlighted Front Door and Interact.", 3f);
                    hud?.FlashInteractButton(1.5f);
                    FindFirstObjectByType<ObjectiveTrackerHUD>()?.Peek();
                }
                break;
            case Beat.Library:
                hud?.SetControlVisibility(move: true, interact: true, jump: true, useKey: false);
                if (announce)
                {
                    hud?.ShowGuidanceToast("Library: inspect the collapsed shelf, then shove it free.", 3.2f);
                    FindFirstObjectByType<ObjectiveTrackerHUD>()?.Peek();
                }
                break;
            case Beat.GhostKeyUse:
                hud?.SetControlVisibility(move: true, interact: true, jump: true, useKey: true);
                if (announce)
                {
                    hud?.ShowGuidanceToast("Sealed door: equip Ghost Key, Use Key, then walk through.", 3.5f);
                    FindFirstObjectByType<ObjectiveTrackerHUD>()?.Peek();
                }
                break;
            case Beat.EchoEncounter:
                hud?.SetControlVisibility(move: true, interact: true, jump: true, useKey: true);
                if (announce)
                    hud?.ShowGuidanceToast("An Echo hunts you — hide in the arch, then escape the passage!", 4.5f);
                eventBus?.SetTension(0.9f);
                break;
            case Beat.Aftermath:
                hud?.SetControlVisibility(move: true, interact: true, jump: true, useKey: true);
                if (announce)
                    hud?.ShowGuidanceToast("You escaped… Claim the Head Key, then open the portrait Mindscape.", 4f);
                eventBus?.SetTension(0.25f);
                break;
            case Beat.MemorySolved:
                hud?.SetControlVisibility(move: true, interact: true, jump: true, useKey: true);
                if (announce)
                    hud?.ShowGuidanceToast("Use Ghost Key at the glowing wall in the sealed passage.", 4f);
                break;
            case Beat.ChapterComplete:
                hud?.SetControlVisibility(move: true, interact: true, jump: true, useKey: true);
                if (announce)
                    hud?.ShowGuidanceToast("Chapter 1 complete — the Black Door waits…", 4f);
                eventBus?.SetTension(0.1f);
                break;
        }
    }
}
