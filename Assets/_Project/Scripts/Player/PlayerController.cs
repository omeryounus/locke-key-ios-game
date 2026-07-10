using System.Collections;
using UnityEngine;

/// <summary>
/// Premium side-scroller movement: responsive digital input with acceleration curves,
/// coyote/buffer, slope-aware grounding, wall slide, and interaction-friendly physics.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5.5f;
    public float jumpForce = 6.5f;
    public float groundAccel = 55f;
    public float groundDecel = 65f;
    public float airAccel = 28f;
    public float airControl = 0.8f;
    public float maxFallSpeed = 14f;
    public float gravityScale = 3.15f;
    public float wallSlideSpeed = 2.2f;
    public float turnBoost = 1.35f;

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
    private bool touchingWall;
    private int wallSign;
    private Coroutine ghostPhaseRoutine;
    private EventBus eventBus;
    private float ghostMoveMultiplier = 0.9f;
    private float noiseStepInterval = 0.42f;
    private float noiseTimer;
    private float moveInput;
    private float coyoteCounter;
    private float jumpBufferCounter;
    private bool jumpHeld;
    private bool jumpCutApplied;
    private float landTimer;
    private int groundMask;
    private float groundNormalY = 1f;

    public bool IsGhostPhasing => isGhostPhasing;
    public bool IsGrounded => isGrounded;
    public bool JustLanded => landTimer > 0f;
    public bool IsWallSliding => touchingWall && !isGrounded && rb != null && rb.linearVelocity.y < -0.05f
                                 && Mathf.Abs(moveInput) > 0.1f && Mathf.Sign(moveInput) == wallSign;
    public int WallSign => wallSign;
    public float HorizontalSpeed => rb != null ? Mathf.Abs(rb.linearVelocity.x) : 0f;
    public float MoveInput => moveInput;
    public Vector2 Velocity => rb != null ? rb.linearVelocity : Vector2.zero;
    public float GhostPhaseDuration => ghostPhaseDuration;
    public float GhostPhaseRemaining { get; private set; }
    public bool JumpHeld => jumpHeld;
    /// <summary>When true, footstep noise is driven by animation contact frames.</summary>
    public bool AnimationDrivesFootsteps { get; set; }

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
            if (!jumpHeld && !jumpCutApplied && rb.linearVelocity.y > 1f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
                jumpCutApplied = true;
            }

            TryConsumeJumpBuffer();
            if (!AnimationDrivesFootsteps)
                TickFootstepNoise();
        }

        if (isGhostPhasing && GhostPhaseRemaining > 0f)
            GhostPhaseRemaining = Mathf.Max(0f, GhostPhaseRemaining - Time.deltaTime);
    }

    private void TickFootstepNoise()
    {
        if (isGrounded && Mathf.Abs(rb.linearVelocity.x) > 0.4f)
        {
            noiseTimer += Time.deltaTime;
            if (noiseTimer >= noiseStepInterval)
            {
                noiseTimer = 0f;
                EmitFootstepNoise();
            }
        }
        else noiseTimer = noiseStepInterval;
    }

    /// <summary>Called by the animator on walk/run ground-contact frames so SFX never drifts from art.</summary>
    public void EmitFootstepNoise(float radius = 2.2f)
    {
        if (!isGrounded || isGhostPhasing) return;
        eventBus?.NoiseHeard(transform.position, radius);
        FindFirstObjectByType<GameAudioController>()?.PlayFootstep();
    }

    public void Move(float horizontalInput) =>
        moveInput = Mathf.Clamp(horizontalInput, -1f, 1f);

    public void SetJumpHeld(bool held) => jumpHeld = held;

    private void FixedUpdate()
    {
        wasGrounded = isGrounded;
        ProbeContacts();

        if (isGrounded)
            coyoteCounter = coyoteTime;

        if (isGrounded && !wasGrounded && rb.linearVelocity.y <= 0.2f)
        {
            landTimer = 0.12f;
            FindFirstObjectByType<CameraFollow2D>()?.Shake(0.035f, 0.1f);
        }

        float speed = isGhostPhasing ? moveSpeed * ghostMoveMultiplier : moveSpeed;
        float targetX = moveInput * speed;
        float currentX = rb.linearVelocity.x;
        float newX;

        if (isGhostPhasing)
        {
            newX = targetX;
        }
        else if (isGrounded)
        {
            float rate = Mathf.Abs(targetX) > 0.01f ? groundAccel : groundDecel;
            if (Mathf.Abs(targetX) > 0.01f && Mathf.Sign(targetX) != Mathf.Sign(currentX) && Mathf.Abs(currentX) > 0.2f)
                rate *= turnBoost;
            // Near-instant but still blended for premium feel
            newX = Mathf.MoveTowards(currentX, targetX, rate * Time.fixedDeltaTime);
            // Snap when very close to target for zero deadzone lag
            if (Mathf.Abs(newX - targetX) < 0.05f) newX = targetX;
        }
        else
        {
            float rate = airAccel * airControl;
            newX = Mathf.MoveTowards(currentX, targetX, rate * Time.fixedDeltaTime);
        }

        float newY = rb.linearVelocity.y;
        if (touchingWall && !isGrounded && newY < 0f && Mathf.Abs(moveInput) > 0.1f && Mathf.Sign(moveInput) == wallSign)
            newY = Mathf.Max(newY, -wallSlideSpeed);

        newY = Mathf.Max(newY, -maxFallSpeed);
        rb.linearVelocity = new Vector2(newX, newY);
    }

    private void ProbeContacts()
    {
        isGrounded = false;
        touchingWall = false;
        wallSign = 0;
        groundNormalY = 1f;
        if (bodyCollider == null || isGhostPhasing) return;

        var bounds = bodyCollider.bounds;

        // Ground
        Vector2 gOrigin = new Vector2(bounds.center.x, bounds.min.y + 0.02f);
        Vector2 gSize = new Vector2(Mathf.Max(0.08f, bounds.size.x * 0.7f), 0.08f);
        var gHit = Physics2D.BoxCast(gOrigin, gSize, 0f, Vector2.down, 0.08f, groundMask);
        if (gHit.collider != null && !gHit.collider.isTrigger && gHit.collider.attachedRigidbody != rb)
        {
            if (gHit.normal.y > 0.35f)
            {
                isGrounded = true;
                groundNormalY = gHit.normal.y;
            }
        }

        // Walls
        float wallDist = 0.06f;
        Vector2 wSize = new Vector2(0.06f, bounds.size.y * 0.55f);
        var left = Physics2D.BoxCast(new Vector2(bounds.min.x + 0.02f, bounds.center.y), wSize, 0f, Vector2.left, wallDist, groundMask);
        var right = Physics2D.BoxCast(new Vector2(bounds.max.x - 0.02f, bounds.center.y), wSize, 0f, Vector2.right, wallDist, groundMask);
        if (left.collider != null && !left.collider.isTrigger && left.normal.x > 0.5f)
        {
            touchingWall = true;
            wallSign = -1;
        }
        else if (right.collider != null && !right.collider.isTrigger && right.normal.x < -0.5f)
        {
            touchingWall = true;
            wallSign = 1;
        }
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
        FindFirstObjectByType<CameraFollow2D>()?.Pulse(0.04f, 0.08f);
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
                if (dist < minDist) { minDist = dist; nearestMirror = mirror; }
            }
        }

        var hud = FindFirstObjectByType<GameplayHUD>();
        if (nearestMirror != null)
        {
            transform.position = nearestMirror.destinationMirror.GetTravelPosition();
            eventBus?.MirrorTravel();
            FindFirstObjectByType<CameraFollow2D>()?.Pulse(0.35f, 0.3f);
            FindFirstObjectByType<CameraFollow2D>()?.Shake(0.1f, 0.25f);
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
        GhostPhaseRemaining = duration;
        eventBus?.GhostPhaseStarted();
        GameHaptics.PhaseStart();
        FindFirstObjectByType<CameraFollow2D>()?.Pulse(0.18f, 0.3f);
        FindFirstObjectByType<CameraFollow2D>()?.Shake(0.05f, 0.2f);

        yield return new WaitForSeconds(duration);

        isGhostPhasing = false;
        GhostPhaseRemaining = 0f;
        ghostPhaseRoutine = null;
        eventBus?.GhostPhaseEnded();
        FindFirstObjectByType<CameraFollow2D>()?.Pulse(0.1f, 0.15f);
    }
}
