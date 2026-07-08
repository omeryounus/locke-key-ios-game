using UnityEngine;

/// <summary>
/// Anything the player can tap / press E on when nearby.
/// </summary>
public interface IInteractable
{
    bool CanInteract { get; }
    string InteractionHint { get; }
    void Interact();
}