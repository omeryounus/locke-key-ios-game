using UnityEngine;

/// <summary>
/// Puzzle 4 — Head Key on family portrait opens Mindscape chronology puzzle.
/// Solving grants lore that points toward the Hidden Key wall.
/// </summary>
public class MemoryFragmentPuzzle : PuzzleBase
{
    [SerializeField] private SpriteRenderer portraitRenderer;
    [SerializeField] private UIManager uiManager;

    public override bool CanInteract => !isSolved;

    public override string InteractionHint =>
        isSolved
            ? string.Empty
            : HasHeadKey()
                ? "Family portrait — Use Head Key or Interact to open Mindscape"
                : "Family portrait — needs the Head Key";

    protected override void Awake()
    {
        base.Awake();
        puzzleID = "chapter1_memory_fragment";
        requiresSpecificKey = false; // we handle Head Key checks manually for better toasts
        requiredKeyType = KeyType.Head;

        if (uiManager == null)
            uiManager = FindFirstObjectByType<UIManager>();
    }

    public override void RestoreSolvedState()
    {
        base.RestoreSolvedState();
        if (portraitRenderer != null)
            portraitRenderer.color = new Color(0.75f, 0.55f, 0.95f, 1f);
    }

    public override void Interact() => TrySolve();

    protected override void TrySolve()
    {
        if (isSolved) return;
        var hud = FindFirstObjectByType<GameplayHUD>();

        if (!HasHeadKey())
        {
            hud?.ShowToast("The portrait hums… it wants the Head Key.", 3f);
            FindFirstObjectByType<GameAudioController>()?.PlayDoorRattle();
            OnPuzzleFailed();
            return;
        }

        // Auto-select Head Key
        var km = FindFirstObjectByType<KeyManager>();
        if (km != null)
        {
            var head = km.ownedKeys.Find(k => k.abilityType == KeyManager.KeyAbilityType.HeadMemory);
            if (head != null) km.SelectKey(head);
        }

        var headKey = FindFirstObjectByType<HeadKey>();
        if (headKey != null && !headKey.CanActivate())
        {
            hud?.ShowToast("The Head Key is cooling down.", 2.5f);
            OnPuzzleFailed();
            return;
        }

        // Cooldown without opening plain text view
        if (headKey != null)
            headKey.BeginCooldownOnly();

        FindFirstObjectByType<CameraFollow2D>()?.Pulse(0.3f, 0.4f);
        GameHaptics.PhaseStart();
        FindFirstObjectByType<GameAudioController>()?.PlayMemoryTransition();

        var touchController = FindFirstObjectByType<TouchGameplayController>();
        if (touchController != null)
            touchController.SetInputLocked(true);

        var player = FindFirstObjectByType<PlayerController>();
        if (player != null) player.IsInteracting = true;

        FindFirstObjectByType<PlayerSpriteAnimator>()?.PlayExpression(
            PlayerSpriteAnimator.AnimState.Happy, 0.8f);

        var mindscapeGo = new GameObject("HeadKeyMindscape");
        var mindscapePanel = mindscapeGo.AddComponent<HeadKeyMindscapePanel>();
        mindscapePanel.Initialize(uiManager);
        hud?.ShowToast("Mindscape opens — restore the timeline.", 2.5f);
    }

    public void SolveFromUI()
    {
        if (isSolved) return;
        if (portraitRenderer != null)
            portraitRenderer.color = new Color(0.75f, 0.55f, 0.95f, 1f);

        FindFirstObjectByType<GameplayHUD>()?.ShowToast(
            "Memory: Rendell hid a key in the sealed passage wall — use the Ghost Key to reach it.", 4.5f);
        FindFirstObjectByType<CameraFollow2D>()?.Pulse(0.18f, 0.25f);
        MarkAsSolved();

        // Advance story toward Hidden Key
        FindFirstObjectByType<ChapterBeatDirector>()?.NotifyMemorySolved();
    }

    private static bool HasHeadKey()
    {
        var keyManager = FindFirstObjectByType<KeyManager>();
        return keyManager != null && keyManager.ownedKeys
            .Exists(k => k.abilityType == KeyManager.KeyAbilityType.HeadMemory);
    }
}
