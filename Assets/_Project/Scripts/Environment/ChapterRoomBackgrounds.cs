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

    private ChapterRoomZone.RoomId lastRoomLog = (ChapterRoomZone.RoomId)(-1);

    private void Update()
    {
        if (roomDirector == null) return;

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

    private void ApplySet(LayerSet set)
    {
        if (farRenderer != null && farRenderer.sprite != set.far)
        {
            Debug.Log($"[Backgrounds] Applying far sprite: {(set.far != null ? set.far.name : "null")}");
            farRenderer.sprite = set.far;
            AdjustScaleToAspect(farRenderer.transform, set.far, 28f, -4.5f);
        }
        if (midRenderer != null && midRenderer.sprite != set.mid)
        {
            Debug.Log($"[Backgrounds] Applying mid sprite: {(set.mid != null ? set.mid.name : "null")}");
            midRenderer.sprite = set.mid;
            AdjustScaleToAspect(midRenderer.transform, set.mid, 30f, -4.5f);
        }
        if (nearRenderer != null && nearRenderer.sprite != set.near)
        {
            Debug.Log($"[Backgrounds] Applying near sprite: {(set.near != null ? set.near.name : "null")}");
            nearRenderer.sprite = set.near;
            AdjustScaleToAspect(nearRenderer.transform, set.near, 32f, -4.5f);
        }
        farParallax?.Configure(set.farScroll);
        midParallax?.Configure(set.midScroll);
        nearParallax?.Configure(set.nearScroll);
    }

    private void AdjustScaleToAspect(Transform t, Sprite sprite, float targetWidth, float baseBottomY)
    {
        if (sprite == null) return;
        var spriteWidth = sprite.bounds.size.x;
        var spriteHeight = sprite.bounds.size.y;

        var scaleFactor = targetWidth / spriteWidth;
        t.localScale = new Vector3(scaleFactor, scaleFactor, 1f);

        var actualHeight = spriteHeight * scaleFactor;
        t.position = new Vector3(t.position.x, baseBottomY + actualHeight * 0.5f, t.position.z);
    }
}