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