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
    private float walkTimer;
    private bool useWalkA = true;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        LoadSprites();
    }

    private void LoadSprites()
    {
        idleSprite = LoadFrame("player_idle") ?? spriteRenderer.sprite;
        walkASprite = LoadFrame("player_walk_a") ?? idleSprite;
        walkBSprite = LoadFrame("player_walk_b") ?? walkASprite;
        jumpSprite = LoadFrame("player_jump") ?? idleSprite;
        spriteRenderer.sprite = idleSprite;
    }

    private static Sprite LoadFrame(string name) =>
        Resources.Load<Sprite>($"Art/Characters/{name}");

    private void Update()
    {
        if (spriteRenderer == null || rb == null) return;

        var vx = Mathf.Abs(rb.linearVelocity.x);
        var vy = rb.linearVelocity.y;

        if (!IsGrounded())
        {
            spriteRenderer.sprite = jumpSprite;
            return;
        }

        if (vx < 0.05f)
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
        var hits = Physics2D.Raycast(transform.position, Vector2.down, 1.1f);
        return hits.collider != null && !hits.collider.isTrigger;
    }
}