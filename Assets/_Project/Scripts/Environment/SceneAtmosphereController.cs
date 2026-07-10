using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Brightens the scene, adds ambient fill lights, dust motes, and soft fog tint
/// so the foyer reads clearly on mobile OLED screens.
/// </summary>
public class SceneAtmosphereController : MonoBehaviour
{
    [SerializeField] private float baseBrightness = 1.22f;
    [SerializeField] private int dustCount = 48;

    private Light2D globalFill;
    private Light2D warmKey;
    private Light2D coolRim;
    private Transform dustRoot;
    private Sprite dustSprite;
    private Camera cam;
    private float appliedBrightness = 1f;

    private void Awake()
    {
        cam = Camera.main;
        EnsureLights();
        EnsureDust();
        ApplyBrightness(GameSettings.Brightness);
    }

    private void Update()
    {
        var b = GameSettings.Brightness;
        if (!Mathf.Approximately(b, appliedBrightness))
            ApplyBrightness(b);

        AnimateDust();
    }

    public void ApplyBrightness(float brightness)
    {
        appliedBrightness = brightness;
        if (globalFill != null)
            globalFill.intensity = 0.55f * brightness;
        if (warmKey != null)
            warmKey.intensity = 0.85f * brightness;
        if (coolRim != null)
            coolRim.intensity = 0.35f * brightness;

        // Lift ambient backdrop slightly so dark art still reads.
        if (cam != null)
            cam.backgroundColor = Color.Lerp(new Color(0.04f, 0.05f, 0.08f), new Color(0.12f, 0.11f, 0.14f), (brightness - 0.7f) / 0.9f);

        var backdrop = GameObject.Find("RoomBackdrop")?.GetComponent<SpriteRenderer>();
        if (backdrop != null)
        {
            var lift = Mathf.Lerp(0.92f, 1.18f, (brightness - 0.7f) / 0.9f);
            backdrop.color = new Color(lift, lift, lift, 1f);
        }
    }

    private void EnsureLights()
    {
        globalFill = FindOrCreateLight("AmbientFill", Light2D.LightType.Global,
            new Color(0.92f, 0.9f, 0.95f), 0.65f);

        warmKey = FindOrCreateLight("WarmKeyLight", Light2D.LightType.Point,
            new Color(1f, 0.78f, 0.48f), 0.9f);
        warmKey.pointLightOuterRadius = 9f;
        warmKey.pointLightInnerRadius = 1.2f;
        warmKey.transform.position = new Vector3(0.5f, 1.8f, 0f);

        coolRim = FindOrCreateLight("CoolRimLight", Light2D.LightType.Point,
            new Color(0.45f, 0.62f, 0.95f), 0.4f);
        coolRim.pointLightOuterRadius = 7f;
        coolRim.transform.position = new Vector3(-2.5f, 2.2f, 0f);
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
        dustRoot = new GameObject("DustMotes").transform;
        dustRoot.SetParent(transform);
        dustSprite = CreateSoftCircleSprite(24);

        for (var i = 0; i < dustCount; i++)
        {
            var mote = new GameObject($"Dust_{i}", typeof(SpriteRenderer));
            mote.transform.SetParent(dustRoot);
            var sr = mote.GetComponent<SpriteRenderer>();
            sr.sprite = dustSprite;
            sr.sortingOrder = 8;
            sr.color = new Color(1f, 0.95f, 0.8f, Random.Range(0.08f, 0.22f));
            float size = Random.Range(0.04f, 0.11f);
            mote.transform.localScale = Vector3.one * size;
            mote.transform.position = RandomDustPos();
            mote.AddComponent<DustMote>().Init(Random.Range(0.15f, 0.45f), Random.Range(0.4f, 1.1f));
        }
    }

    private void AnimateDust()
    {
        if (cam == null || dustRoot == null) return;
        // Keep dust cloud around the camera so the foyer always feels alive.
        var center = cam.transform.position;
        dustRoot.position = new Vector3(center.x, center.y, 0f);
    }

    private Vector3 RandomDustPos()
    {
        var c = cam != null ? cam.transform.position : Vector3.zero;
        return new Vector3(c.x + Random.Range(-4f, 4f), c.y + Random.Range(-2.5f, 2.8f), 0f);
    }

    private static Sprite CreateSoftCircleSprite(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var mid = (size - 1) * 0.5f;
        for (var y = 0; y < size; y++)
        for (var x = 0; x < size; x++)
        {
            var d = Vector2.Distance(new Vector2(x, y), new Vector2(mid, mid)) / mid;
            var a = Mathf.Clamp01(1f - d);
            a = a * a;
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
        }

        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    private class DustMote : MonoBehaviour
    {
        private float speed;
        private float phase;
        private Vector3 baseLocal;
        private float drift;

        public void Init(float s, float d)
        {
            speed = s;
            drift = d;
            phase = Random.Range(0f, 10f);
            baseLocal = transform.localPosition;
        }

        private void Update()
        {
            phase += Time.deltaTime * speed;
            transform.localPosition = baseLocal + new Vector3(
                Mathf.Sin(phase * 0.7f) * drift * 0.35f,
                Mathf.Sin(phase) * drift * 0.55f + Time.deltaTime * 0.02f,
                0f);
            // Wrap slowly
            if (transform.localPosition.y > 3.2f)
                baseLocal.y = -2.8f;
        }
    }
}
