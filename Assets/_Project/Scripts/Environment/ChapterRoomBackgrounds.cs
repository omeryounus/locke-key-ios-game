using UnityEngine;

/// <summary>
/// Swaps layered parallax backgrounds per Chapter 1 room.
/// </summary>
public class ChapterRoomBackgrounds : MonoBehaviour
{
    private struct LayerSet
    {
        public Sprite far;
        public Sprite mid;
        public Sprite near;
        public float farScroll;
        public float midScroll;
        public float nearScroll;
    }

    [SerializeField] private ChapterRoomDirector roomDirector;

    private SpriteRenderer farRenderer;
    private SpriteRenderer midRenderer;
    private SpriteRenderer nearRenderer;
    private ParallaxLayer farParallax;
    private ParallaxLayer midParallax;
    private ParallaxLayer nearParallax;

    private LayerSet foyer;
    private LayerSet library;
    private LayerSet sealedPassage;

    private void Awake()
    {
        if (roomDirector == null)
            roomDirector = FindFirstObjectByType<ChapterRoomDirector>();

        foyer = BuildFoyerSet();
        library = BuildSet("library", 0.06f, 0.16f, 0.3f);
        sealedPassage = new LayerSet
        {
            far = Load("sealed_passage"),
            mid = Load("library_mid"),
            near = Load("library_near"),
            farScroll = 0.04f,
            midScroll = 0.1f,
            nearScroll = 0.22f
        };

        CreateLayers();
        ApplySet(foyer);
    }

    private ChapterRoomZone.RoomId lastRoomLog = (ChapterRoomZone.RoomId)(-1);

    private void Update()
    {
        if (roomDirector == null) return;

        var mapDest = ChapterSaveManager.Instance?.ActiveMapDestination ?? ChapterMapDestination.Foyer;
        if (mapDest == ChapterMapDestination.Wellhouse)
        {
            ApplyWellhouseBackdrop();
            return;
        }

        if (roomDirector.CurrentRoom != lastRoomLog)
        {
            Debug.Log($"[Backgrounds] Current Room changed to: {roomDirector.CurrentRoom}");
            lastRoomLog = roomDirector.CurrentRoom;
        }

        switch (roomDirector.CurrentRoom)
        {
            case ChapterRoomZone.RoomId.ExteriorEntrance:
            case ChapterRoomZone.RoomId.Foyer:
                ApplySet(foyer);
                break;
            case ChapterRoomZone.RoomId.Library:
            case ChapterRoomZone.RoomId.MemoryPortrait:
                ApplySet(library);
                break;
            case ChapterRoomZone.RoomId.SealedPassage:
                ApplySet(sealedPassage);
                break;
        }
    }

    private static LayerSet BuildFoyerSet()
    {
        var portrait = Resources.Load<Sprite>(ArtPaths.BgFoyerPortrait);
        return new LayerSet
        {
            far = portrait != null ? portrait : Load("foyer_far"),
            mid = portrait != null ? null : Load("foyer_mid"),
            near = portrait != null ? null : Load("foyer_near"),
            farScroll = 0.04f,
            midScroll = 0.1f,
            nearScroll = 0.2f
        };
    }

    private static LayerSet BuildSet(string prefix, float farScroll, float midScroll, float nearScroll) =>
        new()
        {
            far = Load($"{prefix}_far"),
            mid = Load($"{prefix}_mid"),
            near = Load($"{prefix}_near"),
            farScroll = farScroll,
            midScroll = midScroll,
            nearScroll = nearScroll
        };

    private static Sprite Load(string name) => Resources.Load<Sprite>($"Art/Parallax/{name}");

    private void CreateLayers()
    {
        farRenderer = CreateLayer("ParallaxFar", -40, 0.08f, 1f, out farParallax);
        midRenderer = CreateLayer("ParallaxMid", -25, 0.18f, 0.5f, out midParallax);
        nearRenderer = CreateLayer("ParallaxNear", -8, 0.32f, -0.25f, out nearParallax);
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

        var pos = go.transform.position;
        go.transform.position = new Vector3(pos.x, pos.y, z);

        parallax = go.GetComponent<ParallaxLayer>();
        if (parallax == null)
            parallax = go.AddComponent<ParallaxLayer>();

        parallax.Configure(scroll);
        return renderer;
    }

    private void ApplyWellhouseBackdrop()
    {
        var wellhouse = Resources.Load<Sprite>(ArtPaths.BgWellhouse);
        if (wellhouse == null) return;

        ApplyLayer(farRenderer, wellhouse, 0f, -4.2f, 1f);
        if (midRenderer != null) midRenderer.enabled = false;
        if (nearRenderer != null) nearRenderer.enabled = false;
    }

    private void ApplySet(LayerSet set)
    {
        ApplyLayer(farRenderer, set.far, set.farScroll, -4.2f, 1f);
        ApplyLayer(midRenderer, set.mid, set.midScroll, -3.8f, 0.55f);
        ApplyLayer(nearRenderer, set.near, set.nearScroll, -3.4f, 0.35f);
    }

    private static void ApplyLayer(SpriteRenderer renderer, Sprite sprite, float scroll, float baseBottomY, float alpha)
    {
        if (renderer == null) return;

        if (sprite == null)
        {
            renderer.sprite = null;
            renderer.enabled = false;
            return;
        }

        renderer.enabled = true;
        renderer.sprite = sprite;
        renderer.color = new Color(1f, 1f, 1f, alpha);
        FitSpriteToCameraHeight(renderer.transform, sprite, baseBottomY);

        var parallax = renderer.GetComponent<ParallaxLayer>();
        parallax?.Configure(scroll);
    }

    private static void FitSpriteToCameraHeight(Transform t, Sprite sprite, float baseBottomY)
    {
        var cam = Camera.main;
        float targetHeight = cam != null ? cam.orthographicSize * 2.05f : 8.5f;

        var spriteHeight = sprite.bounds.size.y;
        if (spriteHeight <= 0.001f) return;

        var scale = targetHeight / spriteHeight;
        t.localScale = new Vector3(scale, scale, 1f);

        var actualHeight = spriteHeight * scale;
        var pos = t.position;
        t.position = new Vector3(pos.x, baseBottomY + actualHeight * 0.5f, pos.z);
    }
}