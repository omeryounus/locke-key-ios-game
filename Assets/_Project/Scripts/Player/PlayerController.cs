using System.Collections;
using UnityEngine;

/// <summary>
/// Production 2.5D player movement: coyote time, jump buffer, accel/decel,
/// variable jump cut, and Ghost Key phasing for touch-first iOS gameplay.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4.4f;
    public float jumpForce = 8.8f;
    public float acceleration = 22f;
    public float deceleration = 28f;
    public float airControlReduction = 0.58f;
    public float turnBoost = 1.55f;
    public float maxFallSpeed = 16f;

    [Header("Jump Assist")]
    public float coyoteTime = 0.1f;
    public float jumpBufferTime = 0.12f;
    public float jumpCutMultiplier = 0.45f;

    [Header("Ghost Key")]
    public float ghostPhaseDuration = 5f;
    public LayerMask solidLayers;

    private Rigidbody2D rb;
    private Collider2D bodyCollider;
    private bool isGrounded;
    private bool wasGrounded;
    private bool isGhostPhasing;
    private Coroutine ghostPhaseRoutine;
    private EventBus eventBus;
    private float ghostMoveMultiplier = 0.88f;
    private float noiseStepInterval = 0.48f;
    private float noiseTimer;
    private float targetHorizontalVelocity;
    private float coyoteCounter;
    private float jumpBufferCounter;
    private bool jumpHeld;
    private bool jumpCutApplied;
    private float landTimer;

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

        // Stable mobile physics defaults if scene leaves defaults loose.
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation = true;
        rb.gravityScale = Mathf.Max(rb.gravityScale, 2.4f);
    }

    private void Update()
    {
        if (isGhostPhasing) return;

        if (coyoteCounter > 0f)
            coyoteCounter -= Time.deltaTime;
        if (jumpBufferCounter > 0f)
            jumpBufferCounter -= Time.deltaTime;
        if (landTimer > 0f)
            landTimer -= Time.deltaTime;

        // Variable jump: release early for shorter hop.
        if (!jumpHeld && !jumpCutApplied && rb.linearVelocity.y > 0.1f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
            jumpCutApplied = true;
        }

        TryConsumeJumpBuffer();

        if (isGrounded && Mathf.Abs(rb.linearVelocity.x) > 0.12f)
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
        targetHorizontalVelocity = Mathf.Clamp(horizontalInput, -1f, 1f) * speed;
    }

    public void SetJumpHeld(bool held) => jumpHeld = held;

    private void FixedUpdate()
    {
        wasGrounded = isGrounded;
        isGrounded = CheckGrounded();

        if (isGrounded)
            coyoteCounter = coyoteTime;

        if (isGrounded && !wasGrounded && rb.linearVelocity.y <= 0.05f)
        {
            landTimer = 0.14f;
            FindFirstObjectByType<CameraFollow2D>()?.Pulse(0.08f, 0.12f);
            GameHaptics.TriggerHapticLight();
        }

        float currentX = rb.linearVelocity.x;
        float targetX = targetHorizontalVelocity;

        float rate;
        if (Mathf.Abs(targetX) > 0.01f)
        {
            rate = isGrounded ? acceleration : acceleration * airControlReduction;
            if (Mathf.Sign(targetX) != Mathf.Sign(currentX) && Mathf.Abs(currentX) > 0.15f)
                rate *= turnBoost;
        }
        else
        {
            rate = isGrounded ? deceleration : deceleration * airControlReduction;
        }

        float newX = Mathf.MoveTowards(currentX, targetX, rate * Time.fixedDeltaTime);
        float newY = Mathf.Max(rb.linearVelocity.y, -maxFallSpeed);
        rb.linearVelocity = new Vector2(newX, newY);
    }

    private bool CheckGrounded()
    {
        if (bodyCollider == null) return false;
        if (isGhostPhasing) return false;

        var bounds = bodyCollider.bounds;
        Vector2 origin = new Vector2(bounds.center.x, bounds.min.y + 0.02f);
        Vector2 size = new Vector2(bounds.size.x * 0.78f, 0.06f);
        int mask = solidLayers.value != 0
            ? solidLayers.value & ~(1 << gameObject.layer)
            : Physics2D.DefaultRaycastLayers & ~(1 << gameObject.layer);

        var hit = Physics2D.BoxCast(origin, size, 0f, Vector2.down, 0.06f, mask);
        return hit.collider != null && !hit.collider.isTrigger;
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
        eventBus?.NoiseHeard(transform.position, 6.0f);
        FindFirstObjectByType<CameraFollow2D>()?.Pulse(0.05f, 0.1f);
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
            var dest = nearestMirror.destinationMirror;
            transform.position = dest.GetTravelPosition();
            FindFirstObjectByType<CameraFollow2D>()?.Pulse(0.45f, 0.35f);
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

        if (bodyCollider != null)
            bodyCollider.isTrigger = true;

        // Soft upward drift so phasing feels ethereal without breaking layout.
        if (rb != null && rb.linearVelocity.y < 0.5f)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, 0.4f));

        FindFirstObjectByType<CameraFollow2D>()?.Pulse(0.22f, 0.35f);

        yield return new WaitForSeconds(duration);

        if (bodyCollider != null)
            bodyCollider.isTrigger = false;

        isGhostPhasing = false;
        ghostPhaseRoutine = null;
        eventBus?.GhostPhaseEnded();
        FindFirstObjectByType<CameraFollow2D>()?.Pulse(0.12f, 0.2f);
    }
}
