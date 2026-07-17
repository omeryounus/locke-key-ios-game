using UnityEngine;

/// <summary>
/// Scene bootstrap — wires core managers and logs chapter start.
/// </summary>
public class GameBootstrap : MonoBehaviour
{
    [Header("Managers")]
    public KeyManager keyManager;
    public UIManager uiManager;
    public EventBus eventBus;

    private void Awake()
    {
        ApplyMobileRuntimeDefaults();

        if (eventBus == null)
            eventBus = Resources.Load<EventBus>("EventBus");

        if (keyManager != null && uiManager != null)
            uiManager.keyManager = keyManager;

        if (eventBus != null)
            eventBus.OnChapterCompleted += HandleChapterCompleted;

        EnsureStoryboardSystems();
    }

    private static void ApplyMobileRuntimeDefaults()
    {
        // Commercial mobile targets
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
        // 2D URP scene — keep shadows cheap
        QualitySettings.shadows = ShadowQuality.Disable;
        // Avoid runaway physics if frame hitch
        Time.maximumDeltaTime = 0.1f;
        Physics2D.simulationMode = SimulationMode2D.FixedUpdate;
    }

    private void EnsureStoryboardSystems()
    {
        if (GetComponent<ChapterSaveManager>() == null)
            gameObject.AddComponent<ChapterSaveManager>();
        if (GetComponent<ChapterBeatDirector>() == null)
            gameObject.AddComponent<ChapterBeatDirector>();
        if (GetComponent<ChapterRoomDirector>() == null)
            gameObject.AddComponent<ChapterRoomDirector>();
        if (GetComponent<GameAudioController>() == null)
            gameObject.AddComponent<GameAudioController>();
        if (GetComponent<GhostPhaseMomentController>() == null)
            gameObject.AddComponent<GhostPhaseMomentController>();
        if (GetComponent<EchoRecoveryController>() == null)
            gameObject.AddComponent<EchoRecoveryController>();
        if (GetComponent<ChapterRoomBackgrounds>() == null)
            gameObject.AddComponent<ChapterRoomBackgrounds>();
        if (GetComponent<ParticleVFXController>() == null)
            gameObject.AddComponent<ParticleVFXController>();
        if (GetComponent<EchoTensionController>() == null)
            gameObject.AddComponent<EchoTensionController>();
        if (GetComponent<ChapterEndScreen>() == null)
            gameObject.AddComponent<ChapterEndScreen>();
        if (GetComponent<SceneAtmosphereController>() == null)
            gameObject.AddComponent<SceneAtmosphereController>();
        if (GetComponent<LivingWorldAtmosphere>() == null)
            gameObject.AddComponent<LivingWorldAtmosphere>();
        if (GetComponent<ObjectiveGuideController>() == null)
            gameObject.AddComponent<ObjectiveGuideController>();
        if (GetComponent<TutorialCoach>() == null)
            gameObject.AddComponent<TutorialCoach>();
        if (GetComponent<FoyerEnvironmentBuilder>() == null)
            gameObject.AddComponent<FoyerEnvironmentBuilder>();
        if (GetComponent<RoomEnvironmentDirector>() == null)
            gameObject.AddComponent<RoomEnvironmentDirector>();
        if (GetComponent<ProductionPropSpawner>() == null)
            gameObject.AddComponent<ProductionPropSpawner>();
        if (GetComponent<PlayableWorldFoundation>() == null)
            gameObject.AddComponent<PlayableWorldFoundation>();
        if (GetComponent<CinematicPostProcessOverlay>() == null)
            gameObject.AddComponent<CinematicPostProcessOverlay>();
        if (FindFirstObjectByType<HideSpot>() == null)
            CreateHideArch();

        var passage = GameObject.Find("PassageZone");
        if (passage != null && passage.GetComponent<PassageEscapeZone>() == null)
            passage.AddComponent<PassageEscapeZone>();

        EnsureRoomZones();
        EnsurePlayerAnimator();
        EnsureFrontDoorHighlight();
        EnsureMirrorPair();
        EnsureHiddenKeyVisuals();
        EnsureGhostKeyAbilityClean();
    }

    private static void EnsureGhostKeyAbilityClean()
    {
        var player = FindFirstObjectByType<PlayerController>();
        if (player == null) return;
        // Keep thin proxy; remove if older broken swipe version somehow remains with missing deps
        if (player.GetComponent<GhostKeyAbility>() == null)
            player.gameObject.AddComponent<GhostKeyAbility>();
        if (player.GetComponent<KeyManager>() == null)
        {
            // KeyManager is usually scene-level
        }
        var km = FindFirstObjectByType<KeyManager>();
        if (km != null && km.player == null)
            km.player = player;
    }

    /// <summary>Pair two mirrors for Mirror Key travel (library ↔ memory wing).</summary>
    private static void EnsureMirrorPair()
    {
        var existing = FindObjectsByType<MirrorSurface>(FindObjectsSortMode.None);
        MirrorSurface a = null, b = null;
        if (existing.Length >= 2)
        {
            a = existing[0];
            b = existing[1];
        }
        else
        {
            a = CreateMirror("Mirror_Library", new Vector3(3.4f, 0.2f, 0f));
            b = CreateMirror("Mirror_Memory", new Vector3(9.6f, 0.2f, 0f));
        }

        if (a != null && b != null)
        {
            a.destinationMirror = b;
            b.destinationMirror = a;
            a.isReflective = true;
            b.isReflective = true;
        }
    }

    private static MirrorSurface CreateMirror(string name, Vector3 pos)
    {
        var existing = GameObject.Find(name);
        GameObject go;
        if (existing != null)
            go = existing;
        else
        {
            go = new GameObject(name, typeof(SpriteRenderer));
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.85f, 1.6f, 1f);
            var sr = go.GetComponent<SpriteRenderer>();
            sr.color = new Color(0.55f, 0.75f, 0.95f, 0.45f);
            sr.sortingOrder = 6;
            // soft disc sprite
            var tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            for (var y = 0; y < 32; y++)
            for (var x = 0; x < 32; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(15.5f, 15.5f)) / 16f;
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(1f - d)));
            }
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32f);
        }

        var mirror = go.GetComponent<MirrorSurface>() ?? go.AddComponent<MirrorSurface>();
        return mirror;
    }

    private static void EnsureHiddenKeyVisuals()
    {
        var puzzle = FindFirstObjectByType<HiddenKeyPuzzle>();
        if (puzzle == null) return;

        // Ensure glow sprite/light exist for ghost-only reveal
        var t = puzzle.transform;
        var glowT = t.Find("HiddenGlow");
        if (glowT == null)
        {
            var glow = new GameObject("HiddenGlow", typeof(SpriteRenderer));
            glow.transform.SetParent(t, false);
            glow.transform.localPosition = Vector3.zero;
            glow.transform.localScale = new Vector3(0.9f, 1.2f, 1f);
            var sr = glow.GetComponent<SpriteRenderer>();
            sr.color = new Color(0.3f, 0.95f, 0.9f, 0f);
            sr.sortingOrder = 12;
            var tex = new Texture2D(24, 24, TextureFormat.RGBA32, false);
            for (var y = 0; y < 24; y++)
            for (var x = 0; x < 24; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(11.5f, 11.5f)) / 12f;
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Pow(Mathf.Clamp01(1f - d), 1.4f)));
            }
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 24, 24), new Vector2(0.5f, 0.5f), 24f);

            puzzle.BindGlow(sr);
        }
    }

    private static void EnsureFrontDoorHighlight()
    {
        var door = FindFirstObjectByType<StuckDoorPuzzle>();
        if (door == null) return;
        if (door.GetComponent<FrontDoorHighlight>() == null)
            door.gameObject.AddComponent<FrontDoorHighlight>();
    }

    private static void EnsurePlayerAnimator()
    {
        var player = FindFirstObjectByType<PlayerController>();
        if (player == null) return;

        // 2.5D layered rig + multi-frame atlas animator
        if (player.GetComponent<PlayerCharacterRig>() == null)
            player.gameObject.AddComponent<PlayerCharacterRig>();

        if (player.GetComponent<PlayerSpriteAnimator>() == null)
            player.gameObject.AddComponent<PlayerSpriteAnimator>();

        // Dual-path Unity Animator graph bridge (parameters mirror runtime director)
        if (player.GetComponent<PlayerAnimatorGraphDriver>() == null)
            player.gameObject.AddComponent<PlayerAnimatorGraphDriver>();

        // Foot-plant dust / soft plant settle
        if (player.GetComponent<PlayerFootContactVFX>() == null)
            player.gameObject.AddComponent<PlayerFootContactVFX>();

        // Visibility boost still adds fill light; secondary motion owned by rig
        if (player.GetComponent<PlayerVisibilityBoost>() == null)
            player.gameObject.AddComponent<PlayerVisibilityBoost>();

        // Legacy idle detail disabled when modern animator is present
        var idleDetail = player.GetComponent<PlayerIdleDetail>();
        if (idleDetail != null)
            idleDetail.enabled = false;

        if (player.GetComponent<GhostPhaseVFX>() == null)
            player.gameObject.AddComponent<GhostPhaseVFX>();

        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.freezeRotation = true;
            if (rb.gravityScale < 2f)
                rb.gravityScale = 2.4f;
        }
    }

    private static void EnsureRoomZones()
    {
        EnsureRoomZone("Room_Exterior", ChapterRoomZone.RoomId.ExteriorEntrance, -5.5f, 4f);
        EnsureRoomZone("Room_Foyer", ChapterRoomZone.RoomId.Foyer, -1.5f, 4f);
        EnsureRoomZone("Room_Library", ChapterRoomZone.RoomId.Library, 2.5f, 4f);
        EnsureRoomZone("Room_SealedPassage", ChapterRoomZone.RoomId.SealedPassage, 6.5f, 4f);
        EnsureRoomZone("Room_Memory", ChapterRoomZone.RoomId.MemoryPortrait, 10f, 4f);
    }

    private static void EnsureRoomZone(string name, ChapterRoomZone.RoomId room, float x, float width)
    {
        var existing = GameObject.Find(name);
        if (existing != null) return;

        var zoneGo = new GameObject(name);
        zoneGo.transform.position = new Vector3(x, -1f, 0f);
        var col = zoneGo.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(width, 4f);
        var zone = zoneGo.AddComponent<ChapterRoomZone>();
        zone.Configure(room);
    }

    private static void CreateHideArch()
    {
        var arch = new GameObject("HideArch");
        arch.transform.position = new Vector3(5.2f, 0f, 0f);
        var col = arch.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1.4f, 2.6f);
        arch.AddComponent<HideSpot>();
    }

    private void Start()
    {
        Debug.Log("Locke & Key: Chapter 1 — Welcome to Keyhouse");
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // Diagnostics only in editor/dev builds — avoid log spam on production devices.
        StartCoroutine(LogDiagnosticsRoutine());
#endif
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private System.Collections.IEnumerator LogDiagnosticsRoutine()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(5.0f);

            var player = FindFirstObjectByType<PlayerController>();
            var gameplay = FindFirstObjectByType<TouchGameplayController>();
            if (player != null && gameplay != null)
            {
                var rb = player.GetComponent<Rigidbody2D>();
                Debug.Log($"[Diag] pos={player.transform.position} vel={(rb != null ? rb.linearVelocity.ToString() : "null")} move={gameplay.MoveInput}");
            }
        }
    }
#endif

    private void OnDestroy()
    {
        if (eventBus != null)
            eventBus.OnChapterCompleted -= HandleChapterCompleted;
    }

    private void HandleChapterCompleted()
    {
        Debug.Log("Chapter 1 complete.");
        ChapterSaveManager.Instance?.RecordChapterComplete();
    }
}
