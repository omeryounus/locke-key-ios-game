using System.Collections;
using UnityEngine;

/// <summary>
/// Beat 3 — three-push bookshelf that reveals the Ghost Key alcove.
/// Smooth push animation, debris, audio, camera, and haptics.
/// </summary>
public class CollapsedBookshelfPuzzle : PuzzleBase
{
    [SerializeField] private Collider2D blockingCollider;
    [SerializeField] private Transform shelfTransform;
    [SerializeField] private SpriteRenderer shelfRenderer;
    [SerializeField] private Transform debrisParent;
    [SerializeField] private float pushDistance = 0.85f;
    [SerializeField] private int pushesRequired = 3;
    [SerializeField] private float pushDuration = 0.28f;

    private int pushesDone;
    private bool animating;
    private Vector3 shelfBasePos;

    public override string InteractionHint =>
        isSolved
            ? string.Empty
            : pushesDone == 0
                ? "Collapsed bookshelf — tap Interact to push"
                : $"Bookshelf — push again ({pushesRequired - pushesDone} left)";

    public override bool CanInteract => !isSolved && !animating;

    protected override void Awake()
    {
        base.Awake();
        puzzleID = "chapter1_bookshelf";
        requiresSpecificKey = false;

        if (shelfTransform == null)
            shelfTransform = transform;
        shelfBasePos = shelfTransform.position;

        if (debrisParent == null)
        {
            var debrisGo = new GameObject("BookshelfDebris");
            debrisGo.transform.SetParent(transform);
            debrisParent = debrisGo.transform;
        }
    }

    protected override void TrySolve()
    {
        if (animating || isSolved) return;
        StartCoroutine(PushRoutine());
    }

    private IEnumerator PushRoutine()
    {
        animating = true;
        pushesDone++;

        FindFirstObjectByType<GameAudioController>()?.PlayDoorRattle();
        GameHaptics.TriggerHapticLight();
        FindFirstObjectByType<CameraFollow2D>()?.Pulse(0.12f, 0.18f);

        var start = shelfTransform.position;
        var end = start + Vector3.right * pushDistance;
        var elapsed = 0f;

        while (elapsed < pushDuration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.SmoothStep(0f, 1f, elapsed / pushDuration);
            // Slight overshoot for weight
            var overshoot = Mathf.Sin(t * Mathf.PI) * 0.04f;
            shelfTransform.position = Vector3.Lerp(start, end, t) + Vector3.right * overshoot;
            if (shelfRenderer != null)
            {
                var shake = Mathf.Sin(elapsed * 55f) * 0.015f * (1f - t);
                shelfTransform.position += Vector3.up * shake;
            }

            yield return null;
        }

        shelfTransform.position = end;
        SpawnDebrisBurst();

        var remaining = pushesRequired - pushesDone;
        var hud = FindFirstObjectByType<GameplayHUD>();
        if (remaining > 0)
        {
            hud?.ShowToast(remaining == 1
                ? "Almost free — one more push."
                : $"Dust and books tumble free — {remaining} pushes left.", 2.4f);
            animating = false;
            yield break;
        }

        if (blockingCollider != null)
            blockingCollider.enabled = false;

        if (shelfRenderer != null)
        {
            var startColor = shelfRenderer.color;
            var endColor = new Color(0.35f, 0.28f, 0.2f, 0.55f);
            elapsed = 0f;
            while (elapsed < 0.35f)
            {
                elapsed += Time.deltaTime;
                shelfRenderer.color = Color.Lerp(startColor, endColor, elapsed / 0.35f);
                yield return null;
            }
        }

        FindFirstObjectByType<CameraFollow2D>()?.Pulse(0.2f, 0.28f);
        GameHaptics.Unlock();
        hud?.ShowToast("A hidden alcove opens — something glimmers inside.", 3.5f);
        MarkAsSolved();
        animating = false;
    }

    public override void RestoreSolvedState()
    {
        base.RestoreSolvedState();
        pushesDone = pushesRequired;
        if (blockingCollider != null)
            blockingCollider.enabled = false;
        if (shelfTransform != null)
            shelfTransform.position = shelfBasePos + Vector3.right * (pushDistance * pushesRequired);
        if (shelfRenderer != null)
            shelfRenderer.color = new Color(0.35f, 0.28f, 0.2f, 0.55f);
    }

    private void SpawnDebrisBurst()
    {
        for (var i = 0; i < 5; i++)
        {
            var bit = new GameObject($"Debris_{pushesDone}_{i}", typeof(SpriteRenderer));
            bit.transform.SetParent(debrisParent);
            bit.transform.position = shelfTransform.position + new Vector3(
                Random.Range(-0.25f, 0.7f), Random.Range(-0.35f, 0.35f), 0f);
            bit.transform.localScale = Vector3.one * Random.Range(0.07f, 0.15f);
            var sr = bit.GetComponent<SpriteRenderer>();
            sr.color = new Color(
                Random.Range(0.35f, 0.55f),
                Random.Range(0.24f, 0.36f),
                Random.Range(0.14f, 0.22f),
                0.85f);
            sr.sortingOrder = 4;
            bit.AddComponent<DebrisDrift>().Init(
                new Vector2(Random.Range(0.4f, 1.4f), Random.Range(0.6f, 1.8f)),
                Random.Range(1.6f, 2.6f));
        }
    }

    private class DebrisDrift : MonoBehaviour
    {
        private Vector2 velocity;
        private float life;
        private float age;
        private SpriteRenderer sr;

        public void Init(Vector2 vel, float lifetime)
        {
            velocity = vel;
            life = lifetime;
            sr = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            age += Time.deltaTime;
            velocity.y -= 4.5f * Time.deltaTime;
            transform.position += (Vector3)(velocity * Time.deltaTime);
            transform.Rotate(0f, 0f, velocity.x * 40f * Time.deltaTime);
            if (sr != null)
            {
                var c = sr.color;
                c.a = Mathf.Lerp(0.85f, 0f, age / life);
                sr.color = c;
            }

            if (age >= life)
                Destroy(gameObject);
        }
    }
}
