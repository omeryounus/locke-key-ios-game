using UnityEngine;

/// <summary>
/// Expanded presentation state machine: idle / walk / run / jump / land / interact pose
/// with smooth visual blending (scale/breathe) — sprite set limited so we interpolate feel.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerSpriteAnimator : MonoBehaviour
{
    public enum AnimState { Idle, Walk, Run, Jump, Fall, Land, Interact }

    [SerializeField] private float walkFrameRate = 11f;
    [SerializeField] private float runFrameRate = 14f;
    [SerializeField] private float idleBreatheAmount = 0.038f;
    [SerializeField] private float idleBreatheSpeed = 1.85f;
    [SerializeField] private float landSquash = 0.14f;
    [SerializeField] private float squashRecoverSpeed = 10f;

    private Sprite idleSprite, walkASprite, walkBSprite, jumpSprite;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private PlayerController playerController;
    private float walkTimer;
    private bool useWalkA = true;
    private Vector3 baseScale = new(1.55f, 1.55f, 1f);
    private float squash;
    private float hoodTilt;
    private Color baseColor = new(1.2f, 1.2f, 1.2f, 1f);
    private Color ghostTint = new(0.35f, 0.95f, 0.65f, 0.55f);
    private EventBus eventBus;
    private bool ghostVisual;
    private AnimState state = AnimState.Idle;
    private float landHold;
    private float interactPoseTimer;

    public AnimState State => state;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();
        eventBus = Resources.Load<EventBus>("EventBus");
        LoadSprites();
        if (eventBus != null)
        {
            eventBus.OnGhostPhaseStarted += OnGhostStart;
            eventBus.OnGhostPhaseEnded += OnGhostEnd;
        }
    }

    private void OnDestroy()
    {
        if (eventBus == null) return;
        eventBus.OnGhostPhaseStarted -= OnGhostStart;
        eventBus.OnGhostPhaseEnded -= OnGhostEnd;
    }

    private void LoadSprites()
    {
        idleSprite = LoadFrame("player_idle") ?? spriteRenderer.sprite;
        walkASprite = LoadFrame("player_walk_a") ?? idleSprite;
        walkBSprite = LoadFrame("player_walk_b") ?? walkASprite;
        jumpSprite = LoadFrame("player_jump") ?? idleSprite;
        spriteRenderer.sprite = idleSprite;
        spriteRenderer.color = baseColor;
        var scale = transform.localScale;
        if (Mathf.Abs(scale.x) < 1.45f || Mathf.Abs(scale.y) < 1.45f)
            transform.localScale = baseScale;
        else
            baseScale = new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), 1f);
    }

    private static Sprite LoadFrame(string name) => Resources.Load<Sprite>($"Art/Characters/{name}");
    private void OnGhostStart() => ghostVisual = true;
    private void OnGhostEnd() => ghostVisual = false;

    public void PlayInteractPose(float duration = 0.35f)
    {
        interactPoseTimer = duration;
        state = AnimState.Interact;
    }

    private void Update()
    {
        if (spriteRenderer == null || rb == null) return;

        var vx = rb.linearVelocity.x;
        var vy = rb.linearVelocity.y;
        var absVx = Mathf.Abs(vx);
        var grounded = playerController != null ? playerController.IsGrounded : true;

        if (absVx > 0.08f)
            spriteRenderer.flipX = vx < 0f;

        if (interactPoseTimer > 0f)
        {
            interactPoseTimer -= Time.deltaTime;
            spriteRenderer.sprite = idleSprite;
            squash = Mathf.Lerp(squash, 0.04f, Time.deltaTime * 8f);
            state = AnimState.Interact;
        }
        else if (!grounded)
        {
            spriteRenderer.sprite = jumpSprite;
            if (vy > 0.15f) state = AnimState.Jump;
            else state = AnimState.Fall;
            squash = Mathf.Lerp(squash, vy > 0.2f ? 0.07f : -0.05f, Time.deltaTime * 8f);
            hoodTilt = Mathf.Lerp(hoodTilt, -vx * 0.9f, Time.deltaTime * 5f);
        }
        else if (playerController != null && playerController.JustLanded)
        {
            state = AnimState.Land;
            landHold = 0.12f;
            spriteRenderer.sprite = idleSprite;
            squash = -landSquash;
        }
        else if (landHold > 0f)
        {
            landHold -= Time.deltaTime;
            spriteRenderer.sprite = idleSprite;
            state = AnimState.Land;
            squash = Mathf.Lerp(squash, 0f, Time.deltaTime * 12f);
        }
        else if (absVx < 0.1f)
        {
            state = AnimState.Idle;
            spriteRenderer.sprite = idleSprite;
            var breathe = Mathf.Sin(Time.time * idleBreatheSpeed) * idleBreatheAmount;
            var breathe2 = Mathf.Sin(Time.time * idleBreatheSpeed * 0.5f) * idleBreatheAmount * 0.4f;
            squash = Mathf.Lerp(squash, breathe + breathe2, Time.deltaTime * 5f);
            hoodTilt = Mathf.Sin(Time.time * 1.6f) * 3.5f;
        }
        else
        {
            bool run = absVx > moveSpeedThreshold();
            state = run ? AnimState.Run : AnimState.Walk;
            float rate = run ? runFrameRate : walkFrameRate;
            walkTimer += Time.deltaTime * rate * Mathf.Clamp(absVx / 3.5f, 0.55f, 1.4f);
            if (walkTimer >= 1f)
            {
                walkTimer = 0f;
                useWalkA = !useWalkA;
                squash = run ? -0.05f : -0.035f;
            }
            spriteRenderer.sprite = useWalkA ? walkASprite : walkBSprite;
            squash = Mathf.Lerp(squash, 0f, Time.deltaTime * squashRecoverSpeed);
            hoodTilt = Mathf.Lerp(hoodTilt, -Mathf.Sign(vx) * (run ? 4f : 2.5f), Time.deltaTime * 6f);
        }

        squash = Mathf.Lerp(squash, 0f, Time.deltaTime * squashRecoverSpeed * 0.2f);
        ApplyScale(squash);

        var targetColor = ghostVisual ? ghostTint : baseColor;
        spriteRenderer.color = Color.Lerp(spriteRenderer.color, targetColor, Time.deltaTime * 8f);
        if (ghostVisual)
        {
            var flicker = 0.5f + Mathf.Sin(Time.time * 9f) * 0.12f;
            var c = spriteRenderer.color;
            spriteRenderer.color = new Color(c.r, c.g, c.b, flicker);
        }

        var hood = transform.Find("HoodSway");
        if (hood != null)
            hood.localRotation = Quaternion.Euler(0f, 0f, hoodTilt);
    }

    private float moveSpeedThreshold()
    {
        return playerController != null ? playerController.moveSpeed * 0.72f : 4f;
    }

    private void ApplyScale(float squashAmount)
    {
        var y = baseScale.y * (1f + squashAmount);
        var x = baseScale.x * (1f - squashAmount * 0.7f);
        transform.localScale = new Vector3(Mathf.Abs(x), Mathf.Abs(y), 1f);
    }
}
