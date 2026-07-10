using UnityEngine;

/// <summary>
/// Foot-plant dust / scuff / land impact tied to animation contact frames.
/// Soft "IK-like" plant: tiny hip dip + shadow pulse via the character rig.
/// </summary>
[DefaultExecutionOrder(20)]
[RequireComponent(typeof(PlayerController))]
public class PlayerFootContactVFX : MonoBehaviour
{
    [SerializeField] private int dustPool = 10;
    [SerializeField] private float dustLifetime = 1.15f;
    [SerializeField] private float landBurstScale = 1.35f;

    private PlayerController player;
    private PlayerSpriteAnimator spriteAnim;
    private PlayerCharacterRig rig;
    private Sprite dustSprite;
    private Transform[] dust;
    private SpriteRenderer[] dustSr;
    private float[] dustAge;
    private int dustIndex;
    private float plantDip;
    private Vector3 hipBase;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
        spriteAnim = GetComponent<PlayerSpriteAnimator>();
        rig = GetComponent<PlayerCharacterRig>();
        dustSprite = SoftDisc(20);
        BuildPool();

        if (spriteAnim != null)
            spriteAnim.OnAnimEvent += OnAnimEvent;
    }

    private void OnDestroy()
    {
        if (spriteAnim != null)
            spriteAnim.OnAnimEvent -= OnAnimEvent;
    }

    private void BuildPool()
    {
        dust = new Transform[dustPool];
        dustSr = new SpriteRenderer[dustPool];
        dustAge = new float[dustPool];
        var root = new GameObject("FootDustPool").transform;
        root.SetParent(transform, false);
        for (var i = 0; i < dustPool; i++)
        {
            var go = new GameObject($"Dust_{i}", typeof(SpriteRenderer));
            go.transform.SetParent(root, false);
            go.SetActive(false);
            dust[i] = go.transform;
            dustSr[i] = go.GetComponent<SpriteRenderer>();
            dustSr[i].sprite = dustSprite;
            dustSr[i].sortingOrder = 12;
            dustAge[i] = 99f;
        }
    }

    private void OnAnimEvent(PlayerSpriteAnimator.AnimEventKind kind)
    {
        if (kind == PlayerSpriteAnimator.AnimEventKind.FootPlant)
            SpawnDust(run: spriteAnim != null && spriteAnim.State == PlayerSpriteAnimator.AnimState.Run, strength: 1f);
        else if (kind == PlayerSpriteAnimator.AnimEventKind.LandImpact)
            SpawnDust(run: false, strength: landBurstScale);
    }

    private void SpawnDust(bool run, float strength)
    {
        if (player != null && player.IsGhostPhasing) return; // no dust while ethereal

        var i = dustIndex++ % dustPool;
        var t = dust[i];
        t.gameObject.SetActive(true);

        float facing = rig != null ? Mathf.Sign(rig.Facing == 0f ? 1f : rig.Facing) : 1f;
        float side = (dustIndex % 2 == 0 ? -1f : 1f) * 0.08f;
        t.position = transform.position + new Vector3(side * facing, -0.05f, 0.02f);
        float s = (run ? 0.28f : 0.2f) * strength * Random.Range(0.85f, 1.15f);
        t.localScale = new Vector3(s * 1.4f, s * 0.55f, 1f);

        var c = run
            ? new Color(0.55f, 0.45f, 0.35f, 0.45f * strength)
            : new Color(0.5f, 0.48f, 0.45f, 0.4f * strength);
        dustSr[i].color = c;
        dustAge[i] = 0f;

        // Soft plant dip (pseudo foot-IK settle)
        plantDip = Mathf.Max(plantDip, 0.035f * strength);
    }

    private void LateUpdate()
    {
        float dt = Time.deltaTime;

        // Fade dust puffs
        for (var i = 0; i < dustPool; i++)
        {
            if (dustAge[i] > dustLifetime)
            {
                if (dust[i].gameObject.activeSelf)
                    dust[i].gameObject.SetActive(false);
                continue;
            }

            dustAge[i] += dt;
            float t = dustAge[i] / dustLifetime;
            dust[i].position += new Vector3(
                (i % 2 == 0 ? -0.15f : 0.15f) * dt,
                0.12f * dt,
                0f);
            dust[i].localScale *= 1f + dt * 0.8f;
            var c = dustSr[i].color;
            c.a = Mathf.Lerp(c.a, 0f, t);
            dustSr[i].color = c;
        }

        // Hip plant settle
        plantDip = Mathf.MoveTowards(plantDip, 0f, dt * 0.45f);
        if (rig != null && rig.Hip != null)
        {
            if (hipBase == Vector3.zero)
                hipBase = rig.Hip.localPosition;
            rig.Hip.localPosition = hipBase + Vector3.down * plantDip;
        }

        // Wall-slide scuff streak
        if (player != null && player.IsWallSliding && Random.value < 0.18f)
            SpawnDust(run: false, strength: 0.45f);
    }

    private static Sprite SoftDisc(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var mid = (size - 1) * 0.5f;
        for (var y = 0; y < size; y++)
        for (var x = 0; x < size; x++)
        {
            var d = Vector2.Distance(new Vector2(x, y), new Vector2(mid, mid)) / mid;
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Pow(Mathf.Clamp01(1f - d), 1.6f)));
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
