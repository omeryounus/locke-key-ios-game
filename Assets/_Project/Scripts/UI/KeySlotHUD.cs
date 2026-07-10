using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays the premium key inventory slot with state-driven art and active pulse.
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
    private Vector3 baseScale = Vector3.one;
    private Outline glow;

    private void Awake()
    {
        if (library == null)
            library = KeySlotLibrary.LoadDefault();
        if (keyManager == null)
            keyManager = FindFirstObjectByType<KeyManager>();
        if (slotImage == null)
            slotImage = GetComponent<Image>();
        baseScale = transform.localScale;
        glow = GetComponent<Outline>();
        if (glow == null)
            glow = gameObject.AddComponent<Outline>();
        glow.effectColor = new Color(LockeKeyUITheme.LKGold.r, LockeKeyUITheme.LKGold.g, LockeKeyUITheme.LKGold.b, 0.35f);
        glow.effectDistance = new Vector2(2f, -2f);
    }

    private void Update()
    {
        if (discoveredTimer > 0f)
        {
            discoveredTimer -= Time.deltaTime;
            var pulse = 1f + Mathf.Sin(Time.time * 10f) * 0.08f;
            transform.localScale = baseScale * pulse;
            if (discoveredTimer <= 0f)
            {
                transform.localScale = baseScale;
                Refresh();
            }
        }
        else if (state is SlotState.GhostActive or SlotState.HeadActive)
        {
            var pulse = 1f + Mathf.Sin(Time.time * 3.2f) * 0.035f;
            transform.localScale = baseScale * pulse;
            if (glow != null)
            {
                var a = 0.35f + Mathf.Sin(Time.time * 3.2f) * 0.2f;
                glow.effectColor = new Color(LockeKeyUITheme.LKGold.r, LockeKeyUITheme.LKGold.g, LockeKeyUITheme.LKGold.b, a);
            }
        }
        else
        {
            transform.localScale = Vector3.Lerp(transform.localScale, baseScale, Time.deltaTime * 10f);
        }
    }

    public void FlashDiscovered(float duration = 2.5f)
    {
        discoveredTimer = duration;
        Apply(SlotState.Discovered);
        GameHaptics.KeyPickup();
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
        slotImage.color = Color.white;
    }
}
