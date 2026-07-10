using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Brightens the scene while keeping a spooky warm/cool split: readable OLED floors,
/// dusty air, candle pools, and soft light shafts — not a washed-out flat look.
/// </summary>
public class SceneAtmosphereController : MonoBehaviour
{
    [SerializeField] private int dustCount = 64;

    private Light2D globalFill;
    private Light2D warmKey;
    private Light2D coolRim;
    private Light2D doorPool;
    private Transform dustRoot;
    private Sprite dustSprite;
    private Camera cam;
    private float appliedBrightness = 1f;
    private GameObject shaftA;
    private GameObject shaftB;

    private void Awake()
    {
        cam = Camera.main;
        EnsureLights();
        EnsureDust();
        EnsureLightShafts();
        ApplyBrightness(GameSettings.Brightness);
    }

    private void Update()
    {
        var b = GameSettings.Brightness;
        if (!Mathf.Approximately(b, appliedBrightness))
            ApplyBrightness(b);

        AnimateDust();
        AnimateShafts();
    }

    public void ApplyBrightness(float brightness)
    {
        appliedBrightness = brightness;

        // Spooky but readable: fill never pure white; warm key carries mood.
        if (globalFill != null)
            globalFill.intensity = 0.72f * brightness;
        if (warmKey != null)
            warmKey.intensity = 1.05f * brightness;
        if (coolRim != null)
            coolRim.intensity = 0.42f * brightness;
        if (doorPool != null)
            doorPool.intensity = 0.65f * brightness;

        if (cam != null)
        {
            // Deep ink edges, lifted mid so sprites separate from walls.
            cam.backgroundColor = Color.Lerp(
                new Color(0.06f, 0.05f, 0.08f),
                new Color(0.14f, 0.12f, 0.15f),
                Mathf.InverseLerp(0.7f, 1.5f, brightness));
        }

        var backdrop = GameObject.Find("RoomBackdrop")?.GetComponent<SpriteRenderer>();
        if (backdrop != null)
        {
            // Higher contrast grade: lifted brights, cooler deeps, warm midtones.
            var lift = Mathf.Lerp(1.05f, 1.32f, Mathf.InverseLerp(0.7f, 1.5f, brightness));
            backdrop.color = new Color(lift * 1.04f, lift * 0.97f, lift * 0.9f, 1f);
        }
    }

    private void EnsureLights()
    {
        globalFill = FindOrCreateLight("AmbientFill", Light2D.LightType.Global,
            new Color(0.88f, 0.86f, 0.95f), 0.8f);

        warmKey = FindOrCreateLight("WarmKeyLight", Light2D.LightType.Point,
            LockeKeyUITheme.LKCandle, 1.25f);
        warmKey.pointLightOuterRadius = 10f;
        warmKey.pointLightInnerRadius = 1.4f;
        warmKey.transform.position = new Vector3(0.4f, 1.9f, 0f);

        coolRim = FindOrCreateLight("CoolRimLight", Light2D.LightType.Point,
            LockeKeyUITheme.LKMoon, 0.55f);
        coolRim.pointLightOuterRadius = 8f;
        coolRim.transform.position = new Vector3(-2.8f, 2.4f, 0f);

        doorPool = FindOrCreateLight("DoorPoolLight", Light2D.LightType.Point,
            new Color(1f, 0.75f, 0.3f), 0.85f);
        doorPool.pointLightOuterRadius = 3.2f;
        doorPool.pointLightInnerRadius = 0.3f;
        doorPool.transform.position = new Vector3(-0.2f, 0.4f, 0f);
    }

    private void EnsureLightShafts()
    {
        if (GameObject.Find("LightShaftA") != null) return;
        shaftA = CreateShaft("LightShaftA", new Vector3(-3.0f, 1.5f, 0.08f), new Vector3(1.6f, 2.6f, 1f),
            new Color(1f, 0.9f, 0.6f, 0.1f));
        shaftB = CreateShaft("LightShaftB", new Vector3(1.1f, 1.7f, 0.08f), new Vector3(1.2f, 2.2f, 1f),
            new Color(0.65f, 0.78f, 1f, 0.07f));
    }

    private static GameObject CreateShaft(string name, Vector3 pos, Vector3 scale, Color color)
    {
        var go = new GameObject(name, typeof(SpriteRenderer));
        go.transform.position = pos;
        go.transform.localScale = scale;
        go.transform.rotation = Quaternion.Euler(0f, 0f, -12f);
        var sr = go.GetComponent<SpriteRenderer>();
        sr.sprite = CreateSoftDisc(48);
        sr.color = color;
        sr.sortingOrder = 1;
        return go;
    }

    private static Light2D FindOrCreateLight(string name, Light2D.LightType type, Color color, float intensity)
    {
        var go = GameObject.Find(name);
        if (go == null)
            go = new GameObject(name);
        var light = go.GetComponent<Light2D>();
        if (light == null)
            light = go.AddComponent<Light2D>();
        light.lightType = type;
        light.color = color;
        light.intensity = intensity;
        return light;
    }

    private void EnsureDust()
    {
        if (dustRoot != null) return;
        dustRoot = new GameObject("DustMotes").transform;
        dustRoot.SetParent(transform);
        dustSprite = CreateSoftDisc(24);

        for (var i = 0; i < dustCount; i++)
        {
            var mote = new GameObject($"Dust_{i}", typeof(SpriteRenderer));
            mote.transform.SetParent(dustRoot);
            var sr = mote.GetComponent<SpriteRenderer>();
            sr.sprite = dustSprite;
            sr.sortingOrder = 8;
            // Warm dust in shafts, cooler motes elsewhere
            bool warm = i % 3 == 0;
            sr.color = warm
                ? new Color(1f, 0.92f, 0.7f, Random.Range(0.1f, 0.28f))
                : new Color(0.85f, 0.9f, 1f, Random.Range(0.08f, 0.2f));
            float size = Random.Range(0.04f, 0.12f);
            mote.transform.localScale = Vector3.one * size;
            mote.transform.position = RandomDustPos();
            mote.AddComponent<EnvDustMote>().Init(Random.Range(0.12f, 0.4f), Random.Range(0.35f, 1.2f));
        }
    }

    private void AnimateDust()
    {
        if (cam == null || dustRoot == null) return;
        var center = cam.transform.position;
        dustRoot.position = new Vector3(center.x, center.y, 0f);
    }

    private void AnimateShafts()
    {
        float pulse = 0.85f + Mathf.Sin(Time.time * 0.7f) * 0.15f;
        if (shaftA != null)
        {
            var sr = shaftA.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                var c = sr.color;
                sr.color = new Color(c.r, c.g, c.b, 0.08f * pulse * appliedBrightness);
            }
        }

        if (shaftB != null)
        {
            var sr = shaftB.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                var c = sr.color;
                sr.color = new Color(c.r, c.g, c.b, 0.06f * pulse * appliedBrightness);
            }
        }
    }

    private Vector3 RandomDustPos()
    {
        var c = cam != null ? cam.transform.position : Vector3.zero;
        return new Vector3(c.x + Random.Range(-4.5f, 4.5f), c.y + Random.Range(-2.6f, 3f), 0f);
    }

    private static Sprite CreateSoftDisc(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var mid = (size - 1) * 0.5f;
        for (var y = 0; y < size; y++)
        for (var x = 0; x < size; x++)
        {
            var d = Vector2.Distance(new Vector2(x, y), new Vector2(mid, mid)) / mid;
            var a = Mathf.Pow(Mathf.Clamp01(1f - d), 1.5f);
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
        }

        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
