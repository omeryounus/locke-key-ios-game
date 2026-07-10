using UnityEngine;

/// <summary>
/// Full-bleed cinematic room backdrops that cover the camera, with optional
/// soft depth layer. Uses landscape game art (not stretched portrait UI plates).
/// </summary>
public class ChapterRoomBackgrounds : MonoBehaviour
{
    [SerializeField] private ChapterRoomDirector roomDirector;

    private SpriteRenderer backdrop;
    private SpriteRenderer depthLayer;
    private ParallaxLayer backdropParallax;
    private ParallaxLayer depthParallax;
    private ChapterRoomZone.RoomId lastRoom = (ChapterRoomZone.RoomId)(-1);
    private string lastMapDest = "";

    private Sprite foyerSprite;
    private Sprite librarySprite;
    private Sprite sealedSprite;
    private Sprite wellhouseSprite;
    private Sprite exteriorSprite;
    private Sprite memorySprite;

    private void Awake()
    {
        if (roomDirector == null)
            roomDirector = FindFirstObjectByType<ChapterRoomDirector>();

        foyerSprite = LoadBg("bg_room_foyer_16x9")
                      ?? Resources.Load<Sprite>(ArtPaths.BgFoyerLandscape);
        librarySprite = LoadBg("bg_room_library_16x9")
                        ?? Resources.Load<Sprite>("Art/Parallax/library_far");
        sealedSprite = LoadBg("bg_room_sealed_16x9")
                       ?? Resources.Load<Sprite>("Art/Parallax/sealed_passage");
        wellhouseSprite = Resources.Load<Sprite>(ArtPaths.BgWellhouse)
                          ?? LoadBg("bg_wellhouse_exterior");
        exteriorSprite = LoadBg("bg_wellhouse_exterior")
                         ?? Resources.Load<Sprite>(ArtPaths.BgFoyerLandscape)
                         ?? foyerSprite;
        memorySprite = LoadBg("bg_room_memory_16x9") ?? librarySprite;

        CreateLayers();
        ApplyRoom(ChapterRoomZone.RoomId.ExteriorEntrance, force: true);
    }

    private static Sprite LoadBg(string fileName) =>
        Resources.Load<Sprite>($"Art/Backgrounds/{fileName}");

    private void Update()
    {
        var mapDest = ChapterSaveManager.Instance?.ActiveMapDestination ?? ChapterMapDestination.Foyer;
        if (mapDest == ChapterMapDestination.Wellhouse)
        {
            if (lastMapDest != mapDest)
            {
                lastMapDest = mapDest;
                ApplySprite(wellhouseSprite ?? foyerSprite);
            }
            return;
        }

        lastMapDest = mapDest;
        if (roomDirector == null) return;

        if (roomDirector.CurrentRoom != lastRoom)
            ApplyRoom(roomDirector.CurrentRoom, force: true);
    }

    private void ApplyRoom(ChapterRoomZone.RoomId room, bool force)
    {
        if (!force && room == lastRoom) return;
        lastRoom = room;

        Sprite sprite = room switch
        {
            ChapterRoomZone.RoomId.ExteriorEntrance => exteriorSprite ?? foyerSprite,
            ChapterRoomZone.RoomId.Foyer => foyerSprite,
            ChapterRoomZone.RoomId.Library => librarySprite,
            ChapterRoomZone.RoomId.MemoryPortrait => memorySprite ?? librarySprite,
            ChapterRoomZone.RoomId.SealedPassage => sealedSprite,
            _ => foyerSprite
        };

        ApplySprite(sprite);
    }

    private void CreateLayers()
    {
        backdrop = CreateLayer("RoomBackdrop", -50, 1f, 2f, out backdropParallax);
        depthLayer = CreateLayer("RoomDepthTint", -45, 0.92f, 1.5f, out depthParallax);
        depthLayer.color = new Color(0.15f, 0.12f, 0.2f, 0.18f);
        depthLayer.enabled = false; // reserved; keep backdrop clean
    }

    private static SpriteRenderer CreateLayer(string name, int sortOrder, float scroll, float z, out ParallaxLayer parallax)
    {
        var go = GameObject.Find(name);
        if (go == null)
            go = new GameObject(name);

        var renderer = go.GetComponent<SpriteRenderer>();
        if (renderer == null)
            renderer = go.AddComponent<SpriteRenderer>();

        renderer.sortingLayerName = "Background";
        renderer.sortingOrder = sortOrder;
        renderer.color = Color.white;
        go.transform.position = new Vector3(0f, 0f, z);

        parallax = go.GetComponent<ParallaxLayer>();
        if (parallax == null)
            parallax = go.AddComponent<ParallaxLayer>();
        parallax.Configure(scroll, followCameraY: true, verticalOffset: 0f);
        return renderer;
    }

    private void ApplySprite(Sprite sprite)
    {
        if (backdrop == null || sprite == null) return;

        backdrop.enabled = true;
        backdrop.sprite = sprite;
        backdrop.color = Color.white;
        FitCoverCamera(backdrop.transform, sprite);
        backdropParallax?.Configure(1f, true, 0f);

        // Hide legacy translucent parallax stacks that wash out the scene.
        DisableLegacyParallaxStacks();
    }

    private static void DisableLegacyParallaxStacks()
    {
        foreach (var name in new[] { "ParallaxFar", "ParallaxMid", "ParallaxNear" })
        {
            var go = GameObject.Find(name);
            if (go == null) continue;
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;
        }
    }

    /// <summary>
    /// Scale sprite to fully cover the orthographic camera view (no letterboxing, no stretch).
    /// </summary>
    private static void FitCoverCamera(Transform t, Sprite sprite)
    {
        var cam = Camera.main;
        if (cam == null || sprite == null) return;

        float worldH = cam.orthographicSize * 2f;
        float worldW = worldH * cam.aspect;

        var size = sprite.bounds.size;
        if (size.x < 0.001f || size.y < 0.001f) return;

        // Cover: scale so both dimensions fill (may crop edges).
        float scale = Mathf.Max(worldW / size.x, worldH / size.y) * 1.02f;
        t.localScale = new Vector3(scale, scale, 1f);

        // Center on camera each apply; parallax keeps it locked afterward.
        var camPos = cam.transform.position;
        t.position = new Vector3(camPos.x, camPos.y, t.position.z);
    }
}
