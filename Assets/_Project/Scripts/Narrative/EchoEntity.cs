using UnityEngine;

/// <summary>
/// Echo threat with line-of-sight, hide-spot awareness, and catch/fail handling.
/// </summary>
public class EchoEntity : MonoBehaviour
{
    public enum AIState
    {
        Patrol,
        Investigate,
        Chase,
        Stunned
    }

    [Header("AI State")]
    public AIState currentState = AIState.Patrol;

    [Header("Sensory Settings")]
    [SerializeField] private float moveSpeed = 1.15f;
    [SerializeField] private float hiddenSpeedMultiplier = 0.2f;
    [SerializeField] private float blindSpeedMultiplier = 0.35f;
    [SerializeField] private float lifetime = 18f;
    [SerializeField] private float reachDistance = 0.75f;
    [SerializeField] private LayerMask lineOfSightMask = ~0;

    [Header("Patrol Settings")]
    [SerializeField] private float patrolRadius = 4f;
    private Vector3 spawnPosition;
    private Vector3 patrolTarget;
    private float patrolWaitTimer;

    [Header("Investigation Settings")]
    [SerializeField] private float investigateDuration = 4f;
    private Vector3 investigateTarget;
    private float investigateTimer;

    [Header("Stun Settings")]
    private float stunTimer;

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

        spawnPosition = transform.position;
        patrolTarget = GetRandomPatrolPoint();
        currentState = AIState.Patrol;

        if (eventBus != null)
            eventBus.OnNoiseHeard += HandleNoiseHeard;
    }

    private void OnDestroy()
    {
        if (eventBus != null)
            eventBus.OnNoiseHeard -= HandleNoiseHeard;
    }

    private void Update()
    {
        if (!active) return;

        // Materialize with ease-out scale, slight chase swell
        var targetScale = currentState == AIState.Chase ? 1.05f : 0.92f;
        transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * targetScale, Time.deltaTime * 3.5f);
        lifeTimer -= Time.deltaTime;

        if (player == null || lifeTimer <= 0f)
        {
            // Lifetime end counts as escape opportunity
            if (!hasCaughtPlayer && beatDirector != null &&
                beatDirector.CurrentBeat == ChapterBeatDirector.Beat.EchoEncounter)
                beatDirector.NotifyEchoEscaped();
            Despawn();
            return;
        }

        HasLineOfSight = EvaluateLineOfSight();

        if (HasLineOfSight && !HideSpot.IsPlayerHidden && currentState != AIState.Stunned)
        {
            if (currentState != AIState.Chase)
            {
                currentState = AIState.Chase;
                eventBus?.SetTension(0.9f);
            }
        }
        else if (currentState == AIState.Chase)
        {
            currentState = AIState.Investigate;
            investigateTarget = player.position;
            investigateTimer = investigateDuration;
            eventBus?.SetTension(0.6f);
        }

        switch (currentState)
        {
            case AIState.Patrol:
                UpdatePatrol();
                break;
            case AIState.Investigate:
                UpdateInvestigate();
                break;
            case AIState.Chase:
                UpdateChase();
                break;
            case AIState.Stunned:
                UpdateStunned();
                break;
        }

        if (spriteRenderer != null)
        {
            float alpha = 0.22f;
            Color tint = new(0.55f, 0.12f, 0.2f, 1f);
            if (currentState == AIState.Chase)
            {
                alpha = 0.42f + Mathf.PingPong(Time.time * 3.2f, 0.35f);
                tint = new Color(0.75f, 0.08f, 0.14f, 1f);
            }
            else if (currentState == AIState.Investigate)
            {
                alpha = 0.3f + Mathf.PingPong(Time.time * 1.1f, 0.18f);
                tint = new Color(0.6f, 0.15f, 0.28f, 1f);
            }
            else if (currentState == AIState.Stunned)
                alpha = 0.14f;

            spriteRenderer.color = new Color(tint.r, tint.g, tint.b, alpha);
            if (player != null)
                spriteRenderer.flipX = player.position.x < transform.position.x;
        }

        if (!hasCaughtPlayer
            && currentState == AIState.Chase
            && !HideSpot.IsPlayerHidden
            && Vector2.Distance(transform.position, player.position) <= reachDistance)
        {
            var playerController = player.GetComponent<PlayerController>();
            if (playerController != null && playerController.IsGhostPhasing)
            {
                var ghostAbility = player.GetComponent<GhostKeyAbility>();
                if (ghostAbility != null && !ghostAbility.IsBodyCaptured)
                {
                    ghostAbility.TriggerBodyCapture();
                    FindFirstObjectByType<GameplayHUD>()?.ShowToast("The Echo has seized your physical body! Return immediately!", 4f);
                    Stun(3.5f);
                }
            }
            else
            {
                CatchPlayer();
            }
        }
    }

    private void UpdatePatrol()
    {
        var step = moveSpeed * 0.6f * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, patrolTarget, step);

        if (Vector3.Distance(transform.position, patrolTarget) < 0.1f)
        {
            patrolWaitTimer += Time.deltaTime;
            if (patrolWaitTimer >= 1.5f)
            {
                patrolWaitTimer = 0f;
                patrolTarget = GetRandomPatrolPoint();
            }
        }
    }

    private void UpdateInvestigate()
    {
        var step = moveSpeed * 0.8f * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, investigateTarget, step);

        investigateTimer -= Time.deltaTime;
        if (investigateTimer <= 0f || Vector3.Distance(transform.position, investigateTarget) < 0.1f)
        {
            currentState = AIState.Patrol;
            patrolTarget = GetRandomPatrolPoint();
            eventBus?.SetTension(0.2f);
        }
    }

    private void UpdateChase()
    {
        var speed = moveSpeed;
        if (HideSpot.IsPlayerHidden)
            speed *= hiddenSpeedMultiplier;

        var dir = (player.position - transform.position).normalized;
        transform.position += dir * (speed * Time.deltaTime);
    }

    private void UpdateStunned()
    {
        stunTimer -= Time.deltaTime;
        if (stunTimer <= 0f)
        {
            currentState = AIState.Patrol;
            patrolTarget = GetRandomPatrolPoint();
        }
    }

    public void Stun(float duration)
    {
        currentState = AIState.Stunned;
        stunTimer = duration;
        eventBus?.SetTension(0.3f);
    }

    private void HandleNoiseHeard(Vector2 noisePos, float radius)
    {
        if (!active || currentState == AIState.Chase || currentState == AIState.Stunned)
            return;

        float dist = Vector2.Distance(transform.position, noisePos);
        if (dist <= radius)
        {
            currentState = AIState.Investigate;
            investigateTarget = noisePos;
            investigateTimer = investigateDuration;
            Debug.Log($"Echo heard noise at {noisePos}, moving to investigate.");
        }
    }

    private Vector3 GetRandomPatrolPoint()
    {
        float offset = Random.Range(-patrolRadius, patrolRadius);
        return new Vector3(spawnPosition.x + offset, spawnPosition.y, spawnPosition.z);
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