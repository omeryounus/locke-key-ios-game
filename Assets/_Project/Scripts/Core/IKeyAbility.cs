using UnityEngine;

/// <summary>
/// Interface that all magical keys must implement.
/// This allows the KeyManager to treat every key the same way
/// while each key can have completely unique behavior.
/// </summary>
public interface IKeyAbility
{
    string KeyName { get; }
    string Description { get; }
    KeyType Type { get; }

    /// <summary>
    /// Called when the player activates this key.
    /// </summary>
    void Activate();

    /// <summary>
    /// Called when the key is deactivated or unequipped.
    /// </summary>
    void Deactivate();

    /// <summary>
    /// Returns whether the key can currently be used.
    /// </summary>
    bool CanActivate();
}

public enum KeyType
{
    Ghost,
    Head,
    Mirror,
    Anywhere,
    Shadow,
    Omega
}