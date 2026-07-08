using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays the premium key inventory slot with state-driven art.
/// </summary>
public class KeySlotHUD : MonoBehaviour
{
    public enum SlotState
    {
        Empty,
        GhostActive,
        HeadActive,
        Cooldown,
        Discovered
    }

    [SerializeField] private Image slotImage;
    [SerializeField] private KeySlotLibrary library;
    [SerializeField] private KeyManager keyManager;

    private SlotState state = SlotState.Empty;
    private float discoveredTimer;

    private void Awake()
    {
        if (library == null)
            library = KeySlotLibrary.LoadDefault();
        if (keyManager == null)
            keyManager = FindFirstObjectByType<KeyManager>();
        if (slotImage == null)
            slotImage = GetComponent<Image>();
    }

    private void Update()
    {
        if (discoveredTimer > 0f)
        {
            discoveredTimer -= Time.deltaTime;
            if (discoveredTimer <= 0f)
                Refresh();
        }
    }

    public void FlashDiscovered(float duration = 2.5f)
    {
        discoveredTimer = duration;
        Apply(SlotState.Discovered);
    }

    public void SetCooldown(bool onCooldown)
    {
        if (onCooldown)
            Apply(SlotState.Cooldown);
        else
            Refresh();
    }

    public void Refresh()
    {
        if (discoveredTimer > 0f)
        {
            Apply(SlotState.Discovered);
            return;
        }

        if (keyManager?.currentActiveKey == null)
        {
            Apply(SlotState.Empty);
            return;
        }

        Apply(keyManager.currentActiveKey.abilityType switch
        {
            KeyManager.KeyAbilityType.GhostPhase => SlotState.GhostActive,
            KeyManager.KeyAbilityType.HeadMemory => SlotState.HeadActive,
            _ => SlotState.Empty
        });
    }

    private void Apply(SlotState newState)
    {
        state = newState;
        if (slotImage == null || library == null)
            return;

        slotImage.sprite = newState switch
        {
            SlotState.GhostActive => library.ghostActive,
            SlotState.HeadActive => library.headActive,
            SlotState.Cooldown => library.cooldown,
            SlotState.Discovered => library.discovered,
            _ => library.empty
        };
        slotImage.enabled = slotImage.sprite != null;
    }
}