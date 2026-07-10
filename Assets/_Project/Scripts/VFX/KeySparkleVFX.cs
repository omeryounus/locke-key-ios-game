using UnityEngine;

/// <summary>
/// Gold sparkle burst for key pickup immersion.
/// </summary>
public class KeySparkleVFX : MonoBehaviour
{
    public static void Play(Vector3 worldPos)
    {
        var go = new GameObject("KeySparkleBurst");
        go.transform.position = worldPos;
        var host = go.AddComponent<KeySparkleVFX>();
        host.Run();
    }

    private void Run()
    {
        var disc = SoftDisc(16);
        for (var i = 0; i < 14; i++)
        {
            var p = new GameObject($"S{i}", typeof(SpriteRenderer));
            p.transform.SetParent(transform);
            p.transform.localPosition = Vector3.zero;
            var sr = p.GetComponent<SpriteRenderer>();
            sr.sprite = disc;
            sr.sortingOrder = 60;
            // Gold → purple accent mix
            bool gold = i % 3 != 0;
            sr.color = gold
                ? new Color(1f, 0.85f, 0.3f, 0.95f)
                : new Color(0.75f, 0.45f, 1f, 0.9f);
            p.transform.localScale = Vector3.one * Random.Range(0.08f, 0.16f);
            var rb = p.AddComponent<Spark>();
            rb.Init(new Vector2(Random.Range(-1.4f, 1.4f), Random.Range(0.8f, 2.4f)), Random.Range(0.4f, 0.85f));
        }
        Destroy(gameObject, 1.1f);
    }

    private class Spark : MonoBehaviour
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

    private static Sprite SoftDisc(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var mid = (size - 1) * 0.5f;
        for (var y = 0; y < size; y++)
        for (var x = 0; x < size; x++)
        {
            var d = Vector2.Distance(new Vector2(x, y), new Vector2(mid, mid)) / mid;
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(1f - d)));
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
