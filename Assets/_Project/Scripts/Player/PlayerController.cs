using System.Collections;
using UnityEngine;

/// <summary>
/// 2.5D player movement and key-ability hooks for touch-first iOS gameplay.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4f;
    public float jumpForce = 8f;

    [Header("Ghost Key")]
    public float ghostPhaseDuration = 5f;
    public LayerMask solidLayers;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isGhostPhasing;
    private Coroutine ghostPhaseRoutine;
    private EventBus eventBus;
    private float ghostMoveMultiplier = 0.85f;
    private float noiseStepInterval = 0.5f;
    private float noiseTimer;

    public bool IsGhostPhasing => isGhostPhasing;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        eventBus = Resources.Load<EventBus>("EventBus");
    }

    private void Update()
    {
        if (isGhostPhasing) return;

        if (isGrounded && Mathf.Abs(rb.linearVelocity.x) > 0.1f)
        {
            noiseTimer += Time.deltaTime;
            if (noiseTimer >= noiseStepInterval)
            {
                noiseTimer = 0f;
                eventBus?.NoiseHeard(transform.position, 2.5f);
            }
        }
        else
        {
            noiseTimer = noiseStepInterval;
        }
    }

    public void Move(float horizontalInput)
    {
        var speed = isGhostPhasing ? moveSpeed * ghostMoveMultiplier : moveSpeed;
        rb.linearVelocity = new Vector2(horizontalInput * speed, rb.linearVelocity.y);
    }

    public void Jump()
    {
        if (!isGrounded) return;
        if (isGhostPhasing) return;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        eventBus?.NoiseHeard(transform.position, 6.0f);
    }

    public void ActivateGhostPhase(float duration)
    {
        if (ghostPhaseRoutine != null)
            StopCoroutine(ghostPhaseRoutine);

        ghostPhaseRoutine = StartCoroutine(GhostPhaseRoutine(duration > 0 ? duration : ghostPhaseDuration));
    }

    public void TryMirrorTravel()
    {
        Debug.Log("Mirror Key: searching for nearby reflective surface...");
        var colliders = Physics2D.OverlapCircleAll(transform.position, 3.5f);
        MirrorSurface nearestMirror = null;
        float minDist = float.MaxValue;

        foreach (var col in colliders)
        {
            var mirror = col.GetComponent<MirrorSurface>();
            if (mirror != null && mirror.isReflective && mirror.destinationMirror != null)
            {
                float dist = Vector2.Distance(transform.position, mirror.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearestMirror = mirror;
                }
            }
        }

        if (nearestMirror != null)
        {
            var dest = nearestMirror.destinationMirror;
            transform.position = dest.GetTravelPosition();

            // Visual and audio feedback
            FindFirstObjectByType<CameraFollow2D>()?.Pulse(0.45f, 0.35f);
            FindFirstObjectByType<GameAudioController>()?.PlayMemoryTransition();
            GameHaptics.TriggerHapticLight();
            FindFirstObjectByType<GameplayHUD>()?.ShowToast("Teleported through reflection.", 3f);
            Debug.Log($"Mirror travel successful: Teleported from {nearestMirror.name} to {dest.name}");
        }
        else
        {
            FindFirstObjectByType<GameplayHUD>()?.ShowToast("No reflective surfaces in range.", 3.5f);
            FindFirstObjectByType<GameAudioController>()?.PlayDoorRattle();
            Debug.LogWarning("Mirror Travel failed: no paired reflective surface in range.");
        }
    }

    public void ManipulateShadows()
    {
        Debug.Log("Shadow Key: shadow manipulation mode active.");
    }

    private IEnumerator GhostPhaseRoutine(float duration)
    {
        isGhostPhasing = true;
        eventBus?.GhostPhaseStarted();

        var collider = GetComponent<Collider2D>();
        if (collider != null)
            collider.isTrigger = true;

        yield return new WaitForSeconds(duration);

        if (collider != null)
            collider.isTrigger = false;

        isGhostPhasing = false;
        ghostPhaseRoutine = null;
        eventBus?.GhostPhaseEnded();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.contacts.Length > 0 && collision.contacts[0].normal.y > 0.5f)
            isGrounded = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }
}