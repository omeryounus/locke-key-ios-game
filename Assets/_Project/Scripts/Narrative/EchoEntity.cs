using UnityEngine;

/// <summary>
/// Simple Echo threat that drifts toward the player until dispelled or timed out.
/// </summary>
public class EchoEntity : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1.6f;
    [SerializeField] private float lifetime = 9f;
    [SerializeField] private float reachDistance = 0.75f;

    private Transform player;
    private SpriteRenderer spriteRenderer;
    private float lifeTimer;
    private bool active;

    public void Initialize(Transform playerTransform)
    {
        player = playerTransform;
        spriteRenderer = GetComponent<SpriteRenderer>();
        lifeTimer = lifetime;
        active = true;
        transform.localScale = Vector3.zero;
    }

    private void Update()
    {
        if (!active) return;

        transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * 0.9f, Time.deltaTime * 3f);
        lifeTimer -= Time.deltaTime;

        if (player == null || lifeTimer <= 0f)
        {
            Despawn();
            return;
        }

        var playerController = player.GetComponent<PlayerController>();
        if (playerController != null && playerController.IsGhostPhasing)
        {
            Despawn();
            return;
        }

        var dir = (player.position - transform.position).normalized;
        transform.position += dir * (moveSpeed * Time.deltaTime);

        if (spriteRenderer != null)
        {
            var alpha = 0.35f + Mathf.PingPong(Time.time * 1.5f, 0.25f);
            spriteRenderer.color = new Color(0.55f, 0.1f, 0.18f, alpha);
        }

        if (Vector2.Distance(transform.position, player.position) <= reachDistance)
        {
            Debug.Log("The Echo brushes past you — cold, hungry, gone.");
            Despawn();
        }
    }

    private void Despawn()
    {
        active = false;
        Destroy(gameObject);
    }
}