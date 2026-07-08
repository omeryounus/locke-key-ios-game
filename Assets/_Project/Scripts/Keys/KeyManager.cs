using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Core Key Management System for Locke & Key iOS Game
/// Handles key collection, activation, and ability triggering.
/// This is a foundational prototype script.
/// </summary>
public class KeyManager : MonoBehaviour
{
    [Header("Key Data")]
    public List<KeyData> ownedKeys = new List<KeyData>();
    public KeyData currentActiveKey;

    [Header("References")]
    public PlayerController player;           // Reference to player movement/interaction
    public UIManager uiManager;               // For updating key UI

    // Event for when a key is used (can trigger puzzles, horror, etc.)
    public delegate void KeyUsedEvent(KeyData key);
    public event KeyUsedEvent OnKeyUsed;

    /// <summary>
    /// Represents a single magical key and its properties.
    /// </summary>
    [System.Serializable]
    public class KeyData
    {
        public string keyName;                    // e.g., "Ghost Key"
        public string description;                // Flavor text
        public KeyAbilityType abilityType;        // What it does
        public bool isActive = false;             // Currently selected?
        public int usesRemaining = -1;            // -1 = unlimited, otherwise limited uses
        public float cooldown = 0f;               // Cooldown between uses
        public bool hasRisk = false;              // Does using this attract demons?
        public float riskLevel = 0f;              // 0-1 scale of danger
    }

    public enum KeyAbilityType
    {
        GhostPhase,        // Phase through objects
        HeadMemory,        // Enter minds / view memories
        MirrorTravel,      // Travel through reflections
        AnywhereDoor,      // Create temporary doors
        ShadowManipulate,  // Control shadows
        Omega              // Ultimate / late-game
    }

    public void GrantGhostKey() => EnsureGhostKey(silent: false);

    public void GrantHeadKey() => EnsureHeadKey(silent: false);

    private void EnsureGhostKey(bool silent)
    {
        if (ownedKeys.Exists(k => k.abilityType == KeyAbilityType.GhostPhase))
            return;

        var ghostKey = new KeyData
        {
            keyName = "Ghost Key",
            description = "Allows the bearer to phase through solid matter for a short time.",
            abilityType = KeyAbilityType.GhostPhase,
            usesRemaining = -1,
            cooldown = 8f,
            hasRisk = true,
            riskLevel = 0.3f
        };

        if (silent)
            ownedKeys.Add(ghostKey);
        else
            DiscoverNewKey(ghostKey);

        SelectKey(ghostKey);
    }

    private void EnsureHeadKey(bool silent)
    {
        if (ownedKeys.Exists(k => k.abilityType == KeyAbilityType.HeadMemory))
            return;

        var headKey = new KeyData
        {
            keyName = "Head Key",
            description = "Unlocks the mind. Use to explore memories and thoughts.",
            abilityType = KeyAbilityType.HeadMemory,
            usesRemaining = -1,
            cooldown = 12f,
            hasRisk = true,
            riskLevel = 0.5f
        };

        if (silent)
            ownedKeys.Add(headKey);
        else
            DiscoverNewKey(headKey);

        SelectKey(headKey);
    }

    /// <summary>
    /// Call this when player selects a key from inventory/UI.
    /// </summary>
    public void SelectKey(KeyData key)
    {
        if (!ownedKeys.Contains(key)) return;

        // Deactivate previous key
        if (currentActiveKey != null)
            currentActiveKey.isActive = false;

        currentActiveKey = key;
        currentActiveKey.isActive = true;

        Debug.Log($"Selected: {key.keyName}");
        uiManager?.UpdateActiveKeyUI(key);
    }

    /// <summary>
    /// Main method called when player uses the currently active key (e.g., tap "Use Key" button).
    /// </summary>
    public void UseActiveKey()
    {
        if (currentActiveKey == null)
        {
            Debug.LogWarning("No key selected!");
            return;
        }

        if (currentActiveKey.usesRemaining == 0)
        {
            Debug.Log("This key has no uses left.");
            return;
        }

        if (currentActiveKey.abilityType == KeyAbilityType.GhostPhase)
        {
            var sealedDoor = FindFirstObjectByType<SealedDoorPuzzle>();
            if (sealedDoor != null && !sealedDoor.isSolved && !sealedDoor.IsPlayerInRange())
            {
                Debug.Log("Move closer to the sealed door before using the Ghost Key.");
                return;
            }
        }

        // Trigger the specific ability
        ActivateAbility(currentActiveKey.abilityType);

        // Handle uses and risk
        if (currentActiveKey.usesRemaining > 0)
            currentActiveKey.usesRemaining--;

        // Echo spawn is scripted after ghost phase ends near the sealed door (Beat 5).
        if (currentActiveKey.hasRisk && currentActiveKey.abilityType != KeyAbilityType.GhostPhase)
            CheckForHorrorConsequence(currentActiveKey.riskLevel);

        // Notify listeners (puzzle system, horror manager, etc.)
        OnKeyUsed?.Invoke(currentActiveKey);

        // Cooldown handling would go here in a full implementation
    }

    private void ActivateAbility(KeyAbilityType ability)
    {
        switch (ability)
        {
            case KeyAbilityType.GhostPhase:
                player?.ActivateGhostPhase(duration: 5f);
                break;

            case KeyAbilityType.HeadMemory:
                // Trigger memory viewing UI or puzzle
                uiManager?.OpenMemoryView();
                break;

            case KeyAbilityType.MirrorTravel:
                player?.TryMirrorTravel();
                break;

            case KeyAbilityType.AnywhereDoor:
                // Open door creation UI or raycast to place door
                Debug.Log("Anywhere Key activated - Door creation mode");
                break;

            case KeyAbilityType.ShadowManipulate:
                player?.ManipulateShadows();
                break;

            case KeyAbilityType.Omega:
                Debug.Log("Omega Key used - Major story consequences!");
                // Trigger end-game sequence
                break;
        }
    }

    private void CheckForHorrorConsequence(float risk)
    {
        var eventBus = Resources.Load<EventBus>("EventBus");
        float roll = Random.Range(0f, 1f);
        if (roll < risk)
        {
            Debug.Log("Risk triggered! A demonic Echo has been attracted...");
            eventBus?.TriggerEcho();
        }
    }

    /// <summary>
    /// Add a newly discovered key to the player's collection.
    /// </summary>
    public void DiscoverNewKey(KeyData newKey)
    {
        if (!ownedKeys.Contains(newKey))
        {
            ownedKeys.Add(newKey);
            Debug.Log($"New key discovered: {newKey.keyName}");
            uiManager?.ShowKeyDiscoveryNotification(newKey);
            Resources.Load<EventBus>("EventBus")?.KeyDiscovered(MapAbilityToKeyType(newKey.abilityType));
            ChapterSaveManager.Instance?.SaveNow();
        }
    }

    public void RestoreFromSave(ChapterSaveData save)
    {
        if (save == null) return;

        if (save.hasGhostKey)
            EnsureGhostKey(silent: true);
        if (save.hasHeadKey)
            EnsureHeadKey(silent: true);

        if (!string.IsNullOrEmpty(save.activeKeyAbility))
        {
            var match = ownedKeys.Find(k => k.abilityType.ToString() == save.activeKeyAbility);
            if (match != null)
                SelectKey(match);
        }
    }

    public void CaptureToSave(ChapterSaveData save)
    {
        if (save == null) return;

        save.hasGhostKey = ownedKeys.Exists(k => k.abilityType == KeyAbilityType.GhostPhase);
        save.hasHeadKey = ownedKeys.Exists(k => k.abilityType == KeyAbilityType.HeadMemory);
        save.ghostKeyRevealed = save.ghostKeyRevealed
            || ChapterSaveManager.Instance?.IsPuzzleSolved("chapter1_bookshelf") == true;
        save.activeKeyAbility = currentActiveKey != null
            ? currentActiveKey.abilityType.ToString()
            : string.Empty;
    }

    private static KeyType MapAbilityToKeyType(KeyAbilityType ability) =>
        ability switch
        {
            KeyAbilityType.GhostPhase => KeyType.Ghost,
            KeyAbilityType.HeadMemory => KeyType.Head,
            KeyAbilityType.MirrorTravel => KeyType.Mirror,
            KeyAbilityType.AnywhereDoor => KeyType.Anywhere,
            KeyAbilityType.ShadowManipulate => KeyType.Shadow,
            KeyAbilityType.Omega => KeyType.Omega,
            _ => KeyType.Ghost
        };
}