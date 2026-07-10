using UnityEngine;

/// <summary>
/// Commercial-quality 2.5D character animation director.
/// Multi-frame atlas playback + layered secondary motion + squash/stretch,
/// physics-synced walk/run/jump, turns, interact, hit, and emotion poses.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(PlayerController))]
[DefaultExecutionOrder(-20)]
public class PlayerSpriteAnimator : MonoBehaviour
{
    public enum AnimState
    {
        Idle, Walk, Run,
        JumpAnticipation, JumpRise, JumpApex, Fall, Land,
        Turn, Interact, Hit, Scare, Happy, Injured
    }

    [Header("Frame rates")]
    [SerializeField] private float idleFps = 10f;
    [SerializeField] private float walkFps = 12f;
    [SerializeField] private float runFps = 16f;
    [SerializeField] private float jumpFps = 14f;
    [SerializeField] private float landFps = 16f;

    [Header("Motion feel")]
    [SerializeField] private float walkThreshold = 0.12f;
    [SerializeField] private float runSpeedFactor = 0.72f;
    [SerializeField] private float landSquash = 0.16f;
    [SerializeField] private float jumpStretch = 0.1f;
    [SerializeField] private float squashRecover = 11f;

    private PlayerController player;
    private Rigidbody2D rb;
    private PlayerCharacterRig rig;
    private EventBus eventBus;

    private Sprite[] idleFrames = System.Array.Empty<Sprite>();
    private Sprite[] walkFrames = System.Array.Empty<Sprite>();
    private Sprite[] runFrames = System.Array.Empty<Sprite>();
    private Sprite[] jumpFrames = System.Array.Empty<Sprite>();
    private Sprite[] expressFrames = System.Array.Empty<Sprite>();
    private Sprite fallbackIdle, fallbackWalkA, fallbackWalkB, fallbackJump;

    private AnimState state = AnimState.Idle;
    private AnimState prevState = AnimState.Idle;
    private float frameTimer;
    private int frameIndex;
    private float stateTimer;
    private float landHold;
    private float interactTimer;
    private float hitTimer;
    private float expressionTimer;
    private AnimState expressionState = AnimState.Idle;
    private float jumpAnticipation;
    private bool jumpAnticipating;
    private float turnTimer;
    private int lastFacing = 1;
    private float squash;
    private bool ghostVisual;
    private Color baseColor = new(1.12f, 1.12f, 1.12f, 1f);
    private Color ghostTint = new(0.35f, 0.95f, 0.65f, 0.55f);
    private bool wasGrounded = true;
    private float airTime;

    public AnimState State => state;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();
        eventBus = Resources.Load<EventBus>("EventBus");

        rig = GetComponent<PlayerCharacterRig>();
        if (rig == null)
            rig = gameObject.AddComponent<PlayerCharacterRig>();

        // Disable legacy overlay systems — rig owns secondary life
        var idleDetail = GetComponent<PlayerIdleDetail>();
        if (idleDetail != null) idleDetail.enabled = false;

        LoadAtlases();
        ApplyFrame(idleFrames, 0, fallbackIdle);

        if (eventBus != null)
        {
            eventBus.OnGhostPhaseStarted += OnGhostStart;
            eventBus.OnGhostPhaseEnded += OnGhostEnd;
            eventBus.OnEchoCaught += OnHit;
            eventBus.OnEchoTriggered += OnScare;
        }
    }

    private void OnDestroy()
    {
        if (eventBus == null) return;
        eventBus.OnGhostPhaseStarted -= OnGhostStart;
        eventBus.OnGhostPhaseEnded -= OnGhostEnd;
        eventBus.OnEchoCaught -= OnHit;
        eventBus.OnEchoTriggered -= OnScare;
    }

    private void LoadAtlases()
    {
        idleFrames = PlayerSpriteAtlas.LoadGrid("Art/Characters/Atlases/atlas_idle", 4, 4);
        walkFrames = PlayerSpriteAtlas.LoadGrid("Art/Characters/Atlases/atlas_walk", 4, 4);
        runFrames = PlayerSpriteAtlas.LoadGrid("Art/Characters/Atlases/atlas_run", 4, 3);
        jumpFrames = PlayerSpriteAtlas.LoadGrid("Art/Characters/Atlases/atlas_jump", 4, 4);
        expressFrames = PlayerSpriteAtlas.LoadGrid("Art/Characters/Atlases/atlas_express", 4, 3);

        fallbackIdle = Resources.Load<Sprite>("Art/Characters/player_idle");
        fallbackWalkA = Resources.Load<Sprite>("Art/Characters/player_walk_a") ?? fallbackIdle;
        fallbackWalkB = Resources.Load<Sprite>("Art/Characters/player_walk_b") ?? fallbackWalkA;
        fallbackJump = Resources.Load<Sprite>("Art/Characters/player_jump") ?? fallbackIdle;

        if (idleFrames.Length == 0 && fallbackIdle != null)
            idleFrames = new[] { fallbackIdle };
        if (walkFrames.Length == 0)
            walkFrames = new[] { fallbackWalkA, fallbackWalkB };
        if (runFrames.Length == 0)
            runFrames = walkFrames;
        if (jumpFrames.Length == 0 && fallbackJump != null)
            jumpFrames = new[] { fallbackJump };
    }

    private void OnGhostStart()
    {
        ghostVisual = true;
        if (rig != null) rig.GhostMode = true;
    }

    private void OnGhostEnd()
    {
        ghostVisual = false;
        if (rig != null) rig.GhostMode = false;
        if (rig != null && rig.BodyRenderer != null)
            rig.BodyRenderer.color = baseColor;
    }

    private void OnHit()
    {
        hitTimer = 0.45f;
        state = AnimState.Hit;
        frameIndex = 0;
        squash = -0.14f;
        rig?.PulseHit();
        PlayExpression(AnimState.Injured, 0.6f);
    }

    private void OnScare()
    {
        PlayExpression(AnimState.Scare, 1.2f);
    }

    public void PlayInteractPose(float duration = 0.4f)
    {
        interactTimer = duration;
        state = AnimState.Interact;
        frameIndex = 0;
        frameTimer = 0f;
        PlayExpression(AnimState.Happy, duration);
    }

    public void PlayExpression(AnimState emotion, float duration)
    {
        expressionState = emotion;
        expressionTimer = duration;
    }

    private void Update()
    {
        if (player == null || rb == null) return;

        var vel = rb.linearVelocity;
        float absVx = Mathf.Abs(vel.x);
        bool grounded = player.IsGrounded;
        float dt = Time.deltaTime;

        // Landing detection
        if (grounded && !wasGrounded)
        {
            landHold = 0.16f;
            squash = -landSquash * Mathf.Clamp(Mathf.Abs(vel.y) / 8f, 0.5f, 1.35f);
            state = AnimState.Land;
            frameIndex = 0;
            airTime = 0f;
        }
        wasGrounded = grounded;
        if (!grounded) airTime += dt;
        else airTime = 0f;

        // Jump anticipation: buffer about to fire while grounded
        // Detect upward impulse start
        if (grounded && vel.y > player.jumpForce * 0.55f && !jumpAnticipating && jumpAnticipation <= 0f)
        {
            // Already left ground visually next frame; stretch now
            squash = jumpStretch;
        }

        // Turn detection
        int face = absVx > 0.15f ? (vel.x < 0f ? -1 : 1) : lastFacing;
        if (face != lastFacing && grounded && absVx > 0.2f)
        {
            turnTimer = 0.12f;
            state = AnimState.Turn;
        }
        if (absVx > 0.15f) lastFacing = face;

        // Priority state machine
        if (hitTimer > 0f)
        {
            hitTimer -= dt;
            state = AnimState.Hit;
        }
        else if (interactTimer > 0f)
        {
            interactTimer -= dt;
            state = AnimState.Interact;
        }
        else if (turnTimer > 0f)
        {
            turnTimer -= dt;
            state = AnimState.Turn;
        }
        else if (landHold > 0f)
        {
            landHold -= dt;
            state = AnimState.Land;
        }
        else if (!grounded)
        {
            if (vel.y > 1.2f) state = AnimState.JumpRise;
            else if (vel.y > -0.4f && airTime > 0.05f) state = AnimState.JumpApex;
            else state = AnimState.Fall;
        }
        else if (absVx < walkThreshold)
        {
            state = AnimState.Idle;
        }
        else
        {
            float runAt = player.moveSpeed * runSpeedFactor;
            state = absVx >= runAt ? AnimState.Run : AnimState.Walk;
        }

        if (state != prevState)
        {
            var previous = prevState;
            OnStateEnter(state, previous);
            bool keepStride =
                (state == AnimState.Walk || state == AnimState.Run) &&
                (previous == AnimState.Walk || previous == AnimState.Run);
            prevState = state;
            frameTimer = 0f;
            if (!keepStride)
                frameIndex = 0;
        }

        stateTimer += dt;
        AdvanceFrames(dt, absVx, vel.y, grounded);
        ApplySquashAndRig(dt, absVx, vel, grounded);
        ApplyColor();
        ApplyExpression();
    }

    private void OnStateEnter(AnimState next, AnimState prev)
    {
        switch (next)
        {
            case AnimState.JumpRise:
                squash = Mathf.Max(squash, jumpStretch);
                break;
            case AnimState.Land:
                // squash already set
                break;
            case AnimState.Idle:
                if (prev == AnimState.Land || prev == AnimState.Run || prev == AnimState.Walk)
                    rig?.SetSecondaryAmplitude(1f);
                break;
            case AnimState.Scare:
                squash = 0.05f;
                break;
        }
    }

    private void AdvanceFrames(float dt, float absVx, float vy, bool grounded)
    {
        Sprite[] frames;
        float fps;
        bool loop = true;

        switch (state)
        {
            case AnimState.Idle:
                frames = idleFrames;
                fps = idleFps;
                break;
            case AnimState.Walk:
                frames = walkFrames;
                fps = walkFps * Mathf.Clamp(absVx / 3.2f, 0.55f, 1.45f);
                break;
            case AnimState.Run:
                frames = runFrames;
                fps = runFps * Mathf.Clamp(absVx / 4.5f, 0.7f, 1.5f);
                break;
            case AnimState.JumpRise:
                frames = jumpFrames;
                fps = jumpFps;
                loop = false;
                frameIndex = Mathf.Clamp(jumpFrames.Length > 4 ? 2 : 0, 0, Mathf.Max(0, frames.Length - 1));
                ApplyFrame(frames, frameIndex, fallbackJump);
                UpdateCyclePhase(0.25f);
                return;
            case AnimState.JumpApex:
                frames = jumpFrames;
                frameIndex = Mathf.Clamp(jumpFrames.Length > 8 ? 6 : frames.Length / 2, 0, Mathf.Max(0, frames.Length - 1));
                ApplyFrame(frames, frameIndex, fallbackJump);
                UpdateCyclePhase(0.5f);
                return;
            case AnimState.Fall:
                frames = jumpFrames;
                frameIndex = Mathf.Clamp(jumpFrames.Length > 10 ? 10 : frames.Length - 2, 0, Mathf.Max(0, frames.Length - 1));
                ApplyFrame(frames, frameIndex, fallbackJump);
                UpdateCyclePhase(0.7f);
                return;
            case AnimState.Land:
                frames = jumpFrames.Length > 0 ? jumpFrames : idleFrames;
                fps = landFps;
                loop = false;
                // Use last quarter of jump sheet as land frames
                int landStart = Mathf.Max(0, frames.Length - 4);
                frameTimer += dt * fps;
                if (frameTimer >= 1f)
                {
                    frameTimer = 0f;
                    frameIndex++;
                }
                int li = landStart + Mathf.Clamp(frameIndex, 0, 3);
                ApplyFrame(frames, Mathf.Min(li, frames.Length - 1), fallbackIdle);
                UpdateCyclePhase(0f);
                return;
            case AnimState.Turn:
                frames = walkFrames.Length > 0 ? walkFrames : idleFrames;
                fps = walkFps * 1.2f;
                break;
            case AnimState.Interact:
                frames = expressFrames.Length > 8 ? expressFrames : idleFrames;
                fps = 10f;
                loop = false;
                // Prefer full-body interact cells (row 3 of express sheet = indices 8-11)
                if (expressFrames.Length >= 12)
                {
                    frameIndex = 8 + Mathf.Clamp(Mathf.FloorToInt((0.4f - interactTimer) / 0.4f * 3f), 0, 3);
                    ApplyFrame(expressFrames, frameIndex, fallbackIdle);
                    UpdateCyclePhase(0f);
                    return;
                }
                break;
            case AnimState.Hit:
                frames = expressFrames.Length > 9 ? expressFrames : idleFrames;
                fps = 14f;
                loop = false;
                if (expressFrames.Length >= 10)
                {
                    ApplyFrame(expressFrames, 9, fallbackIdle); // hit recoil cell
                    UpdateCyclePhase(0f);
                    return;
                }
                break;
            default:
                frames = idleFrames;
                fps = idleFps;
                break;
        }

        if (frames == null || frames.Length == 0)
        {
            ApplyFrame(null, 0, fallbackIdle);
            return;
        }

        frameTimer += dt * fps;
        if (frameTimer >= 1f)
        {
            frameTimer -= 1f;
            frameIndex++;
            // Contact squash pulse on walk/run foot plant (every half cycle)
            if (state == AnimState.Walk || state == AnimState.Run)
            {
                if (frameIndex % Mathf.Max(1, frames.Length / 2) == 0)
                    squash = Mathf.Min(squash, state == AnimState.Run ? -0.05f : -0.035f);
            }
        }

        if (loop)
            frameIndex %= frames.Length;
        else
            frameIndex = Mathf.Min(frameIndex, frames.Length - 1);

        ApplyFrame(frames, frameIndex, fallbackIdle);
        UpdateCyclePhase(frames.Length > 0 ? frameIndex / (float)frames.Length : 0f);
    }

    private void UpdateCyclePhase(float phase)
    {
        if (rig != null)
            rig.CyclePhase = phase;
    }

    private void ApplyFrame(Sprite[] frames, int index, Sprite fallback)
    {
        Sprite spr = null;
        if (frames != null && frames.Length > 0)
            spr = frames[Mathf.Clamp(index, 0, frames.Length - 1)];
        if (spr == null) spr = fallback;
        if (spr == null) return;

        if (rig != null)
            rig.SetBodySprite(spr);
        else
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.sprite = spr;
        }
    }

    private void ApplySquashAndRig(float dt, float absVx, Vector2 vel, bool grounded)
    {
        // Recover squash toward zero with nice overshoot damping
        squash = Mathf.Lerp(squash, 0f, dt * squashRecover);

        // Air stretch from vertical velocity
        float airStretch = 0f;
        if (!grounded)
            airStretch = Mathf.Clamp(vel.y * 0.02f, -0.06f, 0.1f);

        float energy = grounded ? Mathf.Clamp01(absVx / Mathf.Max(0.1f, player.moveSpeed)) : 0.35f;
        if (state == AnimState.Run) energy = Mathf.Max(energy, 0.85f);

        if (rig != null)
        {
            rig.SetSquash(squash);
            rig.AirStretch = airStretch;
            rig.MoveEnergy = energy;
            rig.SetSecondaryAmplitude(state == AnimState.Idle ? 1f : 0.75f);
            if (expressionTimer > 0f)
                rig.EmotionIntensity = Mathf.Lerp(rig.EmotionIntensity, 1f, dt * 6f);
            else
                rig.EmotionIntensity = Mathf.Lerp(rig.EmotionIntensity, 0f, dt * 4f);
        }
        else
        {
            // Fallback scale on root
            var baseScale = 1.55f;
            float sy = 1f + squash + airStretch;
            float sx = 1f - squash * 0.65f;
            int face = lastFacing;
            transform.localScale = new Vector3(baseScale * sx * face, baseScale * sy, 1f);
        }
    }

    private void ApplyColor()
    {
        var sr = rig != null ? rig.BodyRenderer : GetComponent<SpriteRenderer>();
        if (sr == null) return;
        if (ghostVisual) return; // rig handles ghost tint
        sr.color = Color.Lerp(sr.color, baseColor, Time.deltaTime * 8f);
    }

    private void ApplyExpression()
    {
        if (expressionTimer <= 0f)
        {
            rig?.SetExpressionOverlay(null, 0f);
            return;
        }

        expressionTimer -= Time.deltaTime;
        if (expressFrames.Length == 0 || rig == null) return;

        // Expression sheet row 0: happy, surprised, scared, angry
        int idx = expressionState switch
        {
            AnimState.Happy => 0,
            AnimState.Scare => 2,
            AnimState.Hit => 3,
            AnimState.Injured => 4,
            _ => 7
        };
        idx = Mathf.Clamp(idx, 0, expressFrames.Length - 1);
        float a = Mathf.Clamp01(expressionTimer * 2f) * 0.55f;
        rig.SetExpressionOverlay(expressFrames[idx], a);
    }
}
