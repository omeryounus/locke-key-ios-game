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
        if (FindFirstObjectByType<HideSpot>() == null)
            CreateHideArch();

        var passage = GameObject.Find("PassageZone");
        if (passage != null && passage.GetComponent<PassageEscapeZone>() == null)
            passage.AddComponent<PassageEscapeZone>();

        EnsureRoomZones();
        EnsurePlayerAnimator();
        EnsureGameplayCamera();
    }

    private static void EnsureGameplayCamera()
    {
        var cam = Camera.main;
        if (cam == null || cam.GetComponent<GameplayViewportCamera>() != null)
            return;

        cam.gameObject.AddComponent<GameplayViewportCamera>();
    }

    private static void EnsurePlayerAnimator()
    {
        var player = FindFirstObjectByType<PlayerController>();
        if (player == null || player.GetComponent<PlayerSpriteAnimator>() != null)
            return;
        player.gameObject.AddComponent<PlayerSpriteAnimator>();
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
        StartCoroutine(LogDiagnosticsRoutine());
    }

    private System.Collections.IEnumerator LogDiagnosticsRoutine()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        
        Debug.Log("=== RUNTIME RENDER DIAGNOSTICS ===");
        
        // 1. Camera
        var cam = Camera.main;
        if (cam != null)
        {
            var follow = cam.GetComponent<CameraFollow2D>();
            Debug.Log($"[Diag] Camera: pos={cam.transform.position}, target={(follow != null && follow.target != null ? follow.target.name : "null")}, size={cam.orthographicSize}");
        }
        else
        {
            Debug.Log("[Diag] Camera: No Main Camera found!");
        }

        // 2. Player
        var player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            var sr = player.GetComponent<SpriteRenderer>();
            var active = player.gameObject.activeInHierarchy;
            var pos = player.transform.position;
            var scale = player.transform.localScale;
            var spriteName = sr != null && sr.sprite != null ? sr.sprite.name : "null";
            var sLayer = sr != null ? sr.sortingLayerName : "null";
            var sOrder = sr != null ? sr.sortingOrder : 0;
            var enabled = sr != null && sr.enabled;
            Debug.Log($"[Diag] Player: active={active}, pos={pos}, scale={scale}, enabled={enabled}, sprite={spriteName}, sortingLayer={sLayer}, sortingOrder={sOrder}");
        }
        else
        {
            Debug.Log("[Diag] Player: FindFirstObjectByType<PlayerController> returned null!");
        }

        // 3. Parallax Backgrounds
        string[] bgNames = { "ParallaxFar", "ParallaxMid", "ParallaxNear" };
        foreach (var name in bgNames)
        {
            var bg = GameObject.Find(name);
            if (bg != null)
            {
                var sr = bg.GetComponent<SpriteRenderer>();
                var pos = bg.transform.position;
                var scale = bg.transform.localScale;
                var spriteName = sr != null && sr.sprite != null ? sr.sprite.name : "null";
                var sLayer = sr != null ? sr.sortingLayerName : "null";
                var sOrder = sr != null ? sr.sortingOrder : 0;
                var active = bg.activeInHierarchy;
                var enabled = sr != null && sr.enabled;
                Debug.Log($"[Diag] Bg '{name}': active={active}, pos={pos}, scale={scale}, enabled={enabled}, sprite={spriteName}, sortingLayer={sLayer}, sortingOrder={sOrder}");
            }
            else
            {
                Debug.Log($"[Diag] Bg '{name}': GameObject.Find returned null!");
            }
        }
        Debug.Log("==================================");
    }

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