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
        if (eventBus == null)
            eventBus = Resources.Load<EventBus>("EventBus");

        if (keyManager != null && uiManager != null)
            uiManager.keyManager = keyManager;

        if (eventBus != null)
            eventBus.OnChapterCompleted += HandleChapterCompleted;

        EnsureStoryboardSystems();
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
        if (GetComponent<ObjectiveGuideController>() == null)
            gameObject.AddComponent<ObjectiveGuideController>();
        if (GetComponent<TutorialCoach>() == null)
            gameObject.AddComponent<TutorialCoach>();
        if (FindFirstObjectByType<HideSpot>() == null)
            CreateHideArch();

        var passage = GameObject.Find("PassageZone");
        if (passage != null && passage.GetComponent<PassageEscapeZone>() == null)
            passage.AddComponent<PassageEscapeZone>();

        EnsureRoomZones();
        EnsurePlayerAnimator();
        EnsureFoyerProps();
    }

    /// <summary>Lightweight decorative props so the foyer doesn't feel empty.</summary>
    private static void EnsureFoyerProps()
    {
        if (GameObject.Find("FoyerProps") != null) return;
        var root = new GameObject("FoyerProps");
        SpawnProp(root.transform, "Rug", new Vector3(0.2f, -1.35f, 0.2f), new Vector3(2.8f, 0.35f, 1f),
            new Color(0.35f, 0.18f, 0.14f, 0.55f), 2);
        SpawnProp(root.transform, "PaintingA", new Vector3(-1.8f, 1.1f, 0.3f), new Vector3(0.7f, 0.9f, 1f),
            new Color(0.25f, 0.22f, 0.18f, 0.85f), 3);
        SpawnProp(root.transform, "PaintingB", new Vector3(1.6f, 1.25f, 0.3f), new Vector3(0.55f, 0.7f, 1f),
            new Color(0.22f, 0.2f, 0.28f, 0.85f), 3);
        SpawnProp(root.transform, "Cabinet", new Vector3(2.4f, -0.55f, 0.25f), new Vector3(0.9f, 1.1f, 1f),
            new Color(0.28f, 0.18f, 0.12f, 0.9f), 4);
        SpawnProp(root.transform, "WindowGlow", new Vector3(-2.6f, 1.4f, 0.1f), new Vector3(0.9f, 1.4f, 1f),
            new Color(1f, 0.85f, 0.55f, 0.18f), 1);
    }

    private static void SpawnProp(Transform parent, string name, Vector3 pos, Vector3 scale, Color color, int order)
    {
        var go = new GameObject(name, typeof(SpriteRenderer));
        go.transform.SetParent(parent);
        go.transform.position = pos;
        go.transform.localScale = scale;
        var sr = go.GetComponent<SpriteRenderer>();
        sr.color = color;
        sr.sortingOrder = order;
        // 1x1 white sprite
        var tex = Texture2D.whiteTexture;
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 16f);
    }

    private static void EnsurePlayerAnimator()
    {
        var player = FindFirstObjectByType<PlayerController>();
        if (player == null) return;

        if (player.GetComponent<PlayerSpriteAnimator>() == null)
            player.gameObject.AddComponent<PlayerSpriteAnimator>();

        if (player.GetComponent<GhostPhaseVFX>() == null)
            player.gameObject.AddComponent<GhostPhaseVFX>();

        // Tighten rigidbody defaults for mobile feel
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