using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Per-room identity dressing for Chapter 1. Keeps walkable corridor clear (center band).
/// Extends foyer dressing into library, sealed passage, exterior, and memory wing.
/// </summary>
public class RoomEnvironmentDirector : MonoBehaviour
{
    private ChapterRoomDirector rooms;
    private ChapterRoomZone.RoomId last = (ChapterRoomZone.RoomId)(-1);
    private Transform libraryRoot;
    private Transform sealedRoot;
    private Transform exteriorRoot;
    private Transform memoryRoot;
    private static Sprite disc;
    private static Sprite square;

    private void Awake()
    {
        rooms = FindFirstObjectByType<ChapterRoomDirector>();
        disc ??= SoftDisc(40);
        square ??= SoftSquare(28);
        BuildAll();
    }

    private void Update()
    {
        if (rooms == null)
        {
            rooms = FindFirstObjectByType<ChapterRoomDirector>();
            return;
        }

        if (rooms.CurrentRoom == last) return;
        last = rooms.CurrentRoom;
        ApplyRoomVisibility(last);
        FindFirstObjectByType<GameAudioController>()?.SetRoomAmbience(last);
    }

    private void BuildAll()
    {
        libraryRoot = BuildLibrary();
        sealedRoot = BuildSealed();
        exteriorRoot = BuildExterior();
        memoryRoot = BuildMemory();
        // Foyer handled by FoyerEnvironmentBuilder; keep all roots active with spatial placement
    }

    private void ApplyRoomVisibility(ChapterRoomZone.RoomId room)
    {
        // Soft fog/particle density could change; for now ensure room sets are present.
        // Future: fade props by proximity. Spatial layout already places props in world X ranges.
        _ = room;
    }

    private Transform BuildLibrary()
    {
        var root = new GameObject("Env_Library").transform;
        // Library ~ x 2.5 zone
        float cx = 3.2f;
        Prop(root, "LibRug", disc, new Vector3(cx, -1.48f, 0.12f), new Vector3(2.6f, 0.4f, 1f),
            new Color(0.25f, 0.12f, 0.18f, 0.5f), 2);
        // Tall shelves left/right of path
        for (var i = 0; i < 3; i++)
        {
            float x = cx - 1.8f + i * 0.15f;
            Prop(root, $"ShelfL{i}", square, new Vector3(x - 1.2f, 0.2f + i * 0.05f, 0.2f),
                new Vector3(0.85f, 2.0f, 1f), new Color(0.22f, 0.13f, 0.08f, 0.95f), 4);
            Prop(root, $"BooksL{i}", square, new Vector3(x - 1.2f, 0.4f, 0.21f),
                new Vector3(0.65f, 0.35f, 1f), BookColor(i), 5);
        }

        Prop(root, "ReadingTable", square, new Vector3(cx + 1.6f, -0.95f, 0.2f),
            new Vector3(1.2f, 0.45f, 1f), new Color(0.3f, 0.18f, 0.1f, 0.95f), 5);
        var lamp = Prop(root, "DeskLamp", disc, new Vector3(cx + 1.5f, -0.45f, 0.25f),
            new Vector3(0.25f, 0.35f, 1f), LockeKeyUITheme.LKCandle, 6);
        Light(lamp.transform, LockeKeyUITheme.LKCandle, 0.75f, 2.2f);

        Prop(root, "Globe", disc, new Vector3(cx + 2.1f, -0.55f, 0.22f),
            new Vector3(0.4f, 0.4f, 1f), new Color(0.35f, 0.45f, 0.55f, 0.9f), 6);
        Prop(root, "LibPortrait", square, new Vector3(cx - 0.2f, 1.3f, 0.3f),
            new Vector3(0.7f, 0.9f, 1f), new Color(0.2f, 0.22f, 0.28f, 0.95f), 3);
        Prop(root, "CobwebLib", disc, new Vector3(cx + 2.4f, 2.0f, 0.1f),
            new Vector3(1.0f, 0.7f, 1f), new Color(0.9f, 0.9f, 0.95f, 0.14f), 3);
        Prop(root, "ScrollPile", square, new Vector3(cx - 1.5f, -1.2f, 0.18f),
            new Vector3(0.6f, 0.25f, 1f), new Color(0.55f, 0.45f, 0.3f, 0.85f), 4);

        // Magical floating tome
        var tome = Prop(root, "MagicTome", square, new Vector3(cx + 0.8f, 0.3f, 0.25f),
            new Vector3(0.35f, 0.25f, 1f), LockeKeyUITheme.LKMagicPurple * 0.8f + Color.white * 0.2f, 7);
        Light(tome.transform, LockeKeyUITheme.LKMagicPurple, 0.45f, 1.6f);
        tome.AddComponent<FloatBob>().Init(0.12f, 1.4f);
        return root;
    }

    private Transform BuildSealed()
    {
        var root = new GameObject("Env_Sealed").transform;
        float cx = 6.8f;
        Prop(root, "StoneFloorCrack", square, new Vector3(cx, -1.5f, 0.1f),
            new Vector3(3f, 0.15f, 1f), new Color(0.15f, 0.18f, 0.2f, 0.55f), 2);
        Prop(root, "RunePillarL", square, new Vector3(cx - 1.6f, 0.3f, 0.2f),
            new Vector3(0.45f, 2.4f, 1f), new Color(0.2f, 0.25f, 0.28f, 0.95f), 4);
        Prop(root, "RunePillarR", square, new Vector3(cx + 1.6f, 0.3f, 0.2f),
            new Vector3(0.45f, 2.4f, 1f), new Color(0.2f, 0.25f, 0.28f, 0.95f), 4);
        var rune = Prop(root, "RuneGlow", disc, new Vector3(cx, 0.8f, 0.22f),
            new Vector3(0.9f, 0.9f, 1f), LockeKeyUITheme.LKSpiritGreen, 5);
        Light(rune.transform, LockeKeyUITheme.LKSpiritGreen, 0.7f, 2.8f);
        rune.AddComponent<FloatBob>().Init(0.08f, 2f);

        Prop(root, "ChainsL", square, new Vector3(cx - 1.1f, 1.5f, 0.2f),
            new Vector3(0.08f, 1.2f, 1f), new Color(0.4f, 0.4f, 0.42f, 0.8f), 5);
        Prop(root, "ChainsR", square, new Vector3(cx + 1.1f, 1.4f, 0.2f),
            new Vector3(0.08f, 1.3f, 1f), new Color(0.4f, 0.4f, 0.42f, 0.8f), 5);
        var chainL = root.Find("ChainsL");
        if (chainL != null) chainL.gameObject.AddComponent<PendulumSway>().Init(4f, 1.8f);
        var chainR = root.Find("ChainsR");
        if (chainR != null) chainR.gameObject.AddComponent<PendulumSway>().Init(3.5f, 2.1f);

        Prop(root, "SealedCobweb", disc, new Vector3(cx - 1.8f, 2.1f, 0.1f),
            new Vector3(1.1f, 0.8f, 1f), new Color(0.85f, 0.9f, 0.95f, 0.16f), 3);
        Prop(root, "Moss", disc, new Vector3(cx + 1.4f, -1.1f, 0.15f),
            new Vector3(0.8f, 0.35f, 1f), new Color(0.2f, 0.4f, 0.22f, 0.55f), 3);
        return root;
    }

    private Transform BuildExterior()
    {
        var root = new GameObject("Env_Exterior").transform;
        float cx = -4.5f;
        Prop(root, "GatePostL", square, new Vector3(cx - 1.2f, -0.3f, 0.2f),
            new Vector3(0.35f, 1.8f, 1f), new Color(0.28f, 0.28f, 0.3f, 0.95f), 4);
        Prop(root, "GatePostR", square, new Vector3(cx + 0.8f, -0.3f, 0.2f),
            new Vector3(0.35f, 1.8f, 1f), new Color(0.28f, 0.28f, 0.3f, 0.95f), 4);
        Prop(root, "IvyL", disc, new Vector3(cx - 1.3f, 0.6f, 0.22f),
            new Vector3(0.7f, 1.4f, 1f), new Color(0.18f, 0.4f, 0.2f, 0.7f), 5);
        Prop(root, "IvyR", disc, new Vector3(cx + 0.9f, 0.5f, 0.22f),
            new Vector3(0.65f, 1.3f, 1f), new Color(0.16f, 0.38f, 0.18f, 0.7f), 5);
        Prop(root, "RainWindow", square, new Vector3(cx - 2.2f, 1.2f, 0.08f),
            new Vector3(1.2f, 1.6f, 1f), new Color(0.3f, 0.4f, 0.55f, 0.25f), 1);
        Light(root.Find("RainWindow") ?? root, LockeKeyUITheme.LKMoon, 0.4f, 3f);
        Prop(root, "Planter", square, new Vector3(cx + 1.5f, -1.15f, 0.18f),
            new Vector3(0.5f, 0.4f, 1f), new Color(0.35f, 0.22f, 0.15f, 0.9f), 4);
        Prop(root, "Bush", disc, new Vector3(cx + 1.5f, -0.7f, 0.2f),
            new Vector3(0.7f, 0.55f, 1f), new Color(0.15f, 0.35f, 0.18f, 0.85f), 5);
        // Rain streaks (animated)
        for (var i = 0; i < 10; i++)
        {
            var drop = Prop(root, $"Rain{i}", square,
                new Vector3(cx + Random.Range(-2f, 2f), Random.Range(0.5f, 2.5f), 0.05f),
                new Vector3(0.04f, 0.25f, 1f), new Color(0.7f, 0.8f, 1f, 0.25f), 8);
            drop.AddComponent<RainStreak>().Init(Random.Range(2.5f, 4.5f));
        }
        return root;
    }

    private Transform BuildMemory()
    {
        var root = new GameObject("Env_Memory").transform;
        float cx = 10f;
        Prop(root, "MemRug", disc, new Vector3(cx, -1.48f, 0.12f),
            new Vector3(2.2f, 0.4f, 1f), new Color(0.35f, 0.2f, 0.4f, 0.45f), 2);
        Prop(root, "GrandClock", square, new Vector3(cx - 1.5f, 0.1f, 0.2f),
            new Vector3(0.55f, 2.0f, 1f), new Color(0.28f, 0.18f, 0.1f, 0.95f), 5);
        Prop(root, "ClockFace", disc, new Vector3(cx - 1.5f, 0.7f, 0.21f),
            new Vector3(0.35f, 0.35f, 1f), new Color(0.9f, 0.85f, 0.7f, 0.9f), 6);
        Prop(root, "MemPortraitBig", square, new Vector3(cx + 0.3f, 0.9f, 0.25f),
            new Vector3(1.1f, 1.4f, 1f), new Color(0.25f, 0.18f, 0.3f, 0.95f), 4);
        var frameGlow = Prop(root, "PortraitGlow", disc, new Vector3(cx + 0.3f, 0.9f, 0.24f),
            new Vector3(1.4f, 1.7f, 1f), LockeKeyUITheme.LKMagicPurple, 3);
        var lg = frameGlow.GetComponent<SpriteRenderer>();
        if (lg != null) lg.color = new Color(0.65f, 0.35f, 1f, 0.12f);
        Light(frameGlow.transform, LockeKeyUITheme.LKMagicPurple, 0.55f, 2.5f);

        Prop(root, "Pedestal", square, new Vector3(cx + 1.6f, -0.9f, 0.2f),
            new Vector3(0.45f, 0.7f, 1f), new Color(0.35f, 0.32f, 0.3f, 0.95f), 5);
        var artifact = Prop(root, "MemoryOrb", disc, new Vector3(cx + 1.6f, -0.25f, 0.25f),
            new Vector3(0.35f, 0.35f, 1f), LockeKeyUITheme.LKSpiritGreen, 7);
        Light(artifact.transform, LockeKeyUITheme.LKSpiritGreen, 0.65f, 1.8f);
        artifact.AddComponent<FloatBob>().Init(0.15f, 1.6f);

        Prop(root, "CurtainL", square, new Vector3(cx - 2.2f, 0.7f, 0.18f),
            new Vector3(0.5f, 2.0f, 1f), new Color(0.3f, 0.12f, 0.2f, 0.7f), 4);
        Prop(root, "CurtainR", square, new Vector3(cx + 2.3f, 0.7f, 0.18f),
            new Vector3(0.5f, 2.0f, 1f), new Color(0.28f, 0.12f, 0.22f, 0.7f), 4);
        var cl = root.Find("CurtainL");
        if (cl != null) cl.gameObject.AddComponent<PendulumSway>().Init(3f, 1.5f);
        var cr = root.Find("CurtainR");
        if (cr != null) cr.gameObject.AddComponent<PendulumSway>().Init(2.6f, 1.7f);
        return root;
    }

    private static Color BookColor(int i) => i switch
    {
        0 => new Color(0.5f, 0.18f, 0.15f, 0.95f),
        1 => new Color(0.2f, 0.25f, 0.45f, 0.95f),
        _ => new Color(0.35f, 0.3f, 0.15f, 0.95f)
    };

    private static GameObject Prop(Transform parent, string name, Sprite sprite, Vector3 pos, Vector3 scale, Color color, int order)
    {
        var go = new GameObject(name, typeof(SpriteRenderer));
        go.transform.SetParent(parent);
        go.transform.position = pos;
        go.transform.localScale = scale;
        var sr = go.GetComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        sr.sortingOrder = order;
        return go;
    }

    private static void Light(Transform parent, Color color, float intensity, float radius)
    {
        if (parent == null) return;
        var go = new GameObject("Light");
        go.transform.SetParent(parent, false);
        var l = go.AddComponent<Light2D>();
        l.lightType = Light2D.LightType.Point;
        l.color = color;
        l.intensity = intensity;
        l.pointLightOuterRadius = radius;
    }

    private static Sprite SoftDisc(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var mid = (size - 1) * 0.5f;
        for (var y = 0; y < size; y++)
        for (var x = 0; x < size; x++)
        {
            var d = Vector2.Distance(new Vector2(x, y), new Vector2(mid, mid)) / mid;
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Pow(Mathf.Clamp01(1f - d), 1.4f)));
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    private static Sprite SoftSquare(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        for (var y = 0; y < size; y++)
        for (var x = 0; x < size; x++)
        {
            float edge = Mathf.Min(x, y, size - 1 - x, size - 1 - y) / (size * 0.5f);
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(edge * 2.3f)));
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    public class FloatBob : MonoBehaviour
    {
        private float amp, speed, phase;
        private Vector3 origin;
        public void Init(float a, float s) { amp = a; speed = s; phase = Random.value * 10f; origin = transform.position; }
        private void Update() =>
            transform.position = origin + Vector3.up * (Mathf.Sin(Time.time * speed + phase) * amp);
    }

    public class PendulumSway : MonoBehaviour
    {
        private float amp, speed, phase;
        private Quaternion baseRot;
        public void Init(float a, float s) { amp = a; speed = s; phase = Random.value * 5f; baseRot = transform.rotation; }
        private void Update() =>
            transform.rotation = baseRot * Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * speed + phase) * amp);
    }

    public class RainStreak : MonoBehaviour
    {
        private float speed;
        private float top = 2.8f, bot = -1.2f;
        public void Init(float s) => speed = s;
        private void Update()
        {
            var p = transform.position;
            p.y -= speed * Time.deltaTime;
            if (p.y < bot) p.y = top;
            transform.position = p;
        }
    }
}
