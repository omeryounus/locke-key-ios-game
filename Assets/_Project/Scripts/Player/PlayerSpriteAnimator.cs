using UnityEngine;

/// <summary>
/// Idle breathe, hood-friendly squash, walk cycle, jump/land, +20% base brightness.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerSpriteAnimator : MonoBehaviour
{
    [SerializeField] private float walkFrameRate = 11f;
    [SerializeField] private float idleBreatheAmount = 0.035f;
    [SerializeField] private float idleBreatheSpeed = 1.85f;
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
    private Vector3 baseScale = new(1.55f, 1.55f, 1f);
    private float squash;
    private float hoodTilt;
    private Color baseColor = new(1.2f, 1.2f, 1.2f, 1f); // +20% brightness
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
        spriteRenderer.color = baseColor;

        var scale = transform.localScale;
        if (Mathf.Abs(scale.x) < 1.45f || Mathf.Abs(scale.y) < 1.45f)
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

        if (!grounded)
        {
            spriteRenderer.sprite = jumpSprite;
            squash = Mathf.Lerp(squash, vy > 0.2f ? 0.06f : -0.04f, Time.deltaTime * 8f);
            hoodTilt = Mathf.Lerp(hoodTilt, -vx * 0.8f, Time.deltaTime * 5f);
        }
        else if (playerController != null && playerController.JustLanded)
        {
            spriteRenderer.sprite = idleSprite;
            squash = -landSquash;
        }
        else if (absVx < 0.08f)
        {
            spriteRenderer.sprite = idleSprite;
            // Stronger idle breathing
            var breathe = Mathf.Sin(Time.time * idleBreatheSpeed) * idleBreatheAmount;
            var breathe2 = Mathf.Sin(Time.time * idleBreatheSpeed * 0.5f) * idleBreatheAmount * 0.35f;
            squash = Mathf.Lerp(squash, breathe + breathe2, Time.deltaTime * 5f);
            // Hood sway while idle
            hoodTilt = Mathf.Sin(Time.time * 1.6f) * 3.2f;
        }
        else
        {
            walkTimer += Time.deltaTime * walkFrameRate * Mathf.Clamp(absVx / 3.5f, 0.55f, 1.35f);
            if (walkTimer >= 1f)
            {
                walkTimer = 0f;
                useWalkA = !useWalkA;
                squash = -0.04f;
            }

            spriteRenderer.sprite = useWalkA ? walkASprite : walkBSprite;
            squash = Mathf.Lerp(squash, 0f, Time.deltaTime * squashRecoverSpeed);
            hoodTilt = Mathf.Lerp(hoodTilt, -Mathf.Sign(vx) * 2.5f, Time.deltaTime * 6f);
        }

        squash = Mathf.Lerp(squash, 0f, Time.deltaTime * squashRecoverSpeed * 0.25f);
        ApplyScale(squash);

        var targetColor = ghostVisual ? ghostTint : baseColor;
        spriteRenderer.color = Color.Lerp(spriteRenderer.color, targetColor, Time.deltaTime * 8f);
        if (ghostVisual)
        {
            var flicker = 0.5f + Mathf.Sin(Time.time * 9f) * 0.12f;
            var c = spriteRenderer.color;
            spriteRenderer.color = new Color(c.r, c.g, c.b, flicker);
        }

        // Drive hood visual if present
        var hood = transform.Find("HoodSway");
        if (hood != null)
            hood.localRotation = Quaternion.Euler(0f, 0f, hoodTilt);
    }

    private void ApplyScale(float squashAmount)
    {
        var y = baseScale.y * (1f + squashAmount);
        var x = baseScale.x * (1f - squashAmount * 0.7f);
        transform.localScale = new Vector3(Mathf.Abs(x), Mathf.Abs(y), 1f);
    }

    private bool IsGrounded() =>
        playerController != null ? playerController.IsGrounded : true;
}
