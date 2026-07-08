using UnityEngine;

/// <summary>
/// Base class for all environmental puzzles in Keyhouse.
/// Every puzzle should inherit from this to get consistent behavior.
/// </summary>
public abstract class PuzzleBase : MonoBehaviour, IInteractable
{
    [Header("Puzzle Settings")]
    public string puzzleID;
    public bool isSolved = false;
    public bool requiresSpecificKey = false;
    public KeyType requiredKeyType;

    protected EventBus eventBus;

    public virtual bool CanInteract => !isSolved;

    public virtual string InteractionHint =>
        isSolved
            ? string.Empty
            : requiresSpecificKey
                ? $"Requires the {requiredKeyType} Key — tap Interact"
                : "Tap Interact";

    protected virtual void Awake()
    {
        eventBus = Resources.Load<EventBus>("EventBus"); // Or use singleton
    }

    /// <summary>
    /// Called when player interacts with the puzzle.
    /// </summary>
    public virtual void Interact()
    {
        if (isSolved) return;

        if (requiresSpecificKey && !HasRequiredKey())
        {
            Debug.Log($"This puzzle requires the {requiredKeyType} Key.");
            return;
        }

        TrySolve();
    }

    protected abstract void TrySolve();

    protected virtual bool HasRequiredKey()
    {
        var keyManager = FindFirstObjectByType<KeyManager>();
        if (keyManager == null || keyManager.currentActiveKey == null)
            return false;

        return requiredKeyType switch
        {
            KeyType.Ghost => keyManager.currentActiveKey.abilityType == KeyManager.KeyAbilityType.GhostPhase,
            KeyType.Head => keyManager.currentActiveKey.abilityType == KeyManager.KeyAbilityType.HeadMemory,
            KeyType.Mirror => keyManager.currentActiveKey.abilityType == KeyManager.KeyAbilityType.MirrorTravel,
            KeyType.Anywhere => keyManager.currentActiveKey.abilityType == KeyManager.KeyAbilityType.AnywhereDoor,
            KeyType.Shadow => keyManager.currentActiveKey.abilityType == KeyManager.KeyAbilityType.ShadowManipulate,
            KeyType.Omega => keyManager.currentActiveKey.abilityType == KeyManager.KeyAbilityType.Omega,
            _ => true
        };
    }

    protected virtual void MarkAsSolved()
    {
        isSolved = true;
        eventBus?.PuzzleSolved(this);
        Debug.Log($"Puzzle solved: {puzzleID}");
    }

    protected virtual void OnPuzzleFailed()
    {
        eventBus?.PuzzleFailed(this);
    }
}