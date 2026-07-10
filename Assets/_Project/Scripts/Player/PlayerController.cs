using System.Collections;
using UnityEngine;

/// <summary>
/// Snappy side-scroller movement for a narrative puzzle adventure.
/// Digital touch input feels immediate on ground; light air control in jumps.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5.6f;
    public float jumpForce = 6.4f;
    public float airControl = 0.75f;
    public float maxFallSpeed = 14f;
    public float gravityScale = 3.1f;

    [Header("Jump Assist")]
    public float coyoteTime = 0.12f;
    public float jumpBufferTime = 0.14f;
    public float jumpCutMultiplier = 0.55f;

    [Header("Ghost Key")]
    public float ghostPhaseDuration = 5.5f;
    public LayerMask solidLayers;

    private Rigidbody2D rb;
    private Collider2D bodyCollider;
    private bool isGrounded;
    private bool wasGrounded;
    private bool isGhostPhasing;
    private Coroutine ghostPhaseRoutine;
    private EventBus eventBus;
    private float ghostMoveMultiplier = 0.9f;
    private float noiseStepInterval = 0.45f;
    private float noiseTimer;
    private float moveInput;
    private float coyoteCounter;
    private float jumpBufferCounter;
    private bool jumpHeld;
    private bool jumpCutApplied;
    private float landTimer;
    private int groundMask;

    public bool IsGhostPhasing => isGhostPhasing;
    public bool IsGrounded => isGrounded;
    public bool JustLanded => landTimer > 0f;
    public float HorizontalSpeed => rb != null ? Mathf.Abs(rb.linearVelocity.x) : 0f;
    public Vector2 Velocity => rb != null ? rb.linearVelocity : Vector2.zero;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
        eventBus = Resources.Load<EventBus>("EventBus");

        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation = true;
        rb.gravityScale = gravityScale;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Always have a usable ground mask (scene often leaves LayerMask empty).
        groundMask = solidLayers.value != 0
            ? solidLayers.value
            : Physics2D.DefaultRaycastLayers;
        groundMask &= ~(1 << gameObject.layer);
    }

    private void Update()
    {
        if (coyoteCounter > 0f) coyoteCounter -= Time.deltaTime;
        if (jumpBufferCounter > 0f) jumpBufferCounter -= Time.deltaTime;
        if (landTimer > 0f) landTimer -= Time.deltaTime;

        if (!isGhostPhasing)
        {
            // Variable jump cut
            if (!jumpHeld && !jumpCutApplied && rb.linearVelocity.y > 1f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
                jumpCutApplied = true;
            }

            TryConsumeJumpBuffer();
            TickFootstepNoise();
        }
    }

    private void TickFootstepNoise()
    {
        if (isGrounded && Mathf.Abs(rb.linearVelocity.x) > 0.4f)
        {
            noiseTimer += Time.deltaTime;
            if (noiseTimer >= noiseStepInterval)
            {
                noiseTimer = 0f;
                eventBus?.NoiseHeard(transform.position, 2.2f);
            }
        }
        else
        {
            noiseTimer = noiseStepInterval;
        }
    }

    public void Move(float horizontalInput)
    {
        moveInput = Mathf.Clamp(horizontalInput, -1f, 1f);
    }

    public void SetJumpHeld(bool held) => jumpHeld = held;

    private void FixedUpdate()
    {
        wasGrounded = isGrounded;
        isGrounded = !isGhostPhasing && CheckGrounded();

        if (isGrounded)
            coyoteCounter = coyoteTime;

        if (isGrounded && !wasGrounded && rb.linearVelocity.y <= 0.2f)
            landTimer = 0.12f;

        float speed = isGhostPhasing ? moveSpeed * ghostMoveMultiplier : moveSpeed;
        float targetX = moveInput * speed;
        float currentX = rb.linearVelocity.x;
        float newX;

        if (isGrounded || isGhostPhasing)
        {
            // Instant digital response on ground — no sluggish accel curve.
            newX = targetX;
        }
        else
        {
            // Light air steering only.
            newX = Mathf.Lerp(currentX, targetX, airControl * 12f * Time.fixedDeltaTime);
        }

        float newY = Mathf.Max(rb.linearVelocity.y, -maxFallSpeed);
        rb.linearVelocity = new Vector2(newX, newY);
    }

    private bool CheckGrounded()
    {
        if (bodyCollider == null) return false;

        var bounds = bodyCollider.bounds;
        // Slightly inset so wall contacts don't count as ground.
        Vector2 origin = new Vector2(bounds.center.x, bounds.min.y + 0.02f);
        Vector2 size = new Vector2(Mathf.Max(0.08f, bounds.size.x * 0.7f), 0.08f);

        var hit = Physics2D.BoxCast(origin, size, 0f, Vector2.down, 0.08f, groundMask);
        if (hit.collider == null || hit.collider.isTrigger) return false;
        if (hit.collider.attachedRigidbody == rb) return false;
        // Reject near-vertical surfaces.
        return hit.normal.y > 0.4f;
    }

    public void Jump()
    {
        jumpBufferCounter = jumpBufferTime;
        jumpHeld = true;
        TryConsumeJumpBuffer();
    }

    private void TryConsumeJumpBuffer()
    {
        if (jumpBufferCounter <= 0f) return;
        if (isGhostPhasing) return;
        if (coyoteCounter <= 0f && !isGrounded) return;

        jumpBufferCounter = 0f;
        coyoteCounter = 0f;
        jumpCutApplied = false;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        eventBus?.NoiseHeard(transform.position, 5f);
    }

    public void ActivateGhostPhase(float duration)
    {
        if (ghostPhaseRoutine != null)
            StopCoroutine(ghostPhaseRoutine);

        ghostPhaseRoutine = StartCoroutine(GhostPhaseRoutine(duration > 0 ? duration : ghostPhaseDuration));
    }

    public void TryMirrorTravel()
    {
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

        var hud = FindFirstObjectByType<GameplayHUD>();
        if (nearestMirror != null)
        {
            transform.position = nearestMirror.destinationMirror.GetTravelPosition();
            FindFirstObjectByType<CameraFollow2D>()?.Pulse(0.35f, 0.3f);
            FindFirstObjectByType<GameAudioController>()?.PlayMemoryTransition();
            GameHaptics.TriggerHapticLight();
            hud?.ShowToast("Teleported through reflection.", 3f);
        }
        else
        {
            hud?.ShowToast("No reflective surfaces in range.", 3f);
            FindFirstObjectByType<GameAudioController>()?.PlayDoorRattle();
        }
    }

    public void ManipulateShadows()
    {
        FindFirstObjectByType<GameplayHUD>()?.ShowToast("Shadows stir... but refuse to obey yet.", 2.5f);
    }

    private IEnumerator GhostPhaseRoutine(float duration)
    {
        isGhostPhasing = true;
        eventBus?.GhostPhaseStarted();
        GameHaptics.PhaseStart();

        // SealedDoorPuzzle disables its own collider while phasing in range.
        // Keep the player solid so they still stand on floors.
        FindFirstObjectByType<CameraFollow2D>()?.Pulse(0.18f, 0.3f);

        yield return new WaitForSeconds(duration);

        isGhostPhasing = false;
        ghostPhaseRoutine = null;
        eventBus?.GhostPhaseEnded();
        FindFirstObjectByType<CameraFollow2D>()?.Pulse(0.1f, 0.15f);
    }
}
