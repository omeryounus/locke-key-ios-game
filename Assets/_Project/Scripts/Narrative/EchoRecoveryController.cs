using System.Collections;
using UnityEngine;

/// <summary>
/// Fair fail/recovery when the Echo catches the player during Beat 5.
/// </summary>
public class EchoRecoveryController : MonoBehaviour
{
    [SerializeField] private float stunDuration = 1.1f;
    [SerializeField] private float knockbackForce = 4.5f;
    [SerializeField] private float echoRespawnDelay = 2.2f;
    [SerializeField] private float recoveryTension = 0.55f;

    private EventBus eventBus;
    private PlayerController player;
    private Rigidbody2D playerRb;
    private TouchGameplayController gameplay;
    private CameraFollow2D cameraFollow;
    private GameplayHUD hud;
    private EchoEncounterManager echoManager;
    private bool recovering;

    private void Awake()
    {
        eventBus = Resources.Load<EventBus>("EventBus");
        player = FindFirstObjectByType<PlayerController>();
        playerRb = player != null ? player.GetComponent<Rigidbody2D>() : null;
        gameplay = FindFirstObjectByType<TouchGameplayController>();
        cameraFollow = FindFirstObjectByType<CameraFollow2D>();
        hud = FindFirstObjectByType<GameplayHUD>();
        echoManager = FindFirstObjectByType<EchoEncounterManager>();

        if (eventBus != null)
            eventBus.OnEchoCaught += HandleEchoCaught;
    }

    private void OnDestroy()
    {
        if (eventBus != null)
            eventBus.OnEchoCaught -= HandleEchoCaught;
    }

    private void HandleEchoCaught()
    {
        if (recovering || player == null) return;
        StartCoroutine(RecoveryRoutine());
    }

    private IEnumerator RecoveryRoutine()
    {
        recovering = true;
        gameplay?.SetMoveInput(0f);
        gameplay?.SetInputLocked(true);

        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.AddForce(Vector2.left * knockbackForce, ForceMode2D.Impulse);
        }

        cameraFollow?.Pulse(0.35f, 0.45f);
        eventBus?.SetTension(recoveryTension);
        hud?.ShowToast("Hide behind the arch to break its gaze, then run to the passage.", 4.5f);

        yield return new WaitForSeconds(stunDuration);

        var checkpoint = ChapterSaveManager.Instance != null
            ? ChapterSaveManager.Instance.GetCheckpointPosition()
            : player.transform.position;
        player.transform.position = checkpoint;

        if (playerRb != null)
            playerRb.linearVelocity = Vector2.zero;

        gameplay?.SetInputLocked(false);
        yield return new WaitForSeconds(echoRespawnDelay);

        echoManager?.RespawnEchoIfNeeded();
        recovering = false;
    }
}