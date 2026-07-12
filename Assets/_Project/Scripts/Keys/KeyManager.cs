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

    /// <summary>Grant without discovery notification — used after S4 sheet confirms.</summary>
    public void GrantGhostKeySilent() => EnsureGhostKey(silent: true);

    /// <summary>Grant without discovery notification — used after S4 sheet confirms.</summary>
    public void GrantHeadKeySilent() => EnsureHeadKey(silent: true);

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

    public void GrantMirrorKey() => EnsureMirrorKey(silent: false);
    public void GrantMirrorKeySilent() => EnsureMirrorKey(silent: true);

    private void EnsureMirrorKey(bool silent)
    {
        if (ownedKeys.Exists(k => k.abilityType == KeyAbilityType.MirrorTravel))
            return;

        var mirrorKey = new KeyData
        {
            keyName = "Mirror Key",
            description = "Travel through reflective surfaces and explore reflections.",
            abilityType = KeyAbilityType.MirrorTravel,
            usesRemaining = -1,
            cooldown = 10f,
            hasRisk = true,
            riskLevel = 0.4f
        };

        if (silent)
            ownedKeys.Add(mirrorKey);
        else
            DiscoverNewKey(mirrorKey);

        SelectKey(mirrorKey);
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
        if (player == null)
            player = FindFirstObjectByType<PlayerController>();
        var hud = FindFirstObjectByType<GameplayHUD>();

        if (currentActiveKey == null)
        {
            hud?.ShowToast("No key equipped. Open Key Ring to equip one.", 2.8f);
            GameHaptics.TriggerHapticLight();
            return;
        }

        if (currentActiveKey.usesRemaining == 0)
        {
            hud?.ShowToast("This key has no uses left.", 2.5f);
            return;
        }

        if (currentActiveKey.abilityType == KeyAbilityType.GhostPhase)
        {
            if (player != null && player.IsGhostPhasing)
            {
                hud?.ShowToast("Already phasing…", 1.5f);
                return;
            }

            var sealedDoor = FindFirstObjectByType<SealedDoorPuzzle>();
            var hidden = FindFirstObjectByType<HiddenKeyPuzzle>();
            bool freePhase = sealedDoor == null || sealedDoor.isSolved;
            bool nearSealed = sealedDoor != null && sealedDoor.IsPlayerInRange();
            bool nearHidden = hidden != null && !hidden.isSolved &&
                              Vector2.Distance(player != null ? player.transform.position : Vector3.zero,
                                  hidden.transform.position) < 3.2f;

            // Teach first use at sealed door; after that allow phase for Hidden Key etc.
            if (!freePhase && !nearSealed)
            {
                hud?.ShowToast("Stand next to the sealed door, then tap Use Key.", 3f);
                FindFirstObjectByType<GameAudioController>()?.PlayDoorRattle();
                return;
            }

            if (!freePhase && nearSealed)
                hud?.ShowToast("Phasing… walk through the sealed door!", 2.6f);
            else if (nearHidden)
                hud?.ShowToast("Phasing… reach into the glowing brickwork!", 2.6f);
            else
                hud?.ShowToast("Ghost phase active — move quickly.", 2.2f);
        }

        if (currentActiveKey.abilityType == KeyAbilityType.HeadMemory)
        {
            // Head Key Use opens mindscape only at the portrait puzzle
            var portrait = FindFirstObjectByType<MemoryFragmentPuzzle>();
            if (portrait != null && !portrait.isSolved && portrait.CanInteract)
            {
                float d = player != null
                    ? Vector2.Distance(player.transform.position, portrait.transform.position)
                    : 99f;
                if (d <= 3.5f)
                {
                    portrait.Interact();
                    OnKeyUsed?.Invoke(currentActiveKey);
                    return;
                }
            }
            hud?.ShowToast("Stand by the family portrait, then Use the Head Key.", 3f);
            return;
        }

        ActivateAbility(currentActiveKey.abilityType);
        GameHaptics.TriggerHapticLight();

        if (currentActiveKey.usesRemaining > 0)
            currentActiveKey.usesRemaining--;

        OnKeyUsed?.Invoke(currentActiveKey);
    }

    private void ActivateAbility(KeyAbilityType ability)
    {
        if (player == null)
            player = FindFirstObjectByType<PlayerController>();

        switch (ability)
        {
            case KeyAbilityType.GhostPhase:
                player?.ActivateGhostPhase(5.5f);
                break;

            case KeyAbilityType.HeadMemory:
                // Portrait path handled in UseActiveKey
                FindFirstObjectByType<GameplayHUD>()?.ShowToast("The Head Key needs a mind to open.", 2.5f);
                break;

            case KeyAbilityType.MirrorTravel:
                player?.TryMirrorTravel();
                break;

            case KeyAbilityType.AnywhereDoor:
                FindFirstObjectByType<GameplayHUD>()?.ShowToast("The Anywhere Key sleeps… not in this chapter.", 2.8f);
                break;

            case KeyAbilityType.ShadowManipulate:
                player?.ManipulateShadows();
                break;

            case KeyAbilityType.Omega:
                FindFirstObjectByType<GameplayHUD>()?.ShowToast("The Omega Key is sealed beyond Chapter 1.", 2.8f);
                break;
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
        if (save.discoveredKeyIds.Contains("mirror"))
            EnsureMirrorKey(silent: true);

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

        bool hasMirror = ownedKeys.Exists(k => k.abilityType == KeyAbilityType.MirrorTravel);
        if (hasMirror && !save.discoveredKeyIds.Contains("mirror"))
        {
            save.discoveredKeyIds.Add("mirror");
        }

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