using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Puzzle 5: The Hidden Key.
/// Player must use the Ghost Key to enter Ghost Phase, revealing a secret glow in the wall,
/// and interact with it while phasing to reach inside the solid wall and retrieve the Mirror Key.
/// </summary>
public class HiddenKeyPuzzle : PuzzleBase
{
    [Header("Visual Indicators")]
    [SerializeField] private SpriteRenderer hiddenObjectGlow;
    [SerializeField] private Light2D glowLight;
    [SerializeField] private float fadeSpeed = 3f;
    [SerializeField] private float targetLightIntensity = 1.2f;

    [Header("Configuration")]
    [SerializeField] private string mirrorKeyId = "mirror";

    private PlayerController player;
    private bool animating;

    public override bool CanInteract => !isSolved && !animating;

    public override string InteractionHint =>
        isSolved
            ? string.Empty
            : player != null && player.IsGhostPhasing
                ? "Glowing outline — reach inside wall (Interact)"
                : "Odd brickwork — cold to the touch.";

    protected override void Awake()
    {
        base.Awake();
        puzzleID = "chapter1_hidden_key";
        requiresSpecificKey = false; // Resolved contextually by checking player's GhostPhase state

        if (player == null)
            player = FindFirstObjectByType<PlayerController>();

        EnsureGlowVisuals();
    }

    /// <summary>Runtime-safe glow setup when scene refs are missing.</summary>
    public void BindGlow(SpriteRenderer sr)
    {
        hiddenObjectGlow = sr;
        if (hiddenObjectGlow != null)
        {
            var c = hiddenObjectGlow.color;
            c.a = 0f;
            hiddenObjectGlow.color = c;
        }
    }

    private void EnsureGlowVisuals()
    {
        if (hiddenObjectGlow == null)
        {
            var glowT = transform.Find("HiddenGlow");
            if (glowT != null)
                hiddenObjectGlow = glowT.GetComponent<SpriteRenderer>();
        }

        if (hiddenObjectGlow == null)
        {
            var glow = new GameObject("HiddenGlow", typeof(SpriteRenderer));
            glow.transform.SetParent(transform, false);
            glow.transform.localPosition = Vector3.zero;
            glow.transform.localScale = new Vector3(0.95f, 1.25f, 1f);
            hiddenObjectGlow = glow.GetComponent<SpriteRenderer>();
            hiddenObjectGlow.sortingOrder = 12;
            var tex = new Texture2D(24, 24, TextureFormat.RGBA32, false);
            for (var y = 0; y < 24; y++)
            for (var x = 0; x < 24; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(11.5f, 11.5f)) / 12f;
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Pow(Mathf.Clamp01(1f - d), 1.4f)));
            }
            tex.Apply();
            hiddenObjectGlow.sprite = Sprite.Create(tex, new Rect(0, 0, 24, 24), new Vector2(0.5f, 0.5f), 24f);
            hiddenObjectGlow.color = new Color(0.3f, 0.95f, 0.9f, 0f);
        }
        else
        {
            var c = hiddenObjectGlow.color;
            c.a = 0f;
            hiddenObjectGlow.color = c;
        }

        if (glowLight == null)
        {
            var lightGo = new GameObject("HiddenGlowLight");
            lightGo.transform.SetParent(transform, false);
            glowLight = lightGo.AddComponent<Light2D>();
            glowLight.lightType = Light2D.LightType.Point;
            glowLight.color = new Color(0.35f, 0.95f, 0.9f);
            glowLight.pointLightOuterRadius = 2.2f;
        }
        glowLight.intensity = 0f;
    }

    private void Update()
    {
        if (player == null)
            player = FindFirstObjectByType<PlayerController>();

        bool isGhost = player != null && player.IsGhostPhasing;

        // Animate the secret indicator based on ghost phase state
        float targetAlpha = (isGhost && !isSolved) ? 1f : 0f;

        if (hiddenObjectGlow != null)
        {
            Color c = hiddenObjectGlow.color;
            c.a = Mathf.MoveTowards(c.a, targetAlpha, Time.deltaTime * fadeSpeed);
            hiddenObjectGlow.color = c;
        }

        if (glowLight != null)
        {
            float targetIntensity = (isGhost && !isSolved) ? targetLightIntensity : 0f;
            glowLight.intensity = Mathf.MoveTowards(glowLight.intensity, targetIntensity, Time.deltaTime * fadeSpeed * targetLightIntensity);
        }
    }

    public override void Interact()
    {
        if (isSolved || animating) return;

        if (player == null)
            player = FindFirstObjectByType<PlayerController>();

        bool isGhost = player != null && player.IsGhostPhasing;

        if (!isGhost)
        {
            // Give a narrative hint that something is there, but requires phasing
            StartCoroutine(RattleBrickwork());
            FindFirstObjectByType<GameAudioController>()?.PlayDoorRattle();
            FindFirstObjectByType<GameplayHUD>()?.ShowToast("You feel an icy draft behind the brick, but cannot reach inside.", 3.2f);
            OnPuzzleFailed();
            return;
        }

        // Player is ghost phasing -> solve!
        StartCoroutine(SolveSequence());
    }

    protected override void TrySolve() { }

    public override void RestoreSolvedState()
    {
        base.RestoreSolvedState();
        if (hiddenObjectGlow != null)
        {
            Color c = hiddenObjectGlow.color;
            c.a = 0f;
            hiddenObjectGlow.color = c;
        }
        if (glowLight != null)
        {
            glowLight.intensity = 0f;
        }
    }

    private IEnumerator RattleBrickwork()
    {
        animating = true;
        GameHaptics.TriggerHapticLight();
        Vector3 basePos = transform.position;
        float elapsed = 0f;
        while (elapsed < 0.25f)
        {
            elapsed += Time.deltaTime;
            float shake = Mathf.Sin(elapsed * 60f) * 0.03f * (1f - elapsed / 0.25f);
            transform.position = basePos + Vector3.right * shake;
            yield return null;
        }
        transform.position = basePos;
        animating = false;
    }

    private IEnumerator SolveSequence()
    {
        animating = true;

        // Feedback effects
        FindFirstObjectByType<GameAudioController>()?.PlayDoorUnlock();
        GameHaptics.Unlock();
        FindFirstObjectByType<CameraFollow2D>()?.Pulse(0.2f, 0.3f);
        FindFirstObjectByType<CameraFollow2D>()?.Shake(0.1f, 0.3f);
        FindFirstObjectByType<ParticleVFXController>()?.PlayMemoryBurst(transform.position);

        FindFirstObjectByType<GameplayHUD>()?.ShowToast("Phasing into the solid wall... you pull out a hidden key!", 3f);

        yield return new WaitForSeconds(0.4f);

        // Grant the Mirror Key to the player
        var keyManager = FindFirstObjectByType<KeyManager>();
        keyManager?.GrantMirrorKey();

        // Save progress
        var save = ChapterSaveManager.Instance;
        if (save != null)
        {
            save.RecordKeyDiscovered(mirrorKeyId);
            save.SaveNow();
        }

        MarkAsSolved();
        FindFirstObjectByType<ChapterBeatDirector>()?.NotifyHiddenKeySolved();
        FindFirstObjectByType<GameplayHUD>()?.ShowToast(
            "Mirror Key claimed! Stand by a mirror and Use Key to travel.", 4f);
        animating = false;
    }
}
