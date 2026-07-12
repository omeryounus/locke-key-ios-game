using System.Collections;
using UnityEngine;

/// <summary>
/// Ghost Key companion on the player.
/// Phase itself is owned by PlayerController; this handles Echo "body capture"
/// while ethereal (stun / force phase end) without full-screen swipe hijacking.
/// </summary>
[DisallowMultipleComponent]
public class GhostKeyAbility : MonoBehaviour
{
    [SerializeField] private float ghostDuration = 5.5f;
    [SerializeField] private float bodyCaptureDuration = 3.5f;

    private PlayerController player;
    private bool bodyCaptured;
    private float bodyCapturedTimer;

    public bool IsBodyCaptured => bodyCaptured;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (!bodyCaptured) return;
        bodyCapturedTimer -= Time.deltaTime;
        if (bodyCapturedTimer <= 0f)
            bodyCaptured = false;
    }

    /// <summary>Activate unified phase on the player controller.</summary>
    public void ActivateGhostForm()
    {
        if (player == null)
            player = GetComponent<PlayerController>();
        if (player == null || player.IsGhostPhasing || bodyCaptured) return;
        player.ActivateGhostPhase(ghostDuration);
    }

    /// <summary>
    /// Echo seized the abandoned body while player is phasing.
    /// Forces solid form and a brief vulnerability window.
    /// </summary>
    public void TriggerBodyCapture()
    {
        if (bodyCaptured) return;
        bodyCaptured = true;
        bodyCapturedTimer = bodyCaptureDuration;

        if (player == null)
            player = GetComponent<PlayerController>();

        // End phase immediately by restarting a near-zero phase coroutine is messy;
        // stop ethereal state by activating a 0-duration end if still phasing.
        if (player != null && player.IsGhostPhasing)
            StartCoroutine(ForceEndPhaseNextFrame());

        FindFirstObjectByType<GameAudioController>()?.PlayDoorRattle();
        GameHaptics.TriggerHapticLight();
    }

    private IEnumerator ForceEndPhaseNextFrame()
    {
        // Wait one frame so Echo stun toast can show, then snap phase off via tiny phase
        // that immediately completes. PlayerController ends phase when coroutine finishes.
        // Safer: stop all coroutines on player is too broad — use ActivateGhostPhase(0.05f)
        // only if still phasing after a beat.
        yield return null;
        if (player != null && player.IsGhostPhasing)
            player.ActivateGhostPhase(0.05f);
    }
}
