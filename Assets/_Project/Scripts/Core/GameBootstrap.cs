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
        if (GetComponent<GhostPhaseVFX>() == null)
            gameObject.AddComponent<GhostPhaseVFX>();
        if (GetComponent<ParticleVFXController>() == null)
            gameObject.AddComponent<ParticleVFXController>();
        if (GetComponent<EchoTensionController>() == null)
            gameObject.AddComponent<EchoTensionController>();

        if (FindFirstObjectByType<HideSpot>() == null)
            CreateHideArch();

        var passage = GameObject.Find("PassageZone");
        if (passage != null && passage.GetComponent<PassageEscapeZone>() == null)
            passage.AddComponent<PassageEscapeZone>();

        EnsureRoomZones();
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