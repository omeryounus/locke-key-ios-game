using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Makes the foyer feel alive: drifting dust, fog banks, moving moonlight,
/// cloth sway, candle flicker, floating particles, and soft moving shadows.
/// True URP bloom/AO require Volume assets; we approximate with light shafts + grading.
/// </summary>
public class LivingWorldAtmosphere : MonoBehaviour
{
    private Transform dustRoot;
    private Transform fogRoot;
    private Transform clothRoot;
    private Transform particleRoot;
    private Transform shadowRoot;
    private Light2D moonlight;
    private Light2D windowVolA;
    private Light2D windowVolB;
    private Camera cam;
    private Sprite disc;
    private float moonPhase;

    private void Awake()
    {
        cam = Camera.main;
        disc = SoftDisc(32);
        BuildDust(72);
        BuildFog(5);
        BuildCloth();
        BuildParticles(24);
        BuildMovingShadows(4);
        BuildMoonAndVolumes();
        BoostContrast();
    }

    private void Update()
    {
        if (cam == null) cam = Camera.main;
        var center = cam != null ? cam.transform.position : Vector3.zero;

        // Keep effects around the camera
        if (dustRoot != null) dustRoot.position = new Vector3(center.x, center.y, 0f);
        if (fogRoot != null) fogRoot.position = new Vector3(center.x, center.y - 0.4f, 0f);
        if (particleRoot != null) particleRoot.position = new Vector3(center.x, center.y, 0f);

        AnimateMoon();
        AnimateFog();
        AnimateCloth();
        AnimateShadows();
        AnimateCandles();
    }

    private void BoostContrast()
    {
        // Slight backdrop contrast / mystery grade
        var backdrop = GameObject.Find("RoomBackdrop")?.GetComponent<SpriteRenderer>();
        if (backdrop != null)
        {
            var c = backdrop.color;
            // Deeper shadows, warmer mids
            backdrop.color = new Color(
                Mathf.Clamp01(c.r * 1.05f),
                Mathf.Clamp01(c.g * 0.98f),
                Mathf.Clamp01(c.b * 0.92f),
                1f);
        }
    }

    private void BuildMoonAndVolumes()
    {
        moonlight = MakeLight("Moonlight", Light2D.LightType.Point,
            new Color(0.55f, 0.68f, 1f), 0.55f, 11f);
        moonlight.transform.position = new Vector3(-3.5f, 3.2f, 0f);

        windowVolA = MakeLight("WindowVolumeA", Light2D.LightType.Point,
            new Color(0.75f, 0.55f, 1f), 0.5f, 4.5f);
        windowVolA.transform.position = new Vector3(-4.2f, 1.5f, 0f);

        windowVolB = MakeLight("WindowVolumeB", Light2D.LightType.Point,
            new Color(0.4f, 0.6f, 1f), 0.4f, 4f);
        windowVolB.transform.position = new Vector3(4.1f, 1.4f, 0f);

        // Soft volumetric shafts (sprite planes)
        MakeShaft("VolShaftL", new Vector3(-4.0f, 1.2f, 0.06f), new Vector3(1.8f, 2.8f, 1f),
            new Color(0.75f, 0.5f, 1f, 0.1f), -18f);
        MakeShaft("VolShaftR", new Vector3(3.9f, 1.1f, 0.06f), new Vector3(1.6f, 2.5f, 1f),
            new Color(0.45f, 0.65f, 1f, 0.08f), 14f);
        MakeShaft("MoonShaft", new Vector3(-2.5f, 1.8f, 0.05f), new Vector3(2.2f, 3.2f, 1f),
            new Color(0.6f, 0.75f, 1f, 0.07f), -8f);
    }

    private void AnimateMoon()
    {
        moonPhase += Time.deltaTime * 0.15f;
        if (moonlight != null)
        {
            moonlight.intensity = 0.45f + Mathf.Sin(moonPhase) * 0.12f;
            var p = moonlight.transform.position;
            moonlight.transform.position = new Vector3(-3.5f + Mathf.Sin(moonPhase * 0.4f) * 0.4f, p.y, 0f);
        }

        if (windowVolA != null)
            windowVolA.intensity = 0.4f + Mathf.Sin(Time.time * 0.7f) * 0.12f;
        if (windowVolB != null)
            windowVolB.intensity = 0.35f + Mathf.Sin(Time.time * 0.55f + 1f) * 0.1f;
    }

    private void BuildDust(int count)
    {
        dustRoot = new GameObject("LivingDust").transform;
        dustRoot.SetParent(transform);
        for (var i = 0; i < count; i++)
        {
            var go = new GameObject($"Dust_{i}", typeof(SpriteRenderer));
            go.transform.SetParent(dustRoot);
            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = disc;
            sr.sortingOrder = 9;
            bool warm = i % 3 == 0;
            sr.color = warm
                ? new Color(1f, 0.92f, 0.7f, Random.Range(0.1f, 0.28f))
                : new Color(0.85f, 0.9f, 1f, Random.Range(0.08f, 0.22f));
            go.transform.localScale = Vector3.one * Random.Range(0.04f, 0.13f);
            go.transform.localPosition = new Vector3(Random.Range(-5f, 5f), Random.Range(-2.5f, 3f), 0f);
            go.AddComponent<DriftMote>().Init(
                new Vector2(Random.Range(-0.15f, 0.15f), Random.Range(0.05f, 0.22f)),
                Random.Range(0.4f, 1.4f));
        }
    }

    private void BuildFog(int banks)
    {
        fogRoot = new GameObject("LivingFog").transform;
        fogRoot.SetParent(transform);
        for (var i = 0; i < banks; i++)
        {
            var go = new GameObject($"Fog_{i}", typeof(SpriteRenderer));
            go.transform.SetParent(fogRoot);
            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = disc;
            sr.sortingOrder = 2;
            sr.color = new Color(0.55f, 0.58f, 0.7f, Random.Range(0.06f, 0.12f));
            go.transform.localScale = new Vector3(Random.Range(3.5f, 6f), Random.Range(0.8f, 1.6f), 1f);
            go.transform.localPosition = new Vector3(Random.Range(-4f, 4f), Random.Range(-1.2f, 0.6f), 0f);
            go.AddComponent<DriftMote>().Init(
                new Vector2(Random.Range(0.04f, 0.12f) * (i % 2 == 0 ? 1f : -1f), Random.Range(-0.02f, 0.02f)),
                Random.Range(1.5f, 3f));
        }
    }

    private void BuildCloth()
    {
        clothRoot = new GameObject("LivingCloth").transform;
        clothRoot.SetParent(transform);
        // Curtains / tapestries near sides
        MakeCloth("CurtainL", new Vector3(-4.6f, 0.8f, 0.2f), new Vector3(0.55f, 2.2f, 1f),
            new Color(0.35f, 0.12f, 0.15f, 0.75f));
        MakeCloth("CurtainR", new Vector3(4.5f, 0.75f, 0.2f), new Vector3(0.5f, 2.1f, 1f),
            new Color(0.28f, 0.12f, 0.2f, 0.72f));
        MakeCloth("Banner", new Vector3(0.2f, 1.9f, 0.18f), new Vector3(0.7f, 1.1f, 1f),
            new Color(0.25f, 0.18f, 0.12f, 0.55f));
    }

    private void MakeCloth(string name, Vector3 pos, Vector3 scale, Color color)
    {
        var go = new GameObject(name, typeof(SpriteRenderer));
        go.transform.SetParent(clothRoot);
        go.transform.position = pos;
        go.transform.localScale = scale;
        var sr = go.GetComponent<SpriteRenderer>();
        sr.sprite = SoftSquare(24);
        sr.color = color;
        sr.sortingOrder = 4;
        go.AddComponent<ClothSway>().Init(Random.Range(1.2f, 2.2f), Random.Range(2f, 5f));
    }

    private void AnimateCloth() { /* ClothSway self-updates */ }

    private void BuildParticles(int count)
    {
        particleRoot = new GameObject("LivingEmbers").transform;
        particleRoot.SetParent(transform);
        for (var i = 0; i < count; i++)
        {
            var go = new GameObject($"Ember_{i}", typeof(SpriteRenderer));
            go.transform.SetParent(particleRoot);
            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = disc;
            sr.sortingOrder = 11;
            sr.color = new Color(1f, 0.75f, 0.35f, Random.Range(0.15f, 0.4f));
            go.transform.localScale = Vector3.one * Random.Range(0.03f, 0.08f);
            go.transform.localPosition = new Vector3(Random.Range(-4f, 4f), Random.Range(-1f, 2f), 0f);
            go.AddComponent<DriftMote>().Init(
                new Vector2(Random.Range(-0.08f, 0.08f), Random.Range(0.12f, 0.35f)),
                Random.Range(0.3f, 0.9f));
        }
    }

    private void BuildMovingShadows(int count)
    {
        shadowRoot = new GameObject("LivingShadows").transform;
        shadowRoot.SetParent(transform);
        for (var i = 0; i < count; i++)
        {
            var go = new GameObject($"Shadow_{i}", typeof(SpriteRenderer));
            go.transform.SetParent(shadowRoot);
            go.transform.position = new Vector3(-3f + i * 2.2f, -0.2f, 0.1f);
            go.transform.localScale = new Vector3(1.8f + i * 0.2f, 2.5f, 1f);
            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = disc;
            sr.color = new Color(0f, 0f, 0f, 0.08f + i * 0.01f);
            sr.sortingOrder = 3;
            go.AddComponent<ClothSway>().Init(0.6f + i * 0.15f, 1.5f + i * 0.3f);
        }
    }

    private void AnimateFog() { /* DriftMote handles */ }
    private void AnimateShadows() { /* ClothSway handles */ }

    private void AnimateCandles()
    {
        // Flicker any Light2D under FoyerEnvironment named *Flame* parent lights
        var env = GameObject.Find("FoyerEnvironment");
        if (env == null) return;
        var lights = env.GetComponentsInChildren<Light2D>();
        foreach (var l in lights)
        {
            if (l.lightType != Light2D.LightType.Point) continue;
            if (l.pointLightOuterRadius > 3.5f) continue; // skip big room lights
            float n = Mathf.PerlinNoise(Time.time * 4.2f + l.GetInstanceID() * 0.01f, 0.3f);
            // Store base via intensity oscillation around current
            l.intensity = Mathf.Lerp(l.intensity, 0.45f + n * 0.55f, Time.deltaTime * 8f);
        }
    }

    private void MakeShaft(string name, Vector3 pos, Vector3 scale, Color color, float zRot)
    {
        if (GameObject.Find(name) != null) return;
        var go = new GameObject(name, typeof(SpriteRenderer));
        go.transform.position = pos;
        go.transform.localScale = scale;
        go.transform.rotation = Quaternion.Euler(0f, 0f, zRot);
        var sr = go.GetComponent<SpriteRenderer>();
        sr.sprite = disc;
        sr.color = color;
        sr.sortingOrder = 1;
        go.AddComponent<DriftMote>().Init(new Vector2(0.01f, 0f), 0.5f);
    }

    private static Light2D MakeLight(string name, Light2D.LightType type, Color color, float intensity, float radius)
    {
        var go = GameObject.Find(name) ?? new GameObject(name);
        var light = go.GetComponent<Light2D>() ?? go.AddComponent<Light2D>();
        light.lightType = type;
        light.color = color;
        light.intensity = intensity;
        light.pointLightOuterRadius = radius;
        light.pointLightInnerRadius = 0.2f;
        return light;
    }

    private static Sprite SoftDisc(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var mid = (size - 1) * 0.5f;
        for (var y = 0; y < size; y++)
        for (var x = 0; x < size; x++)
        {
            var d = Vector2.Distance(new Vector2(x, y), new Vector2(mid, mid)) / mid;
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Pow(Mathf.Clamp01(1f - d), 1.5f)));
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
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(edge * 2.2f)));
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    private class DriftMote : MonoBehaviour
    {
        private Vector2 vel;
        private float wander;
        private Vector3 origin;

        public void Init(Vector2 v, float w)
        {
            vel = v;
            wander = w;
            origin = transform.localPosition;
        }

        private void Update()
        {
            var p = transform.localPosition;
            p += (Vector3)(vel * Time.deltaTime);
            p.x += Mathf.Sin(Time.time * wander + origin.x) * 0.01f;
            // Wrap
            if (p.y > 3.5f) p.y = -2.8f;
            if (p.y < -3f) p.y = 3.2f;
            if (p.x > 6f) p.x = -6f;
            if (p.x < -6f) p.x = 6f;
            transform.localPosition = p;
        }
    }

    private class ClothSway : MonoBehaviour
    {
        private float speed;
        private float amount;
        private Vector3 baseScale;
        private Quaternion baseRot;

        public void Init(float s, float a)
        {
            speed = s;
            amount = a;
            baseScale = transform.localScale;
            baseRot = transform.rotation;
        }

        private void Update()
        {
            float t = Time.time * speed;
            transform.rotation = baseRot * Quaternion.Euler(0f, 0f, Mathf.Sin(t) * amount);
            transform.localScale = new Vector3(
                baseScale.x * (1f + Mathf.Sin(t * 0.7f) * 0.03f),
                baseScale.y * (1f + Mathf.Sin(t) * 0.04f),
                1f);
        }
    }
}
