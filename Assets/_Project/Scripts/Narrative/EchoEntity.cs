using UnityEngine;

/// <summary>
/// Echo threat with line-of-sight, hide-spot awareness, and catch/fail handling.
/// </summary>
public class EchoEntity : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1.15f;
    [SerializeField] private float hiddenSpeedMultiplier = 0.2f;
    [SerializeField] private float blindSpeedMultiplier = 0.35f;
    [SerializeField] private float lifetime = 14f;
    [SerializeField] private float reachDistance = 0.75f;
    [SerializeField] private LayerMask lineOfSightMask = ~0;

    private Transform player;
    private SpriteRenderer spriteRenderer;
    private EventBus eventBus;
    private ChapterBeatDirector beatDirector;
    private float lifeTimer;
    private bool active;
    private bool hasCaughtPlayer;

    public bool HasLineOfSight { get; private set; }

    public void Initialize(Transform playerTransform)
    {
        player = playerTransform;
        spriteRenderer = GetComponent<SpriteRenderer>();
        eventBus = Resources.Load<EventBus>("EventBus");
        beatDirector = FindFirstObjectByType<ChapterBeatDirector>();
        lifeTimer = lifetime;
        active = true;
        hasCaughtPlayer = false;
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

        HasLineOfSight = EvaluateLineOfSight();
        var speed = moveSpeed;
        if (HideSpot.IsPlayerHidden)
            speed *= hiddenSpeedMultiplier;
        else if (!HasLineOfSight)
            speed *= blindSpeedMultiplier;

        if (HasLineOfSight || !HideSpot.IsPlayerHidden)
        {
            var dir = (player.position - transform.position).normalized;
            transform.position += dir * (speed * Time.deltaTime);
        }

        if (spriteRenderer != null)
        {
            var alpha = HasLineOfSight
                ? 0.35f + Mathf.PingPong(Time.time * 1.5f, 0.25f)
                : 0.18f;
            spriteRenderer.color = new Color(0.55f, 0.1f, 0.18f, alpha);
        }

        if (!hasCaughtPlayer
            && HasLineOfSight
            && !HideSpot.IsPlayerHidden
            && Vector2.Distance(transform.position, player.position) <= reachDistance)
        {
            CatchPlayer();
        }
    }

    private bool EvaluateLineOfSight()
    {
        if (player == null || HideSpot.IsPlayerHidden)
            return false;

        var origin = (Vector2)transform.position;
        var target = (Vector2)player.position;
        var delta = target - origin;
        var distance = delta.magnitude;
        if (distance <= 0.05f)
            return true;

        var hits = Physics2D.RaycastAll(origin, delta.normalized, distance, lineOfSightMask);
        foreach (var hit in hits)
        {
            if (hit.collider == null || hit.collider.isTrigger) continue;
            if (hit.collider.GetComponent<PlayerController>() != null)
                return true;
            if (hit.collider.GetComponent<HideSpot>() != null)
                return false;
            return false;
        }

        return true;
    }

    private void CatchPlayer()
    {
        hasCaughtPlayer = true;
        eventBus?.EchoCaught();
        beatDirector?.NotifyEchoCaught();
        Despawn();
    }

    private void Despawn()
    {
        active = false;
        Destroy(gameObject);
    }
}