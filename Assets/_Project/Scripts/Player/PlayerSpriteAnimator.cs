using UnityEngine;

/// <summary>
/// Production character presentation: idle breathe, walk cycle, jump/land squash,
/// facing, and Ghost Key ethereal tint.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerSpriteAnimator : MonoBehaviour
{
    [SerializeField] private float walkFrameRate = 11f;
    [SerializeField] private float idleBreatheAmount = 0.018f;
    [SerializeField] private float idleBreatheSpeed = 2.2f;
    [SerializeField] private float landSquash = 0.12f;
    [SerializeField] private float squashRecoverSpeed = 10f;

    private Sprite idleSprite;
    private Sprite walkASprite;
    private Sprite walkBSprite;
    private Sprite jumpSprite;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private PlayerController playerController;
    private float walkTimer;
    private bool useWalkA = true;
    private Vector3 baseScale = new(1.35f, 1.35f, 1f);
    private float squash;
    private Color baseColor = Color.white;
    private Color ghostTint = new(0.62f, 0.88f, 1f, 0.55f);
    private EventBus eventBus;
    private bool ghostVisual;

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
        spriteRenderer.color = Color.white;
        baseColor = Color.white;

        var scale = transform.localScale;
        if (Mathf.Abs(scale.x) < 1.2f || Mathf.Abs(scale.y) < 1.2f)
            transform.localScale = baseScale;
        else
            baseScale = new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), 1f);
    }

    private static Sprite LoadFrame(string name) =>
        Resources.Load<Sprite>($"Art/Characters/{name}");

    private void OnGhostStart() => ghostVisual = true;
    private void OnGhostEnd() => ghostVisual = false;

    private void Update()
    {
        if (spriteRenderer == null || rb == null) return;

        var vx = rb.linearVelocity.x;
        var vy = rb.linearVelocity.y;
        var absVx = Mathf.Abs(vx);
        var grounded = IsGrounded();

        if (absVx > 0.08f)
            spriteRenderer.flipX = vx < 0f;

        // Frame selection
        if (!grounded)
        {
            spriteRenderer.sprite = jumpSprite;
            // Air stretch when rising, slight squash falling.
            var air = vy > 0.2f ? 0.06f : -0.04f;
            squash = Mathf.Lerp(squash, air, Time.deltaTime * 8f);
        }
        else if (playerController != null && playerController.JustLanded)
        {
            spriteRenderer.sprite = idleSprite;
            squash = -landSquash;
        }
        else if (absVx < 0.08f)
        {
            spriteRenderer.sprite = idleSprite;
            // Idle breathe
            var breathe = Mathf.Sin(Time.time * idleBreatheSpeed) * idleBreatheAmount;
            squash = Mathf.Lerp(squash, breathe, Time.deltaTime * 6f);
        }
        else
        {
            walkTimer += Time.deltaTime * walkFrameRate * Mathf.Clamp(absVx / 3.5f, 0.55f, 1.35f);
            if (walkTimer >= 1f)
            {
                walkTimer = 0f;
                useWalkA = !useWalkA;
                // Subtle walk bob per step
                squash = -0.04f;
            }

            spriteRenderer.sprite = useWalkA ? walkASprite : walkBSprite;
            squash = Mathf.Lerp(squash, 0f, Time.deltaTime * squashRecoverSpeed);
        }

        squash = Mathf.Lerp(squash, 0f, Time.deltaTime * squashRecoverSpeed * 0.35f);
        ApplyScale(squash);

        // Ghost ethereal blend
        var targetColor = ghostVisual ? ghostTint : baseColor;
        spriteRenderer.color = Color.Lerp(spriteRenderer.color, targetColor, Time.deltaTime * 8f);
        if (ghostVisual)
        {
            var flicker = 0.5f + Mathf.Sin(Time.time * 9f) * 0.12f;
            var c = spriteRenderer.color;
            spriteRenderer.color = new Color(c.r, c.g, c.b, flicker);
        }
    }

    private void ApplyScale(float squashAmount)
    {
        // squashAmount negative = shorter/wider (land), positive = taller/thinner (jump)
        var y = baseScale.y * (1f + squashAmount);
        var x = baseScale.x * (1f - squashAmount * 0.7f);
        var facing = spriteRenderer.flipX ? -1f : 1f;
        // Keep positive base; flipX handles facing on sprite, not scale.
        transform.localScale = new Vector3(Mathf.Abs(x), Mathf.Abs(y), 1f);
        _ = facing;
    }

    private bool IsGrounded()
    {
        return playerController != null ? playerController.IsGrounded : true;
    }
}
