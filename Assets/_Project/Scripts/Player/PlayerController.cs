using System.Collections;
using UnityEngine;

/// <summary>
/// Chapter 1 side-scroller controller: acceleration, jump assist, wall slide,
/// unified Ghost phase (collision ignore + timer), and mirror travel.
/// Driven by TouchGameplayController (left/right hold) for reliable mobile play.
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
    public float ghostMoveMultiplier = 0.9f;
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
    private float noiseStepInterval = 0.42f;
    private float noiseTimer;
    private float moveInput;
    private float coyoteCounter;
    private float jumpBufferCounter;
    private bool jumpHeld;
    private bool jumpCutApplied;
    private float landTimer;
    private int groundMask;
    private float ghostPhaseRemaining;
    private Collider2D[] ignoredPhaseColliders;
    private bool inputLocked;

    public bool IsGhostPhasing => isGhostPhasing;
    public bool IsGrounded => isGrounded;
    public bool IsWallSliding => touchingWall && !isGrounded && rb != null && rb.linearVelocity.y < 0f;
    public bool JumpHeld => jumpHeld;
    public bool JustLanded => landTimer > 0f;
    public float HorizontalSpeed => rb != null ? Mathf.Abs(rb.linearVelocity.x) : 0f;
    public float MoveInput => moveInput;
    public Vector2 Velocity => rb != null ? rb.linearVelocity : Vector2.zero;
    public int WallSign => wallSign;
    public float GhostPhaseRemaining => ghostPhaseRemaining;
    public bool AnimationDrivesFootsteps { get; set; }
    public bool IsInteracting
    {
        get => inputLocked;
        set => inputLocked = value;
    }

    private void Awake()
    {
        Application.targetFrameRate = 60;
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
        eventBus = Resources.Load<EventBus>("EventBus");

        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation = true;
        rb.gravityScale = gravityScale;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.simulated = true;

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

        if (isGhostPhasing)
            ghostPhaseRemaining = Mathf.Max(0f, ghostPhaseRemaining - Time.deltaTime);

        if (!isGhostPhasing && !inputLocked)
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
    }

    private void TickFootstepNoise()
    {
        if (isGrounded && Mathf.Abs(rb.linearVelocity.x) > 0.4f)
        {
            noiseTimer += Time.deltaTime;
            if (noiseTimer >= noiseStepInterval)
            {
                noiseTimer = 0f;
                EmitFootstepNoise(2.2f);
            }
        }
        else noiseTimer = noiseStepInterval;
    }

    public void EmitFootstepNoise(float radius)
    {
        eventBus?.NoiseHeard(transform.position, radius);
        FindFirstObjectByType<GameAudioController>()?.PlayFootstep();
    }

    public void Move(float horizontalInput)
    {
        if (inputLocked)
        {
            moveInput = 0f;
            return;
        }
        moveInput = Mathf.Clamp(horizontalInput, -1f, 1f);
    }

    public void SetJumpHeld(bool held) => jumpHeld = held;

    public void Jump()
    {
        if (inputLocked || isGhostPhasing) return;
        jumpBufferCounter = jumpBufferTime;
        jumpHeld = true;
        TryConsumeJumpBuffer();
    }

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
            newX = targetX; // snappy while ethereal
        }
        else if (isGrounded)
        {
            float rate = Mathf.Abs(targetX) > 0.01f ? groundAccel : groundDecel;
            if (Mathf.Abs(targetX) > 0.01f && Mathf.Sign(targetX) != Mathf.Sign(currentX) && Mathf.Abs(currentX) > 0.2f)
                rate *= turnBoost;
            newX = Mathf.MoveTowards(currentX, targetX, rate * Time.fixedDeltaTime);
            if (Mathf.Abs(newX - targetX) < 0.05f) newX = targetX;
        }
        else
        {
            float rate = airAccel * airControl;
            newX = Mathf.MoveTowards(currentX, targetX, rate * Time.fixedDeltaTime);
        }

        float newY = rb.linearVelocity.y;
        if (touchingWall && !isGrounded && !isGhostPhasing && newY < 0f &&
            Mathf.Abs(moveInput) > 0.1f && Mathf.Sign(moveInput) == wallSign)
            newY = Mathf.Max(newY, -wallSlideSpeed);

        newY = Mathf.Max(newY, -maxFallSpeed);
        rb.linearVelocity = new Vector2(newX, newY);
    }

    private void ProbeContacts()
    {
        isGrounded = false;
        touchingWall = false;
        wallSign = 0;
        if (bodyCollider == null || isGhostPhasing) return;

        var bounds = bodyCollider.bounds;

        Vector2 gOrigin = new Vector2(bounds.center.x, bounds.min.y + 0.02f);
        Vector2 gSize = new Vector2(Mathf.Max(0.08f, bounds.size.x * 0.7f), 0.08f);
        var gHit = Physics2D.BoxCast(gOrigin, gSize, 0f, Vector2.down, 0.08f, groundMask);
        if (gHit.collider != null && !gHit.collider.isTrigger && gHit.collider.attachedRigidbody != rb)
        {
            if (gHit.normal.y > 0.35f)
                isGrounded = true;
        }

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

    private void TryConsumeJumpBuffer()
    {
        if (jumpBufferCounter <= 0f) return;
        if (isGhostPhasing || inputLocked) return;
        if (coyoteCounter <= 0f && !isGrounded) return;

        jumpBufferCounter = 0f;
        coyoteCounter = 0f;
        jumpCutApplied = false;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        eventBus?.NoiseHeard(transform.position, 5f);
        FindFirstObjectByType<CameraFollow2D>()?.Pulse(0.04f, 0.08f);
    }

    /// <summary>Unified Ghost Key phase — used by KeyManager, GhostKey, HUD Use Key.</summary>
    public void ActivateGhostPhase(float duration)
    {
        float d = duration > 0f ? duration : ghostPhaseDuration;
        if (ghostPhaseRoutine != null)
            StopCoroutine(ghostPhaseRoutine);
        ghostPhaseRoutine = StartCoroutine(GhostPhaseRoutine(d));
    }

    private IEnumerator GhostPhaseRoutine(float duration)
    {
        isGhostPhasing = true;
        ghostPhaseRemaining = duration;
        eventBus?.GhostPhaseStarted();
        GameHaptics.PhaseStart();
        FindFirstObjectByType<CameraFollow2D>()?.Pulse(0.18f, 0.3f);
        FindFirstObjectByType<CameraFollow2D>()?.Shake(0.05f, 0.2f);

        // Ignore solid colliders that are not triggers so player can pass sealed doors / walls
        BeginPhaseCollisions();

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            ghostPhaseRemaining = duration - t;
            yield return null;
        }

        EndPhaseCollisions();
        // Safe eject if stuck inside a solid
        SafeEjectFromSolids();

        isGhostPhasing = false;
        ghostPhaseRemaining = 0f;
        ghostPhaseRoutine = null;
        eventBus?.GhostPhaseEnded();
        FindFirstObjectByType<CameraFollow2D>()?.Pulse(0.1f, 0.15f);
        FindFirstObjectByType<GameplayHUD>()?.ShowToast("Phase ends — solid again.", 1.8f);
    }

    private void BeginPhaseCollisions()
    {
        if (bodyCollider == null) return;
        var solids = FindObjectsByType<Collider2D>(FindObjectsSortMode.None);
        var list = new System.Collections.Generic.List<Collider2D>();
        foreach (var c in solids)
        {
            if (c == null || c == bodyCollider || c.isTrigger) continue;
            if (c.attachedRigidbody == rb) continue;
            // Keep floors so player doesn't fall forever — only ignore vertical blockers with low normal.y preference:
            // Heuristic: ignore colliders taller than wide (doors/walls) or tagged via name
            var b = c.bounds;
            bool wallLike = b.size.y > b.size.x * 1.15f ||
                            c.name.IndexOf("Door", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                            c.name.IndexOf("Sealed", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                            c.name.IndexOf("Wall", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                            c.name.IndexOf("Brick", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                            c.name.IndexOf("Hidden", System.StringComparison.OrdinalIgnoreCase) >= 0;
            if (!wallLike) continue;
            Physics2D.IgnoreCollision(bodyCollider, c, true);
            list.Add(c);
        }
        ignoredPhaseColliders = list.ToArray();
    }

    private void EndPhaseCollisions()
    {
        if (bodyCollider == null || ignoredPhaseColliders == null) return;
        foreach (var c in ignoredPhaseColliders)
        {
            if (c != null)
                Physics2D.IgnoreCollision(bodyCollider, c, false);
        }
        ignoredPhaseColliders = null;
    }

    private void SafeEjectFromSolids()
    {
        if (bodyCollider == null) return;
        var filter = new ContactFilter2D { useTriggers = false, useLayerMask = true, layerMask = groundMask };
        var hits = new Collider2D[8];
        int n = bodyCollider.Overlap(filter, hits);
        if (n <= 0) return;

        // Nudge right/left until free
        for (var i = 0; i < 12; i++)
        {
            transform.position += Vector3.right * 0.15f;
            n = bodyCollider.Overlap(filter, hits);
            if (n == 0) return;
        }
        for (var i = 0; i < 24; i++)
        {
            transform.position += Vector3.left * 0.15f;
            n = bodyCollider.Overlap(filter, hits);
            if (n == 0) return;
        }
    }

    public bool TryMirrorTravel()
    {
        var colliders = Physics2D.OverlapCircleAll(transform.position, 3.8f);
        MirrorSurface nearest = null;
        float minDist = float.MaxValue;
        foreach (var col in colliders)
        {
            var mirror = col.GetComponent<MirrorSurface>();
            if (mirror == null || !mirror.isReflective || mirror.destinationMirror == null) continue;
            float dist = Vector2.Distance(transform.position, mirror.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = mirror;
            }
        }

        var hud = FindFirstObjectByType<GameplayHUD>();
        var km = FindFirstObjectByType<KeyManager>();
        bool hasMirror = km != null && km.ownedKeys.Exists(k => k.abilityType == KeyManager.KeyAbilityType.MirrorTravel);
        if (!hasMirror)
        {
            hud?.ShowToast("You need the Mirror Key to travel reflections.", 2.8f);
            return false;
        }

        if (nearest != null)
        {
            transform.position = nearest.destinationMirror.GetTravelPosition();
            FindFirstObjectByType<CameraFollow2D>()?.Pulse(0.35f, 0.3f);
            FindFirstObjectByType<CameraFollow2D>()?.Shake(0.1f, 0.25f);
            FindFirstObjectByType<GameAudioController>()?.PlayMemoryTransition();
            GameHaptics.TriggerHapticLight();
            eventBus?.MirrorTravel();
            FindFirstObjectByType<PlayerSpriteAnimator>()?.PlayMirrorTravel(0.55f);
            hud?.ShowToast("You step through the glass…", 2.5f);
            return true;
        }

        hud?.ShowToast("No linked mirror nearby. Find a reflective surface.", 3f);
        FindFirstObjectByType<GameAudioController>()?.PlayDoorRattle();
        return false;
    }

    public void ManipulateShadows(bool active = true)
    {
        FindFirstObjectByType<GameplayHUD>()?.ShowToast("Shadows stir… but refuse to obey yet.", 2.5f);
    }
}
