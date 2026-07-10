using UnityEngine;

/// <summary>
/// Extra life for the player: blink flashes, stronger breathing, cloak flutter.
/// Works with PlayerSpriteAnimator + PlayerVisibilityBoost.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerIdleDetail : MonoBehaviour
{
    private SpriteRenderer body;
    private SpriteRenderer blink;
    private SpriteRenderer cloak;
    private PlayerController player;
    private float blinkTimer;
    private float blinkOpen = 2.5f;
    private bool eyesClosed;
    private float closeTimer;
    private Color baseColor;

    private void Awake()
    {
        body = GetComponent<SpriteRenderer>();
        player = GetComponent<PlayerController>();
        baseColor = body != null ? body.color : Color.white;

        // Blink lid overlay
        var b = transform.Find("BlinkLid");
        if (b == null)
        {
            var go = new GameObject("BlinkLid");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            go.transform.localScale = new Vector3(0.35f, 0.08f, 1f);
            blink = go.AddComponent<SpriteRenderer>();
            blink.sprite = SoftDisc(16);
            blink.color = new Color(0.08f, 0.07f, 0.1f, 0f);
            blink.sortingOrder = (body != null ? body.sortingOrder : 20) + 3;
        }
        else blink = b.GetComponent<SpriteRenderer>();

        // Extra cloak layer — prefer production hood/cape art
        var c = transform.Find("CloakFlutter");
        if (c == null)
        {
            var go = new GameObject("CloakFlutter");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, 0.08f, 0f);
            go.transform.localScale = new Vector3(0.85f, 0.95f, 1f);
            cloak = go.AddComponent<SpriteRenderer>();
            var hoodSpr = Resources.Load<Sprite>("Art/Characters/Layers/player_hood_cape");
            if (hoodSpr != null)
            {
                cloak.sprite = hoodSpr;
                cloak.color = new Color(1f, 1f, 1f, 0.55f);
            }
            else
            {
                cloak.sprite = SoftDisc(24);
                cloak.color = new Color(0.18f, 0.12f, 0.28f, 0.28f);
            }
            cloak.sortingOrder = (body != null ? body.sortingOrder : 20) - 1;
        }
        else cloak = c.GetComponent<SpriteRenderer>();

        blinkTimer = Random.Range(1.5f, 3.5f);
    }

    private void Update()
    {
        float speed = player != null ? player.HorizontalSpeed : 0f;
        bool idle = speed < 0.15f && (player == null || player.IsGrounded);

        // Breathing scale on body (stacks with animator)
        if (body != null && idle)
        {
            float breath = 1f + Mathf.Sin(Time.time * 1.9f) * 0.018f;
            // Don't fight flip; only nudge Y via child-like feel — apply subtle alpha pulse
            var c = body.color;
            float aBoost = 0.02f * Mathf.Sin(Time.time * 1.9f);
            body.color = new Color(c.r, c.g, c.b, Mathf.Clamp01(0.98f + aBoost));
        }

        // Blink
        if (!eyesClosed)
        {
            blinkTimer -= Time.deltaTime;
            if (blinkTimer <= 0f && idle)
            {
                eyesClosed = true;
                closeTimer = 0.09f;
                if (blink != null) blink.color = new Color(0.08f, 0.07f, 0.1f, 0.85f);
            }
        }
        else
        {
            closeTimer -= Time.deltaTime;
            if (closeTimer <= 0f)
            {
                eyesClosed = false;
                blinkTimer = Random.Range(2f, 4.5f);
                if (blink != null) blink.color = new Color(0.08f, 0.07f, 0.1f, 0f);
            }
        }

        // Cloak flutter
        if (cloak != null)
        {
            float t = Time.time;
            float flutter = idle ? 1f : 1.6f;
            cloak.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 2.1f * flutter) * 6f);
            cloak.transform.localPosition = new Vector3(
                Mathf.Sin(t * 1.7f) * 0.03f - (player != null && body != null && body.flipX ? 0.04f : -0.04f),
                0.05f + Mathf.Sin(t * 2.4f) * 0.02f,
                0f);
            float a = idle ? 0.28f : 0.34f;
            cloak.color = new Color(0.2f, 0.12f, 0.32f, a + Mathf.Sin(t * 3f) * 0.04f);
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
