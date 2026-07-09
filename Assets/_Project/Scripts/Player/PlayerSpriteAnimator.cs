using UnityEngine;

/// <summary>
/// Cycles authored player sprites for idle, walk, and jump states.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerSpriteAnimator : MonoBehaviour
{
    [SerializeField] private float walkFrameRate = 8f;

    private Sprite idleSprite;
    private Sprite walkASprite;
    private Sprite walkBSprite;
    private Sprite jumpSprite;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private PlayerController playerController;
    private float walkTimer;
    private bool useWalkA = true;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();
        LoadSprites();
    }

    private void LoadSprites()
    {
        idleSprite = LoadFrame("player_idle") ?? spriteRenderer.sprite;
        walkASprite = LoadFrame("player_walk_a") ?? idleSprite;
        walkBSprite = LoadFrame("player_walk_b") ?? walkASprite;
        jumpSprite = LoadFrame("player_jump") ?? idleSprite;
        spriteRenderer.sprite = idleSprite;
        spriteRenderer.color = Color.white;

        var scale = transform.localScale;
        if (scale.x < 1.2f || scale.y < 1.2f)
            transform.localScale = new Vector3(1.35f, 1.35f, 1f);
    }

    private static Sprite LoadFrame(string name) =>
        Resources.Load<Sprite>($"Art/Characters/{name}");

    private void Update()
    {
        if (spriteRenderer == null || rb == null) return;

        var vx = rb.linearVelocity.x;
        var absVx = Mathf.Abs(vx);

        if (absVx > 0.08f)
            spriteRenderer.flipX = vx < 0f;

        if (!IsGrounded())
        {
            spriteRenderer.sprite = jumpSprite;
            return;
        }

        if (absVx < 0.05f)
        {
            spriteRenderer.sprite = idleSprite;
            return;
        }

        walkTimer += Time.deltaTime * walkFrameRate;
        if (walkTimer >= 1f)
        {
            walkTimer = 0f;
            useWalkA = !useWalkA;
        }

        spriteRenderer.sprite = useWalkA ? walkASprite : walkBSprite;
    }

    private bool IsGrounded()
    {
        return playerController != null ? playerController.IsGrounded : true;
    }
}