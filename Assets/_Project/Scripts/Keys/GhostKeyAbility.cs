using UnityEngine;

/// <summary>
/// Thin Ghost Key helper on the player. Phase is owned by PlayerController —
/// this component only exists so older call sites resolve cleanly.
/// Does NOT steal touch/mouse input (swipe hijacking removed for reliable HUD).
/// </summary>
[DisallowMultipleComponent]
public class GhostKeyAbility : MonoBehaviour
{
    [SerializeField] private float ghostDuration = 5.5f;

    private PlayerController player;

    public bool IsBodyCaptured => false;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
    }

    /// <summary>Activate unified phase on the player controller.</summary>
    public void ActivateGhostForm()
    {
        if (player == null)
            player = GetComponent<PlayerController>();
        if (player == null) return;
        if (player.IsGhostPhasing) return;
        player.ActivateGhostPhase(ghostDuration);
    }
}
