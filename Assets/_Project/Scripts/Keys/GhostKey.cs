using UnityEngine;

/// <summary>
/// Ghost Key — phase through walls and doors for a short time.
/// </summary>
public class GhostKey : MonoBehaviour, IKeyAbility
{
    [SerializeField] private string keyName = "Ghost Key";
    [SerializeField] private string description = "Phase through solid matter for a short time.";
    [SerializeField] private float phaseDuration = 5f;
    [SerializeField] private float cooldown = 8f;

    private PlayerController player;
    private EventBus eventBus;
    private float cooldownTimer;
    private bool isActive;

    public string KeyName => keyName;
    public string Description => description;
    public KeyType Type => KeyType.Ghost;

    private void Awake()
    {
        player = FindFirstObjectByType<PlayerController>();
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

        isActive = true;
        cooldownTimer = cooldown;
        player?.ActivateGhostPhase(phaseDuration);
        eventBus?.KeyActivated(this);
        eventBus?.SetTension(0.3f);
    }

    public void Deactivate()
    {
        if (!isActive) return;

        isActive = false;
        eventBus?.KeyDeactivated(this);
        eventBus?.SetTension(0.1f);
    }

    public bool CanActivate() => !isActive && cooldownTimer <= 0f && player != null;
}