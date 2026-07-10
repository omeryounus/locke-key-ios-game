using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Top-level motion helpers (Unity requires MonoBehaviours at top level for AddComponent).
/// </summary>
public class EnvFloatBob : MonoBehaviour
{
    private float amp, speed, phase;
    private Vector3 origin;

    public void Init(float a, float s)
    {
        amp = a;
        speed = s;
        phase = Random.value * 10f;
        origin = transform.position;
    }

    private void Update() =>
        transform.position = origin + Vector3.up * (Mathf.Sin(Time.time * speed + phase) * amp);
}

public class EnvPendulumSway : MonoBehaviour
{
    private float amp, speed, phase;
    private Quaternion baseRot;

    public void Init(float a, float s)
    {
        amp = a;
        speed = s;
        phase = Random.value * 5f;
        baseRot = transform.rotation;
    }

    private void Update() =>
        transform.rotation = baseRot * Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * speed + phase) * amp);
}

public class EnvRainStreak : MonoBehaviour
{
    private float speed;
    private float top = 2.8f;
    private float bot = -1.2f;

    public void Init(float s) => speed = s;

    private void Update()
    {
        var p = transform.position;
        p.y -= speed * Time.deltaTime;
        if (p.y < bot) p.y = top;
        transform.position = p;
    }
}

public class EnvFlickerLight : MonoBehaviour
{
    private Light2D light2d;
    private float baseI = 0.7f;

    private void Start()
    {
        light2d = GetComponentInChildren<Light2D>();
        if (light2d != null) baseI = light2d.intensity;
    }

    private void Update()
    {
        if (light2d == null) return;
        float n = Mathf.PerlinNoise(Time.time * 3.5f, 0.2f);
        light2d.intensity = baseI * (0.85f + n * 0.3f);
    }
}

public class EnvDriftMote : MonoBehaviour
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
        if (p.y > 3.5f) p.y = -2.8f;
        if (p.y < -3f) p.y = 3.2f;
        if (p.x > 6f) p.x = -6f;
        if (p.x < -6f) p.x = 6f;
        transform.localPosition = p;
    }
}

public class EnvClothSway : MonoBehaviour
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

public class EnvDustMote : MonoBehaviour
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
            Mathf.Sin(phase) * drift * 0.55f,
            0f);
        if (transform.localPosition.y > 3.4f)
            baseLocal.y = -2.9f;
    }
}

public class EnvDebrisDrift : MonoBehaviour
{
    private Vector2 velocity;
    private float life;
    private float age;
    private SpriteRenderer sr;

    public void Init(Vector2 vel, float lifetime)
    {
        velocity = vel;
        life = lifetime;
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        age += Time.deltaTime;
        velocity.y -= 5f * Time.deltaTime;
        transform.position += (Vector3)(velocity * Time.deltaTime);
        transform.Rotate(0f, 0f, velocity.x * 50f * Time.deltaTime);
        if (sr != null)
        {
            var c = sr.color;
            c.a = Mathf.Lerp(0.9f, 0f, age / life);
            sr.color = c;
        }

        if (age >= life)
            Destroy(gameObject);
    }
}

public class EnvSpark : MonoBehaviour
{
    private Vector2 vel;
    private float life;
    private float age;
    private SpriteRenderer sr;

    public void Init(Vector2 v, float l)
    {
        vel = v;
        life = l;
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        age += Time.deltaTime;
        vel.y -= 3.2f * Time.deltaTime;
        transform.position += (Vector3)(vel * Time.deltaTime);
        if (sr != null)
        {
            var c = sr.color;
            c.a = Mathf.Lerp(c.a, 0f, age / life);
            sr.color = c;
        }
        if (age >= life) Destroy(gameObject);
    }
}
