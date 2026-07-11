using System.Collections;
using UnityEngine;

/// <summary>
/// Commercial mobile side-scroller foundation:
/// fixed-step movement, coyote/buffer, variable gravity, slope-aware ground,
/// wall slide, jump cut, and cached game-feel hooks. No root motion.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5.6f;
    public float jumpForce = 6.75f;
    public float groundAccel = 70f;
    public float groundDecel = 85f;
    public float turnBoost = 1.55f;
    public float airAccel = 36f;
    public float airControl = 0.85f;
    public float maxFallSpeed = 16f;
    public float gravityScale = 3.2f;
    [Tooltip("Extra gravity when falling — removes floaty apex hang.")]
    public float fallGravityMult = 1.55f;
    [Tooltip("Extra gravity after jump cut while still rising slightly.")]
    public float jumpCutGravityMult = 1.25f;
    public float wallSlideSpeed = 2.0f;
    [Tooltip("Input magnitude below this is treated as zero.")]
    public float inputDeadzone = 0.08f;

    [Header("Jump Assist")]
    public float coyoteTime = 0.11f;
    public float jumpBufferTime = 0.13f;
    public float jumpCutMultiplier = 0.5f;
    public float landLockTime = 0.04f;

    [Header("Grounding")]
    public float groundCastDistance = 0.1f;
    public float groundCastSkin = 0.02f;
    public float minGroundNormalY = 0.5f;
    public float slopeLimitDeg = 55f;
    public LayerMask solidLayers;

    [Header("Ghost Key")]
    public float ghostPhaseDuration = 5.5f;

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
    private bool jumpQueued;
    private float landTimer;
    private float landLockTimer;
    private int groundMask;
    private float groundNormalY = 1f;
    private Vector2 groundNormal = Vector2.up;

    // Cached scene services (avoid FindFirstObjectByType in hot paths)
    private CameraFollow2D cachedCamera;
    private GameAudioController cachedAudio;
    private GameplayHUD cachedHud;
    private float serviceRefreshTimer;

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
    public Vector2 GroundNormal => groundNormal;
    public bool AnimationDrivesFootsteps { get; set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
        eventBus = Resources.Load<EventBus>("EventBus");

        // Commercial defaults — overwrite scene serialization drift
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation = true;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.gravityScale = gravityScale;
        rb.linearDamping = 0f;
        rb.angularDamping = 0.05f;
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;

        groundMask = solidLayers.value != 0
            ? solidLayers.value
            : Physics2D.DefaultRaycastLayers;
        groundMask &= ~(1 << gameObject.layer);

        CacheServices(force: true);
    }

    private void CacheServices(bool force = false)
    {
        serviceRefreshTimer -= Time.unscaledDeltaTime;
        if (!force && serviceRefreshTimer > 0f) return;
        serviceRefreshTimer = 1.5f;
        if (cachedCamera == null) cachedCamera = FindFirstObjectByType<CameraFollow2D>();
        if (cachedAudio == null) cachedAudio = FindFirstObjectByType<GameAudioController>();
        if (cachedHud == null) cachedHud = FindFirstObjectByType<GameplayHUD>();
    }

    private void Update()
    {
        CacheServices();

        if (coyoteCounter > 0f) coyoteCounter -= Time.deltaTime;
        if (jumpBufferCounter > 0f) jumpBufferCounter -= Time.deltaTime;
        if (landTimer > 0f) landTimer -= Time.deltaTime;
        if (landLockTimer > 0f) landLockTimer -= Time.deltaTime;

        if (!isGhostPhasing)
        {
            // Jump cut is applied once when releasing while rising
            if (!jumpHeld && !jumpCutApplied && rb.linearVelocity.y > 0.8f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
                jumpCutApplied = true;
            }

            TryQueueJumpFromBuffer();
            if (!AnimationDrivesFootsteps)
                TickFootstepNoise();
        }

        if (isGhostPhasing && GhostPhaseRemaining > 0f)
            GhostPhaseRemaining = Mathf.Max(0f, GhostPhaseRemaining - Time.deltaTime);

        ApplyVariableGravity();
    }

    private void ApplyVariableGravity()
    {
        if (rb == null || isGhostPhasing)
        {
            if (rb != null) rb.gravityScale = gravityScale * 0.35f; // light float while phasing
            return;
        }

        float g = gravityScale;
        float vy = rb.linearVelocity.y;
        if (vy < -0.1f)
            g *= fallGravityMult; // snappy fall
        else if (vy > 0.1f && jumpCutApplied)
            g *= jumpCutGravityMult;
        else if (vy > 0.1f && !jumpHeld)
            g *= jumpCutGravityMult; // release before cut flag edge case
        rb.gravityScale = g;
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

    public void EmitFootstepNoise(float radius = 2.2f)
    {
        if (!isGrounded || isGhostPhasing) return;
        eventBus?.NoiseHeard(transform.position, radius);
        cachedAudio?.PlayFootstep();
    }

    public void Move(float horizontalInput)
    {
        float v = Mathf.Clamp(horizontalInput, -1f, 1f);
        moveInput = Mathf.Abs(v) < inputDeadzone ? 0f : v;
    }

    public void SetJumpHeld(bool held) => jumpHeld = held;

    private void FixedUpdate()
    {
        wasGrounded = isGrounded;
        ProbeContacts();

        if (isGrounded)
            coyoteCounter = coyoteTime;

        // Landing response
        if (isGrounded && !wasGrounded && rb.linearVelocity.y <= 0.35f)
        {
            landTimer = 0.14f;
            landLockTimer = landLockTime;
            // Kill residual downward velocity into floor (stops bounce jitter)
            if (rb.linearVelocity.y < 0f)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            cachedCamera?.Shake(0.028f, 0.09f);
        }

        // Apply buffered jump on physics step only
        if (jumpQueued)
        {
            jumpQueued = false;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            eventBus?.NoiseHeard(transform.position, 5f);
            cachedCamera?.Pulse(0.035f, 0.07f);
        }

        float speed = isGhostPhasing ? moveSpeed * ghostMoveMultiplier : moveSpeed;
        // Micro land lock: slightly reduced control on touchdown for weight
        if (landLockTimer > 0f && isGrounded)
            speed *= 0.72f;

        float targetX = moveInput * speed;
        float currentX = rb.linearVelocity.x;
        float newX;

        if (isGhostPhasing)
        {
            newX = Mathf.MoveTowards(currentX, targetX, groundAccel * 0.7f * Time.fixedDeltaTime);
        }
        else if (isGrounded)
        {
            bool accelerating = Mathf.Abs(targetX) > 0.01f;
            float rate = accelerating ? groundAccel : groundDecel;
            // Snappier reverse (turnaround)
            if (accelerating && Mathf.Sign(targetX) != Mathf.Sign(currentX) && Mathf.Abs(currentX) > 0.15f)
                rate *= turnBoost;
            newX = Mathf.MoveTowards(currentX, targetX, rate * Time.fixedDeltaTime);
            // Zero deadzone lag when nearly at target
            if (Mathf.Abs(newX - targetX) < 0.04f) newX = targetX;

            // Project along slope so feet don't skate uphill
            if (groundNormalY < 0.999f && Mathf.Abs(newX) > 0.01f)
            {
                Vector2 along = new Vector2(groundNormal.y, -groundNormal.x);
                if (Mathf.Sign(along.x) != Mathf.Sign(newX) && Mathf.Abs(along.x) > 0.01f)
                    along = -along;
                // Keep horizontal magnitude dominant for control feel
                float mag = Mathf.Abs(newX);
                Vector2 projected = along.normalized * mag;
                // Blend so pure flats stay pure horizontal
                float slopeBlend = 1f - groundNormalY;
                newX = Mathf.Lerp(newX, projected.x, slopeBlend);
                float newYSlope = Mathf.Lerp(rb.linearVelocity.y, projected.y, slopeBlend * 0.85f);
                // Only apply slope Y when grounded and not jumping
                if (!jumpQueued && rb.linearVelocity.y <= 0.5f)
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, newYSlope);
            }
        }
        else
        {
            float rate = airAccel * airControl;
            // Slightly stronger air control when returning toward neutral (feels fair)
            if (Mathf.Abs(targetX) < 0.01f)
                rate *= 0.65f;
            newX = Mathf.MoveTowards(currentX, targetX, rate * Time.fixedDeltaTime);
        }

        float newY = rb.linearVelocity.y;
        if (IsWallSliding)
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
        groundNormal = Vector2.up;
        if (bodyCollider == null) return;
        // Ghost phase still probes ground for landing animation, but ignores walls/solids via layer later
        if (isGhostPhasing)
        {
            // Keep coyote-friendly grounded read for FX only
            ProbeGroundOnly();
            return;
        }

        ProbeGroundOnly();
        ProbeWalls();
    }

    private void ProbeGroundOnly()
    {
        var bounds = bodyCollider.bounds;
        float width = Mathf.Max(0.1f, bounds.size.x * 0.72f);
        Vector2 boxSize = new Vector2(width, groundCastSkin);
        Vector2 origin = new Vector2(bounds.center.x, bounds.min.y + groundCastSkin);

        // Primary box cast
        var hit = Physics2D.BoxCast(origin, boxSize, 0f, Vector2.down, groundCastDistance, groundMask);
        if (IsValidGround(hit))
        {
            AcceptGround(hit);
            return;
        }

        // Triple ray fallback (edges + center) — catches seams/stairs
        float y = bounds.min.y + 0.01f;
        float left = bounds.min.x + 0.04f;
        float right = bounds.max.x - 0.04f;
        float mid = bounds.center.x;
        if (RayGround(new Vector2(mid, y)) || RayGround(new Vector2(left, y)) || RayGround(new Vector2(right, y)))
            return;
    }

    private bool RayGround(Vector2 origin)
    {
        var hit = Physics2D.Raycast(origin, Vector2.down, groundCastDistance + 0.04f, groundMask);
        if (!IsValidGround(hit)) return false;
        AcceptGround(hit);
        return true;
    }

    private bool IsValidGround(RaycastHit2D hit)
    {
        if (hit.collider == null || hit.collider.isTrigger) return false;
        if (hit.collider.attachedRigidbody == rb) return false;
        if (hit.normal.y < minGroundNormalY) return false;
        float angle = Vector2.Angle(hit.normal, Vector2.up);
        return angle <= slopeLimitDeg;
    }

    private void AcceptGround(RaycastHit2D hit)
    {
        isGrounded = true;
        groundNormal = hit.normal;
        groundNormalY = hit.normal.y;
    }

    private void ProbeWalls()
    {
        var bounds = bodyCollider.bounds;
        float wallDist = 0.07f;
        Vector2 wSize = new Vector2(0.05f, bounds.size.y * 0.5f);
        // Cast from mid-body so floor colliders don't register as walls
        float y = bounds.center.y + bounds.size.y * 0.05f;

        var left = Physics2D.BoxCast(new Vector2(bounds.min.x + 0.03f, y), wSize, 0f, Vector2.left, wallDist, groundMask);
        var right = Physics2D.BoxCast(new Vector2(bounds.max.x - 0.03f, y), wSize, 0f, Vector2.right, wallDist, groundMask);

        if (left.collider != null && !left.collider.isTrigger && left.collider.attachedRigidbody != rb && left.normal.x > 0.55f)
        {
            touchingWall = true;
            wallSign = -1;
        }
        else if (right.collider != null && !right.collider.isTrigger && right.collider.attachedRigidbody != rb && right.normal.x < -0.55f)
        {
            touchingWall = true;
            wallSign = 1;
        }
    }

    public void Jump()
    {
        jumpBufferCounter = jumpBufferTime;
        jumpHeld = true;
        TryQueueJumpFromBuffer();
    }

    private void TryQueueJumpFromBuffer()
    {
        if (jumpBufferCounter <= 0f) return;
        if (isGhostPhasing) return;
        if (coyoteCounter <= 0f && !isGrounded) return;

        jumpBufferCounter = 0f;
        coyoteCounter = 0f;
        jumpCutApplied = false;
        jumpQueued = true; // applied in FixedUpdate
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

        CacheServices(force: true);
        if (nearestMirror != null)
        {
            transform.position = nearestMirror.destinationMirror.GetTravelPosition();
            eventBus?.MirrorTravel();
            cachedCamera?.Pulse(0.35f, 0.3f);
            cachedCamera?.Shake(0.1f, 0.25f);
            cachedAudio?.PlayMemoryTransition();
            GameHaptics.TriggerHapticLight();
            cachedHud?.ShowToast("Teleported through reflection.", 3f);
        }
        else
        {
            cachedHud?.ShowToast("No reflective surfaces in range.", 3f);
            cachedAudio?.PlayDoorRattle();
        }
    }

    public void ManipulateShadows()
    {
        CacheServices(force: true);
        cachedHud?.ShowToast("Shadows stir... but refuse to obey yet.", 2.5f);
    }

    private IEnumerator GhostPhaseRoutine(float duration)
    {
        isGhostPhasing = true;
        GhostPhaseRemaining = duration;
        eventBus?.GhostPhaseStarted();
        GameHaptics.PhaseStart();
        cachedCamera?.Pulse(0.18f, 0.3f);
        cachedCamera?.Shake(0.05f, 0.2f);

        // Do NOT set player colliders to trigger — floors must stay solid.
        // SealedDoorPuzzle disables its own collider on GhostPhaseStarted.

        yield return new WaitForSeconds(duration);

        isGhostPhasing = false;
        GhostPhaseRemaining = 0f;
        ghostPhaseRoutine = null;
        rb.gravityScale = gravityScale;
        eventBus?.GhostPhaseEnded();
        cachedCamera?.Pulse(0.1f, 0.15f);
    }
}
