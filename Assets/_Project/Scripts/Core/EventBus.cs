using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Global Event Bus using ScriptableObject for decoupled communication.
/// Keys, Puzzles, Narrative, and UI can subscribe without direct references.
/// </summary>
[CreateAssetMenu(fileName = "EventBus", menuName = "LockeKey/Core/EventBus")]
public class EventBus : ScriptableObject
{
    // Key Events
    public event Action<IKeyAbility> OnKeyActivated;
    public event Action<IKeyAbility> OnKeyDeactivated;
    public event Action<KeyType> OnKeyDiscovered;

    // Puzzle Events
    public event Action<PuzzleBase> OnPuzzleSolved;
    public event Action<PuzzleBase> OnPuzzleFailed;

    // Narrative Events
    public event Action<string> OnDialogueTriggered;
    public event Action OnChapterCompleted;

    // Horror / Atmosphere Events
    public event Action<float> OnTensionChanged; // 0-1 scale
    public event Action OnEchoTriggered;
    public event Action OnEchoCaught;
    public event Action OnGhostPhaseStarted;
    public event Action OnGhostPhaseEnded;

    public void KeyActivated(IKeyAbility key) => OnKeyActivated?.Invoke(key);
    public void KeyDeactivated(IKeyAbility key) => OnKeyDeactivated?.Invoke(key);
    public void KeyDiscovered(KeyType keyType) => OnKeyDiscovered?.Invoke(keyType);

    public void PuzzleSolved(PuzzleBase puzzle) => OnPuzzleSolved?.Invoke(puzzle);
    public void PuzzleFailed(PuzzleBase puzzle) => OnPuzzleFailed?.Invoke(puzzle);

    public void TriggerDialogue(string dialogueId) => OnDialogueTriggered?.Invoke(dialogueId);
    public void ChapterCompleted() => OnChapterCompleted?.Invoke();

    public void SetTension(float tension) => OnTensionChanged?.Invoke(Mathf.Clamp01(tension));

    public void TriggerEcho() => OnEchoTriggered?.Invoke();

    public void EchoCaught() => OnEchoCaught?.Invoke();

    public void GhostPhaseStarted() => OnGhostPhaseStarted?.Invoke();

    public void GhostPhaseEnded() => OnGhostPhaseEnded?.Invoke();
}