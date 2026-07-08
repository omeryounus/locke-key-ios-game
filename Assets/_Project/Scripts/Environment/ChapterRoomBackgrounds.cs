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

        foyer = BuildSet("foyer", 0.06f, 0.14f, 0.28f);
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

    private void Update()
    {
        if (roomDirector == null) return;

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
        farRenderer = CreateLayer("RoomFar", -40, 0.08f, 28f, 8f, 2.5f, 1f, out farParallax);
        midRenderer = CreateLayer("RoomMid", -25, 0.18f, 30f, 9f, 1.8f, 0.5f, out midParallax);
        nearRenderer = CreateLayer("RoomNear", -8, 0.32f, 32f, 10f, 0.8f, -0.25f, out nearParallax);
    }

    private static SpriteRenderer CreateLayer(string name, int sortOrder, float scroll, float scaleX, float scaleY,
        float y, float z, out ParallaxLayer parallax)
    {
        var go = new GameObject(name);
        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sortingOrder = sortOrder;
        renderer.color = Color.white;
        go.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        go.transform.position = new Vector3(0f, y, z);
        parallax = go.AddComponent<ParallaxLayer>();
        parallax.Configure(scroll);
        return renderer;
    }

    private void ApplySet(LayerSet set)
    {
        if (farRenderer != null) farRenderer.sprite = set.far;
        if (midRenderer != null) midRenderer.sprite = set.mid;
        if (nearRenderer != null) nearRenderer.sprite = set.near;
        farParallax?.Configure(set.farScroll);
        midParallax?.Configure(set.midScroll);
        nearParallax?.Configure(set.nearScroll);
    }
}