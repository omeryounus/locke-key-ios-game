using System;
using UnityEngine;

/// <summary>
/// Production 2.5D animation director tuned to PlayerController physics and Keyhouse abilities.
/// Locomotion atlases + Ghost/Head/Hide/Echo/Mirror/WallSlide states, footstep-frame sync,
/// jump-cut awareness, and brave-but-vulnerable emotional beats.
/// Dual-path: fires AnimEvents for Animator graph bridge + foot-contact VFX.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(PlayerController))]
[DefaultExecutionOrder(-20)]
public class PlayerSpriteAnimator : MonoBehaviour
{
    public enum AnimState
    {
        Idle, Walk, Run,
        JumpRise, JumpApex, Fall, Land,
        Turn, WallSlide,
        Interact, HouseKeyInteract,
        Hit, ScareFlinch, ScareFreeze, ScareRecover,
        GhostEnter, GhostLoop, GhostExit,
        Mindscape, Hide, MirrorTravel,
        Happy, Injured
    }

    public enum AnimEventKind { FootPlant, LandImpact, JumpTakeoff, Interact, Hit }

    [Header("Frame rates")]
    [SerializeField] private float idleFps = 10f;
    [SerializeField] private float walkFps = 12f;
    [SerializeField] private float runFps = 16f;
    [SerializeField] private float jumpFps = 14f;
    [SerializeField] private float landFps = 16f;

    [Header("Motion feel")]
    [SerializeField] private float walkThreshold = 0.18f;
    [SerializeField] private float runSpeedFactor = 0.72f;
    [SerializeField] private float landSquash = 0.16f;
    [SerializeField] private float jumpStretch = 0.1f;
    [SerializeField] private float squashRecover = 12f;
    [Tooltip("Hysteresis so idle/walk doesn't flicker at the threshold.")]
    [SerializeField] private float stopThreshold = 0.1f;

    private PlayerController player;
    private Rigidbody2D rb;
    private PlayerCharacterRig rig;
    private EventBus eventBus;

    private Sprite[] idleFrames = System.Array.Empty<Sprite>();
    private Sprite[] walkFrames = System.Array.Empty<Sprite>();
    private Sprite[] runFrames = System.Array.Empty<Sprite>();
    private Sprite[] jumpFrames = System.Array.Empty<Sprite>();
    private Sprite[] expressFrames = System.Array.Empty<Sprite>();
    private Sprite[] ghostFrames = System.Array.Empty<Sprite>();
    private Sprite[] scareFrames = System.Array.Empty<Sprite>();
    private Sprite[] hideFrames = System.Array.Empty<Sprite>();
    private Sprite fallbackIdle, fallbackWalkA, fallbackWalkB, fallbackJump;

    private AnimState state = AnimState.Idle;
    private AnimState prevState = AnimState.Idle;
    private float frameTimer;
    private int frameIndex;
    private int lastFootstepFrame = -1;
    private float landHold;
    private float interactTimer;
    private bool houseKeyInteract;
    private float hitTimer;
    private float scareTimer;
    private float scarePhase; // 0 flinch, 1 freeze, 2 recover
    private float expressionTimer;
    private AnimState expressionState = AnimState.Idle;
    private float turnTimer;
    private int lastFacing = 1;
    private float squash;
    private bool wasGrounded = true;
    private float airTime;
    private bool ghostActive;
    private float ghostEnterTimer;
    private float ghostExitTimer;
    private bool mindscapeActive;
    private bool hideActive;
    private float mirrorTimer;
    private Color baseColor = new(1.12f, 1.12f, 1.12f, 1f);

    public AnimState State => state;
    /// <summary>Contact / impact events for dual-path Animator + VFX listeners.</summary>
    public event Action<AnimEventKind> OnAnimEvent;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();
        eventBus = Resources.Load<EventBus>("EventBus");

        rig = GetComponent<PlayerCharacterRig>();
        if (rig == null)
            rig = gameObject.AddComponent<PlayerCharacterRig>();

        var idleDetail = GetComponent<PlayerIdleDetail>();
        if (idleDetail != null) idleDetail.enabled = false;
        PlayerCharacterRig.DisableLegacyVisualLayers(transform);

        if (player != null)
            player.AnimationDrivesFootsteps = true;

        LoadAtlases();
        ApplyFrame(idleFrames, 0, fallbackIdle);
        Subscribe();
    }

    private void OnDestroy() => Unsubscribe();

    private void Subscribe()
    {
        if (eventBus == null) return;
        eventBus.OnGhostPhaseStarted += OnGhostStart;
        eventBus.OnGhostPhaseEnded += OnGhostEnd;
        eventBus.OnEchoCaught += OnHit;
        eventBus.OnEchoTriggered += OnScare;
        eventBus.OnMindscapeEntered += OnMindscapeEnter;
        eventBus.OnMindscapeExited += OnMindscapeExit;
        eventBus.OnMirrorTravel += OnMirror;
        eventBus.OnHideEntered += OnHideEnter;
        eventBus.OnHideExited += OnHideExit;
        eventBus.OnKeyActivated += OnKeyActivated;
    }

    private void Unsubscribe()
    {
        if (eventBus == null) return;
        eventBus.OnGhostPhaseStarted -= OnGhostStart;
        eventBus.OnGhostPhaseEnded -= OnGhostEnd;
        eventBus.OnEchoCaught -= OnHit;
        eventBus.OnEchoTriggered -= OnScare;
        eventBus.OnMindscapeEntered -= OnMindscapeEnter;
        eventBus.OnMindscapeExited -= OnMindscapeExit;
        eventBus.OnMirrorTravel -= OnMirror;
        eventBus.OnHideEntered -= OnHideEnter;
        eventBus.OnHideExited -= OnHideExit;
        eventBus.OnKeyActivated -= OnKeyActivated;
    }

    private void LoadAtlases()
    {
        idleFrames = PlayerSpriteAtlas.LoadGrid("Art/Characters/Atlases/atlas_idle", 4, 4);
        walkFrames = PlayerSpriteAtlas.LoadGrid("Art/Characters/Atlases/atlas_walk", 4, 4);
        runFrames = PlayerSpriteAtlas.LoadGrid("Art/Characters/Atlases/atlas_run", 4, 3);
        jumpFrames = PlayerSpriteAtlas.LoadGrid("Art/Characters/Atlases/atlas_jump", 4, 4);
        expressFrames = PlayerSpriteAtlas.LoadGrid("Art/Characters/Atlases/atlas_express", 4, 3);
        ghostFrames = PlayerSpriteAtlas.LoadGrid("Art/Characters/Atlases/atlas_ghost", 4, 2);
        scareFrames = PlayerSpriteAtlas.LoadGrid("Art/Characters/Atlases/atlas_scare", 4, 3);
        hideFrames = PlayerSpriteAtlas.LoadGrid("Art/Characters/Atlases/atlas_hide", 4, 2);

        fallbackIdle = Resources.Load<Sprite>("Art/Characters/player_idle");
        fallbackWalkA = Resources.Load<Sprite>("Art/Characters/player_walk_a") ?? fallbackIdle;
        fallbackWalkB = Resources.Load<Sprite>("Art/Characters/player_walk_b") ?? fallbackWalkA;
        fallbackJump = Resources.Load<Sprite>("Art/Characters/player_jump") ?? fallbackIdle;

        if (idleFrames.Length == 0 && fallbackIdle != null) idleFrames = new[] { fallbackIdle };
        if (walkFrames.Length == 0) walkFrames = new[] { fallbackWalkA, fallbackWalkB };
        if (runFrames.Length == 0) runFrames = walkFrames;
        if (jumpFrames.Length == 0 && fallbackJump != null) jumpFrames = new[] { fallbackJump };
        if (ghostFrames.Length == 0) ghostFrames = idleFrames;
        if (scareFrames.Length == 0) scareFrames = expressFrames.Length > 0 ? expressFrames : idleFrames;
        if (hideFrames.Length == 0) hideFrames = idleFrames;
    }

    private void OnGhostStart()
    {
        ghostActive = true;
        ghostEnterTimer = 0.22f;
        ghostExitTimer = 0f;
        state = AnimState.GhostEnter;
        frameIndex = 0;
        rig?.SetMode(PlayerCharacterRig.VisualMode.Ghost);
    }

    private void OnGhostEnd()
    {
        ghostActive = false;
        ghostExitTimer = 0.18f;
        state = AnimState.GhostExit;
        frameIndex = 0;
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
        // Flinch → freeze (stillness) → recover shaky breath
        scareTimer = 2.4f;
        scarePhase = 0f;
        state = AnimState.ScareFlinch;
        frameIndex = 0;
        squash = 0.06f;
        rig?.BeginScareFreeze();
    }

    private void OnMindscapeEnter()
    {
        mindscapeActive = true;
        state = AnimState.Mindscape;
        rig?.SetMode(PlayerCharacterRig.VisualMode.Mindscape);
        PlayExpression(AnimState.Happy, 0.3f);
    }

    private void OnMindscapeExit()
    {
        mindscapeActive = false;
        // Snap-back flinch + blink
        squash = -0.06f;
        interactTimer = 0.2f;
        rig?.SetMode(PlayerCharacterRig.VisualMode.Normal);
        rig?.SetSecondaryAmplitude(1f);
    }

    private void OnMirror()
    {
        mirrorTimer = 0.35f;
        state = AnimState.MirrorTravel;
        rig?.SetMode(PlayerCharacterRig.VisualMode.MirrorFlash);
    }

    private void OnHideEnter()
    {
        hideActive = true;
        state = AnimState.Hide;
        frameIndex = 0;
        rig?.SetMode(PlayerCharacterRig.VisualMode.Hide);
        rig?.SetSecondaryAmplitude(0.35f);
    }

    private void OnHideExit()
    {
        hideActive = false;
        rig?.SetMode(PlayerCharacterRig.VisualMode.Normal);
        rig?.SetSecondaryAmplitude(1f);
    }

    private void OnKeyActivated(IKeyAbility key)
    {
        if (key == null) return;
        if (key.Type == KeyType.Head)
        {
            // Head key often opens mindscape — entranced pose if not already
            if (!mindscapeActive)
                PlayExpression(AnimState.Happy, 0.5f);
        }
        else if (key.Type == KeyType.Ghost)
        {
            // ghost handled via phase events
        }
    }

    public void PlayInteractPose(float duration = 0.4f)
    {
        interactTimer = duration;
        houseKeyInteract = false;
        state = AnimState.Interact;
        frameIndex = 0;
        frameTimer = 0f;
        PlayExpression(AnimState.Happy, duration);
    }

    /// <summary>Deliberate House Key door reach — slightly longer confidence beat.</summary>
    public void PlayHouseKeyInteract(float duration = 0.55f)
    {
        interactTimer = duration;
        houseKeyInteract = true;
        state = AnimState.HouseKeyInteract;
        frameIndex = 0;
        rig?.SetKeyPropVisible(true, new Color(0.95f, 0.82f, 0.35f));
        PlayExpression(AnimState.Happy, duration);
    }

    /// <summary>Mirror Key travel pose flash (step-through beat).</summary>
    public void PlayMirrorTravel(float duration = 0.55f)
    {
        mirrorTimer = Mathf.Max(0.2f, duration);
        state = AnimState.MirrorTravel;
        frameIndex = 0;
        frameTimer = 0f;
        rig?.SetMode(PlayerCharacterRig.VisualMode.MirrorFlash);
        PlayExpression(AnimState.Happy, duration * 0.5f);
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

        // Landing
        if (grounded && !wasGrounded)
        {
            float impact = Mathf.Clamp(Mathf.Abs(vel.y) / 8f, 0.45f, 1.4f);
            landHold = 0.14f + impact * 0.04f;
            // Soft vs hard land squash (two intensities)
            squash = -landSquash * (impact < 0.75f ? 0.7f : 1.15f);
            state = AnimState.Land;
            frameIndex = 0;
            airTime = 0f;
            OnAnimEvent?.Invoke(AnimEventKind.LandImpact);
        }
        wasGrounded = grounded;
        if (!grounded) airTime += dt;
        else airTime = 0f;

        // Turn
        int face = absVx > 0.15f ? (vel.x < 0f ? -1 : 1) : lastFacing;
        if (face != lastFacing && grounded && absVx > 0.2f && !AbilityLocksLocomotion())
        {
            turnTimer = 0.1f;
            state = AnimState.Turn;
        }
        if (absVx > 0.15f) lastFacing = face;

        // Ghost warning last 1s
        if (ghostActive && player.IsGhostPhasing && rig != null)
        {
            float rem = player.GhostPhaseRemaining;
            rig.GhostWarning = rem > 0f && rem < 1f ? 1f - rem : 0f;
        }
        else if (rig != null) rig.GhostWarning = 0f;

        ResolveState(dt, absVx, vel, grounded);

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
            {
                frameIndex = 0;
                lastFootstepFrame = -1;
            }
        }

        AdvanceFrames(dt, absVx, vel, grounded);
        ApplySquashAndRig(dt, absVx, vel, grounded);
        ApplyExpression();
    }

    private bool AbilityLocksLocomotion() =>
        hitTimer > 0f || scareTimer > 0f || mindscapeActive || hideActive
        || mirrorTimer > 0f || ghostEnterTimer > 0f || ghostExitTimer > 0f;

    private void ResolveState(float dt, float absVx, Vector2 vel, bool grounded)
    {
        // Highest priority: ability / reaction states
        if (mirrorTimer > 0f)
        {
            mirrorTimer -= dt;
            state = AnimState.MirrorTravel;
            if (mirrorTimer <= 0f)
                rig?.SetMode(ghostActive ? PlayerCharacterRig.VisualMode.Ghost : PlayerCharacterRig.VisualMode.Normal);
            return;
        }

        if (hitTimer > 0f)
        {
            hitTimer -= dt;
            state = AnimState.Hit;
            return;
        }

        if (scareTimer > 0f)
        {
            scareTimer -= dt;
            // 0–0.12 flinch, 0.12–1.1 freeze, rest recover
            float elapsed = 2.4f - scareTimer;
            if (elapsed < 0.12f)
            {
                scarePhase = 0f;
                state = AnimState.ScareFlinch;
            }
            else if (elapsed < 1.15f)
            {
                scarePhase = 1f;
                state = AnimState.ScareFreeze;
                rig?.BeginScareFreeze();
            }
            else
            {
                scarePhase = 2f;
                state = AnimState.ScareRecover;
                rig?.EndScareFreeze();
            }
            if (scareTimer <= 0f)
            {
                rig?.EndScareFreeze();
                rig?.SetMode(PlayerCharacterRig.VisualMode.Normal);
            }
            return;
        }

        if (ghostEnterTimer > 0f)
        {
            ghostEnterTimer -= dt;
            state = AnimState.GhostEnter;
            return;
        }

        if (ghostExitTimer > 0f)
        {
            ghostExitTimer -= dt;
            state = AnimState.GhostExit;
            if (ghostExitTimer <= 0f)
                rig?.SetMode(PlayerCharacterRig.VisualMode.Normal);
            return;
        }

        if (ghostActive && player.IsGhostPhasing)
        {
            state = AnimState.GhostLoop;
            rig?.SetMode(PlayerCharacterRig.VisualMode.Ghost);
            return;
        }

        if (mindscapeActive)
        {
            state = AnimState.Mindscape;
            return;
        }

        if (hideActive || HideSpot.IsPlayerHidden)
        {
            if (!hideActive && HideSpot.IsPlayerHidden)
                OnHideEnter();
            state = AnimState.Hide;
            return;
        }

        if (interactTimer > 0f)
        {
            interactTimer -= dt;
            state = houseKeyInteract ? AnimState.HouseKeyInteract : AnimState.Interact;
            if (interactTimer <= 0f)
            {
                houseKeyInteract = false;
                rig?.SetKeyPropVisible(false);
            }
            return;
        }

        if (turnTimer > 0f)
        {
            turnTimer -= dt;
            state = AnimState.Turn;
            return;
        }

        if (landHold > 0f)
        {
            landHold -= dt;
            state = AnimState.Land;
            return;
        }

        if (player.IsWallSliding)
        {
            state = AnimState.WallSlide;
            return;
        }

        if (!grounded)
        {
            // Jump cut: early release → snap toward apex sooner
            bool shortHop = !player.JumpHeld && vel.y > 0.5f && vel.y < player.jumpForce * 0.85f;
            if (vel.y > 1.2f && !shortHop) state = AnimState.JumpRise;
            else if (vel.y > -0.35f || shortHop) state = AnimState.JumpApex;
            else state = AnimState.Fall;
            return;
        }

        // Hysteresis: harder to leave idle than enter walk — kills foot skate flicker
        float enterWalk = walkThreshold;
        float leaveWalk = stopThreshold;
        if (prevState == AnimState.Idle || prevState == AnimState.Land)
        {
            if (absVx < enterWalk)
            {
                state = AnimState.Idle;
                return;
            }
        }
        else if (state == AnimState.Idle || absVx < leaveWalk)
        {
            if (absVx < leaveWalk)
            {
                state = AnimState.Idle;
                return;
            }
        }

        if (absVx < enterWalk)
        {
            state = AnimState.Idle;
            return;
        }

        float runAt = player.moveSpeed * runSpeedFactor;
        // Hysteresis on run too
        float runEnter = runAt;
        float runLeave = runAt * 0.88f;
        if (prevState == AnimState.Run)
            state = absVx >= runLeave ? AnimState.Run : AnimState.Walk;
        else
            state = absVx >= runEnter ? AnimState.Run : AnimState.Walk;
    }

    private void OnStateEnter(AnimState next, AnimState prev)
    {
        switch (next)
        {
            case AnimState.JumpRise:
                squash = Mathf.Max(squash, jumpStretch);
                break;
            case AnimState.WallSlide:
                squash = Mathf.Lerp(squash, 0.04f, 0.5f);
                break;
            case AnimState.Hide:
                squash = -0.06f;
                break;
            case AnimState.Idle:
                if (prev == AnimState.Land || prev == AnimState.Run || prev == AnimState.Walk)
                    rig?.SetSecondaryAmplitude(1f);
                break;
        }
    }

    private void AdvanceFrames(float dt, float absVx, Vector2 vel, bool grounded)
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
                // Match cycle rate to world speed to reduce foot skating
                fps = walkFps * Mathf.Clamp(absVx / Mathf.Max(0.5f, player.moveSpeed * 0.55f), 0.5f, 1.5f);
                break;
            case AnimState.Run:
                frames = runFrames;
                fps = runFps * Mathf.Clamp(absVx / Mathf.Max(0.5f, player.moveSpeed * 0.85f), 0.65f, 1.55f);
                break;
            case AnimState.GhostLoop:
                // Reuse walk/idle at 0.9× speed with float feel
                frames = absVx > walkThreshold ? walkFrames : idleFrames;
                fps = (absVx > walkThreshold ? walkFps : idleFps) * 0.9f;
                break;
            case AnimState.GhostEnter:
                frames = ghostFrames.Length > 0 ? ghostFrames : idleFrames;
                fps = 14f;
                loop = false;
                break;
            case AnimState.GhostExit:
                frames = ghostFrames.Length > 0 ? ghostFrames : idleFrames;
                fps = 16f;
                loop = false;
                break;
            case AnimState.JumpRise:
            {
                frames = jumpFrames;
                int idx = Mathf.Clamp(jumpFrames.Length > 4 ? 2 : 0, 0, Mathf.Max(0, frames.Length - 1));
                // Fall-velocity stretch param for hard drops uses same frames
                ApplyFrame(frames, idx, fallbackJump);
                UpdateCyclePhase(0.25f);
                return;
            }
            case AnimState.JumpApex:
            {
                frames = jumpFrames;
                int idx = Mathf.Clamp(jumpFrames.Length > 8 ? 6 : frames.Length / 2, 0, Mathf.Max(0, frames.Length - 1));
                ApplyFrame(frames, idx, fallbackJump);
                UpdateCyclePhase(0.5f);
                return;
            }
            case AnimState.Fall:
            {
                frames = jumpFrames;
                int idx = Mathf.Clamp(jumpFrames.Length > 10 ? 10 : frames.Length - 2, 0, Mathf.Max(0, frames.Length - 1));
                ApplyFrame(frames, idx, fallbackJump);
                UpdateCyclePhase(0.7f);
                // Stretch from fall speed toward maxFall 14
                float fallT = Mathf.Clamp01(Mathf.Abs(vel.y) / Mathf.Max(0.1f, player.maxFallSpeed));
                squash = Mathf.Lerp(squash, jumpStretch * 0.35f * fallT, dt * 6f);
                return;
            }
            case AnimState.Land:
            {
                frames = jumpFrames.Length > 0 ? jumpFrames : idleFrames;
                int landStart = Mathf.Max(0, frames.Length - 4);
                frameTimer += dt * landFps;
                if (frameTimer >= 1f) { frameTimer = 0f; frameIndex++; }
                int li = landStart + Mathf.Clamp(frameIndex, 0, 3);
                ApplyFrame(frames, Mathf.Min(li, frames.Length - 1), fallbackIdle);
                UpdateCyclePhase(0f);
                return;
            }
            case AnimState.Turn:
                frames = walkFrames.Length > 0 ? walkFrames : idleFrames;
                fps = walkFps * 1.35f;
                break;
            case AnimState.WallSlide:
                frames = jumpFrames.Length > 0 ? jumpFrames : idleFrames;
                // Pressed pose — hold mid-fall cell
                ApplyFrame(frames, Mathf.Clamp(frames.Length > 8 ? 9 : frames.Length / 2, 0, frames.Length - 1), fallbackJump);
                UpdateCyclePhase(0.6f);
                return;
            case AnimState.Interact:
            case AnimState.HouseKeyInteract:
                frames = expressFrames.Length > 8 ? expressFrames : idleFrames;
                if (expressFrames.Length >= 12)
                {
                    // Confidence pause for house key: hold frame 8 longer
                    float t = houseKeyInteract ? (0.55f - interactTimer) / 0.55f : (0.4f - interactTimer) / 0.4f;
                    if (houseKeyInteract && t < 0.25f)
                        frameIndex = 8;
                    else
                        frameIndex = 8 + Mathf.Clamp(Mathf.FloorToInt(t * 3f), 0, 3);
                    ApplyFrame(expressFrames, frameIndex, fallbackIdle);
                    UpdateCyclePhase(0f);
                    return;
                }
                fps = 10f;
                loop = false;
                break;
            case AnimState.Hit:
                if (expressFrames.Length >= 10)
                {
                    ApplyFrame(expressFrames, 9, fallbackIdle);
                    UpdateCyclePhase(0f);
                    return;
                }
                frames = scareFrames;
                fps = 14f;
                loop = false;
                break;
            case AnimState.ScareFlinch:
                frames = scareFrames;
                ApplyFrame(frames, 0, fallbackIdle);
                UpdateCyclePhase(0f);
                return;
            case AnimState.ScareFreeze:
                frames = scareFrames;
                ApplyFrame(frames, Mathf.Min(2, frames.Length - 1), fallbackIdle);
                UpdateCyclePhase(0f);
                return;
            case AnimState.ScareRecover:
                frames = scareFrames.Length > 4 ? scareFrames : idleFrames;
                fps = 9f;
                break;
            case AnimState.Mindscape:
                frames = expressFrames.Length > 0 ? expressFrames : idleFrames;
                ApplyFrame(frames, Mathf.Min(1, frames.Length - 1), fallbackIdle);
                UpdateCyclePhase(0f);
                return;
            case AnimState.Hide:
                frames = hideFrames;
                fps = 6f;
                break;
            case AnimState.MirrorTravel:
                frames = jumpFrames.Length > 0 ? jumpFrames : idleFrames;
                ApplyFrame(frames, Mathf.Min(4, frames.Length - 1), fallbackJump);
                UpdateCyclePhase(0.3f);
                return;
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
            if (state == AnimState.Walk || state == AnimState.Run || state == AnimState.GhostLoop)
            {
                int half = Mathf.Max(1, frames.Length / 2);
                if (frameIndex % half == 0)
                    squash = Mathf.Min(squash, state == AnimState.Run ? -0.05f : -0.035f);
                // Footstep on contact frames (0 and mid-cycle)
                TryFootstep(frames.Length);
            }
        }

        if (loop) frameIndex %= frames.Length;
        else frameIndex = Mathf.Min(frameIndex, frames.Length - 1);

        ApplyFrame(frames, frameIndex, fallbackIdle);
        UpdateCyclePhase(frames.Length > 0 ? frameIndex / (float)frames.Length : 0f);
    }

    private void TryFootstep(int frameCount)
    {
        if (player == null || !player.IsGrounded) return;
        // Contact frames: 0 and halfway through cycle
        int half = Mathf.Max(1, frameCount / 2);
        int contact = frameIndex % half == 0 ? frameIndex : -1;
        if (contact < 0 || contact == lastFootstepFrame) return;
        lastFootstepFrame = contact;
        float radius = state == AnimState.Run ? 2.8f : 2.2f;
        player.EmitFootstepNoise(radius);
        OnAnimEvent?.Invoke(AnimEventKind.FootPlant);
    }

    private void UpdateCyclePhase(float phase)
    {
        if (rig != null) rig.CyclePhase = phase;
    }

    private void ApplyFrame(Sprite[] frames, int index, Sprite fallback)
    {
        Sprite spr = null;
        if (frames != null && frames.Length > 0)
            spr = frames[Mathf.Clamp(index, 0, frames.Length - 1)];
        if (spr == null) spr = fallback;
        if (spr == null) return;
        if (rig != null) rig.SetBodySprite(spr);
        else
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.sprite = spr;
        }
    }

    private void ApplySquashAndRig(float dt, float absVx, Vector2 vel, bool grounded)
    {
        squash = Mathf.Lerp(squash, 0f, dt * squashRecover);

        float airStretch = 0f;
        if (!grounded)
            airStretch = Mathf.Clamp(vel.y * 0.02f, -0.06f, 0.1f);

        float energy = grounded ? Mathf.Clamp01(absVx / Mathf.Max(0.1f, player.moveSpeed)) : 0.35f;
        if (state == AnimState.Run) energy = Mathf.Max(energy, 0.85f);
        if (state == AnimState.GhostLoop) energy *= 0.9f;
        if (state == AnimState.WallSlide) energy = 0.4f;

        if (rig != null)
        {
            rig.SetSquash(squash);
            rig.AirStretch = airStretch;
            rig.MoveEnergy = energy;
            if (state != AnimState.ScareFreeze && state != AnimState.Hide)
                rig.SetSecondaryAmplitude(state == AnimState.Idle ? 1f : 0.75f);
            if (expressionTimer > 0f)
                rig.EmotionIntensity = Mathf.Lerp(rig.EmotionIntensity, 1f, dt * 6f);
            else
                rig.EmotionIntensity = Mathf.Lerp(rig.EmotionIntensity, 0f, dt * 4f);
        }
    }

    private void ApplyExpression()
    {
        if (expressionTimer <= 0f)
        {
            if (state != AnimState.Mindscape)
                rig?.SetExpressionOverlay(null, 0f);
            return;
        }

        expressionTimer -= Time.deltaTime;
        if (expressFrames.Length == 0 || rig == null) return;

        int idx = expressionState switch
        {
            AnimState.Happy => 0,
            AnimState.ScareFlinch => 2,
            AnimState.Hit => 3,
            AnimState.Injured => 4,
            _ => 7
        };
        idx = Mathf.Clamp(idx, 0, expressFrames.Length - 1);
        float a = Mathf.Clamp01(expressionTimer * 2f) * 0.55f;
        rig.SetExpressionOverlay(expressFrames[idx], a);
    }
}
