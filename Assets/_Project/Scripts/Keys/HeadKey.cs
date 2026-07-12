using UnityEngine;

/// <summary>
/// Head Key — view memories locked inside objects and minds.
/// </summary>
public class HeadKey : MonoBehaviour, IKeyAbility
{
    [SerializeField] private string keyName = "Head Key";
    [SerializeField] private string description = "Unlock memories hidden in Keyhouse.";
    [SerializeField] private float cooldown = 12f;

    private UIManager uiManager;
    private EventBus eventBus;
    private float cooldownTimer;

    public string KeyName => keyName;
    public string Description => description;
    public KeyType Type => KeyType.Head;

    private void Awake()
    {
        uiManager = FindFirstObjectByType<UIManager>();
        eventBus = Resources.Load<EventBus>("EventBus");
    }

    private void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;
    }

    public void Activate()
    {
        if (!CanActivate()) return;

        // Head Key in Chapter 1 opens the portrait Mindscape via MemoryFragmentPuzzle,
        // not a floating text-only view. Prefer interact/Use Key near the portrait.
        cooldownTimer = cooldown;
        var portrait = FindFirstObjectByType<MemoryFragmentPuzzle>();
        if (portrait != null && !portrait.isSolved)
        {
            portrait.Interact();
            eventBus?.KeyActivated(this);
            eventBus?.SetTension(0.45f);
            return;
        }

        FindFirstObjectByType<GameplayHUD>()?.ShowToast(
            "No open mind nearby. Find the family portrait.", 2.8f);
        eventBus?.KeyActivated(this);
    }

    /// <summary>Apply cooldown without opening UI (used when Mindscape already started).</summary>
    public void BeginCooldownOnly()
    {
        cooldownTimer = cooldown;
        eventBus?.KeyActivated(this);
        eventBus?.SetTension(0.45f);
    }

    public void Deactivate()
    {
        eventBus?.KeyDeactivated(this);
        eventBus?.SetTension(0.15f);
    }

    public bool CanActivate() => cooldownTimer <= 0f;
}