using System.Collections;
using UnityEngine;

/// <summary>
/// Library multi-step puzzle:
/// 1) Inspect wreckage (clue)
/// 2) Brace and push the lower beam (Interact again)
/// 3) Clear path — Ghost Key alcove opens
/// </summary>
public class CollapsedBookshelfPuzzle : PuzzleBase
{
    public enum Stage { Inspect, Brace, Cleared }

    [SerializeField] private Collider2D blockingCollider;
    [SerializeField] private Transform shelfTransform;
    [SerializeField] private SpriteRenderer shelfRenderer;
    [SerializeField] private Transform debrisParent;
    [SerializeField] private float clearDuration = 1.15f;

    private Stage stage = Stage.Inspect;
    private bool animating;
    private Vector3 shelfBasePos;
    private int pushCount;

    public Stage CurrentStage => stage;
    public override bool CanInteract => !isSolved && !animating;

    public override string InteractionHint
    {
        get
        {
            if (isSolved) return string.Empty;
            return stage switch
            {
                Stage.Inspect => "Collapsed bookshelf — tap Interact to inspect",
                Stage.Brace => "Loose beam — tap Interact twice to shove it free",
                _ => string.Empty
            };
        }
    }

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

    public override void Interact()
    {
        if (isSolved || animating) return;

        switch (stage)
        {
            case Stage.Inspect:
                StartCoroutine(InspectRoutine());
                break;
            case Stage.Brace:
                pushCount++;
                if (pushCount == 1)
                {
                    StartCoroutine(BracePulse());
                    FindFirstObjectByType<GameplayHUD>()?.ShowToast(
                        "It budges — shove again with Interact!", 2.6f);
                }
                else
                    StartCoroutine(ClearWreckageRoutine());
                break;
        }
    }

    protected override void TrySolve()
    {
        // Multi-step uses Interact override
    }

    private IEnumerator InspectRoutine()
    {
        animating = true;
        var hud = FindFirstObjectByType<GameplayHUD>();
        hud?.ShowToast("Heavy wreckage… a gap under the lower beam. Brace and push.", 3.2f);
        FindFirstObjectByType<GameAudioController>()?.PlayDoorRattle();
        GameHaptics.TriggerHapticLight();

        // Small dust shake on inspect
        float e = 0f;
        var start = shelfTransform.position;
        while (e < 0.35f)
        {
            e += Time.deltaTime;
            shelfTransform.position = start + Vector3.right * (Mathf.Sin(e * 40f) * 0.02f);
            yield return null;
        }
        shelfTransform.position = start;
        stage = Stage.Brace;
        pushCount = 0;
        animating = false;
        hud?.ShowToast("Interact again to shove the beam.", 2.5f);
    }

    private IEnumerator BracePulse()
    {
        animating = true;
        FindFirstObjectByType<GameAudioController>()?.PlayDoorRattle();
        GameHaptics.TriggerHapticLight();
        float e = 0f;
        var start = shelfTransform.position;
        while (e < 0.4f)
        {
            e += Time.deltaTime;
            shelfTransform.position = start + Vector3.right * (Mathf.Sin(e * 55f) * 0.045f);
            yield return null;
        }
        shelfTransform.position = start;
        SpawnDebrisBurst(3);
        animating = false;
    }

    private IEnumerator ClearWreckageRoutine()
    {
        animating = true;
        var hud = FindFirstObjectByType<GameplayHUD>();
        hud?.ShowToast("You brace and shove the wreckage aside…", 2.2f);
        FindFirstObjectByType<GameAudioController>()?.PlayDoorRattle();
        GameHaptics.TriggerHapticLight();
        FindFirstObjectByType<CameraFollow2D>()?.Pulse(0.14f, 0.25f);

        var start = shelfTransform.position;
        var end = start + new Vector3(1.1f, -0.15f, 0f);
        var startRot = shelfTransform.eulerAngles.z;
        var elapsed = 0f;

        while (elapsed < 0.28f)
        {
            elapsed += Time.deltaTime;
            var shake = Mathf.Sin(elapsed * 60f) * 0.04f;
            shelfTransform.position = start + Vector3.right * shake;
            yield return null;
        }

        SpawnDebrisBurst(6);
        FindFirstObjectByType<GameAudioController>()?.PlayDoorRattle();

        elapsed = 0f;
        while (elapsed < clearDuration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.SmoothStep(0f, 1f, elapsed / clearDuration);
            shelfTransform.position = Vector3.Lerp(start, end, t);
            shelfTransform.rotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(startRot, startRot - 18f, t));

            if (shelfRenderer != null)
            {
                var c = shelfRenderer.color;
                shelfRenderer.color = new Color(c.r, c.g, c.b, Mathf.Lerp(1f, 0.55f, t));
            }

            if (elapsed > clearDuration * 0.4f && elapsed < clearDuration * 0.45f)
                SpawnDebrisBurst(4);

            yield return null;
        }

        if (blockingCollider != null)
            blockingCollider.enabled = false;

        stage = Stage.Cleared;
        GameHaptics.Unlock();
        FindFirstObjectByType<CameraFollow2D>()?.Pulse(0.2f, 0.28f);
        hud?.ShowToast("An alcove opens — a key glimmers inside. Claim the Ghost Key.", 3.5f);
        MarkAsSolved();
        animating = false;
    }

    public override void RestoreSolvedState()
    {
        base.RestoreSolvedState();
        stage = Stage.Cleared;
        if (blockingCollider != null)
            blockingCollider.enabled = false;
        if (shelfTransform != null)
        {
            shelfTransform.position = shelfBasePos + new Vector3(1.1f, -0.15f, 0f);
            shelfTransform.rotation = Quaternion.Euler(0f, 0f, -18f);
        }
        if (shelfRenderer != null)
        {
            var c = shelfRenderer.color;
            shelfRenderer.color = new Color(c.r, c.g, c.b, 0.55f);
        }
    }

    private void SpawnDebrisBurst(int count)
    {
        for (var i = 0; i < count; i++)
        {
            var bit = new GameObject($"Debris_{i}", typeof(SpriteRenderer));
            bit.transform.SetParent(debrisParent);
            bit.transform.position = shelfTransform.position + new Vector3(
                Random.Range(-0.3f, 0.8f), Random.Range(-0.2f, 0.5f), 0f);
            bit.transform.localScale = Vector3.one * Random.Range(0.07f, 0.16f);
            var sr = bit.GetComponent<SpriteRenderer>();
            sr.color = new Color(
                Random.Range(0.32f, 0.55f),
                Random.Range(0.22f, 0.36f),
                Random.Range(0.12f, 0.22f),
                0.9f);
            sr.sortingOrder = 4;
            bit.AddComponent<EnvDebrisDrift>().Init(
                new Vector2(Random.Range(0.5f, 1.6f), Random.Range(0.8f, 2.2f)),
                Random.Range(1.4f, 2.4f));
        }
    }
}
