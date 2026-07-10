using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 2.5D layered character hierarchy: cape, legs, torso, arms, head, hair, face.
/// Secondary motion (breath, sway, blink, follow-through) runs here so the body
/// never feels like a stiff puppet even between atlas frames.
/// </summary>
[DisallowMultipleComponent]
public class PlayerCharacterRig : MonoBehaviour
{
    public Transform VisualRoot { get; private set; }
    public Transform Cape { get; private set; }
    public Transform Legs { get; private set; }
    public Transform Torso { get; private set; }
    public Transform ArmBack { get; private set; }
    public Transform ArmFront { get; private set; }
    public Transform Head { get; private set; }
    public Transform Hair { get; private set; }
    public Transform Face { get; private set; }
    public Transform Eyes { get; private set; }
    public Transform Shadow { get; private set; }

    public SpriteRenderer BodyRenderer { get; private set; }
    public SpriteRenderer CapeRenderer { get; private set; }
    public SpriteRenderer HeadRenderer { get; private set; }
    public SpriteRenderer HairRenderer { get; private set; }
    public SpriteRenderer FaceRenderer { get; private set; }
    public SpriteRenderer EyesRenderer { get; private set; }
    public SpriteRenderer ArmBackRenderer { get; private set; }
    public SpriteRenderer ArmFrontRenderer { get; private set; }
    public SpriteRenderer LegsRenderer { get; private set; }
    public SpriteRenderer ShadowRenderer { get; private set; }

    private Light2D rimLight;
    private Vector3 baseVisualScale = new(1.55f, 1.55f, 1f);
    private float facing = 1f;
    private float facingVel;
    private float squashY;
    private float leanZ;
    private float breathPhase;
    private float blinkTimer = 2.2f;
    private float blinkClose;
    private bool eyesClosed;
    private float secondaryAmp = 1f;
    private PlayerController player;

    // Pose offsets driven by animator (walk cycle phase 0..1, air stretch, etc.)
    public float CyclePhase { get; set; }
    public float AirStretch { get; set; }
    public float MoveEnergy { get; set; }
    public float EmotionIntensity { get; set; }
    public bool GhostMode { get; set; }

    public float Facing => facing;
    public Vector3 BaseVisualScale => baseVisualScale;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
        // Move authored root scale onto VisualRoot so facing flip does not fight physics scale
        var rs = transform.localScale;
        if (Mathf.Abs(rs.x) > 1.01f || Mathf.Abs(rs.y) > 1.01f)
        {
            baseVisualScale = new Vector3(Mathf.Abs(rs.x), Mathf.Abs(rs.y), 1f);
            transform.localScale = Vector3.one;
        }
        BuildHierarchy();
        LoadLayerSprites();
        EnsureRim();
    }

    private void BuildHierarchy()
    {
        // Root body sprite stays on this GO for physics visuals fallback
        BodyRenderer = GetComponent<SpriteRenderer>();
        if (BodyRenderer == null)
            BodyRenderer = gameObject.AddComponent<SpriteRenderer>();
        BodyRenderer.sortingOrder = 20;

        VisualRoot = EnsureChild("VisualRoot", transform, Vector3.zero);
        Shadow = EnsureChild("FootShadow", VisualRoot, new Vector3(0f, -0.02f, 0.05f));
        Cape = EnsureChild("Cape", VisualRoot, new Vector3(-0.02f, 0.12f, 0f));
        Legs = EnsureChild("Legs", VisualRoot, new Vector3(0f, 0.02f, 0f));
        Torso = EnsureChild("Torso", VisualRoot, new Vector3(0f, 0.28f, 0f));
        ArmBack = EnsureChild("ArmBack", Torso, new Vector3(-0.12f, 0.05f, 0f));
        ArmFront = EnsureChild("ArmFront", Torso, new Vector3(0.14f, 0.02f, 0f));
        Head = EnsureChild("Head", Torso, new Vector3(0.02f, 0.38f, 0f));
        Hair = EnsureChild("Hair", Head, new Vector3(-0.02f, 0.08f, 0f));
        Face = EnsureChild("Face", Head, new Vector3(0.04f, 0.02f, 0f));
        Eyes = EnsureChild("Eyes", Face, new Vector3(0.02f, 0.04f, 0f));

        ShadowRenderer = EnsureSr(Shadow, 5);
        CapeRenderer = EnsureSr(Cape, 18);
        LegsRenderer = EnsureSr(Legs, 19);
        // Body is main atlas frame
        ArmBackRenderer = EnsureSr(ArmBack, 19);
        ArmFrontRenderer = EnsureSr(ArmFront, 22);
        HeadRenderer = EnsureSr(Head, 23);
        HairRenderer = EnsureSr(Hair, 24);
        FaceRenderer = EnsureSr(Face, 25);
        EyesRenderer = EnsureSr(Eyes, 26);

        Shadow.localScale = new Vector3(0.9f, 0.22f, 1f);
        Cape.localScale = new Vector3(0.92f, 0.95f, 1f);
        Legs.localScale = new Vector3(0.55f, 0.45f, 1f);
        ArmBack.localScale = ArmFront.localScale = new Vector3(0.35f, 0.4f, 1f);
        Head.localScale = new Vector3(0.42f, 0.42f, 1f);
        Hair.localScale = new Vector3(0.55f, 0.5f, 1f);
        Face.localScale = new Vector3(0.28f, 0.28f, 1f);
        Eyes.localScale = new Vector3(0.35f, 0.12f, 1f);

        // Default layers semi-hidden until expression/overlay needed; body atlas carries most form
        SetLayerAlpha(CapeRenderer, 0.55f);
        SetLayerAlpha(LegsRenderer, 0f);
        SetLayerAlpha(ArmBackRenderer, 0f);
        SetLayerAlpha(ArmFrontRenderer, 0f);
        SetLayerAlpha(HeadRenderer, 0f);
        SetLayerAlpha(HairRenderer, 0.35f);
        SetLayerAlpha(FaceRenderer, 0f);
        SetLayerAlpha(EyesRenderer, 0f);
    }

    private void LoadLayerSprites()
    {
        var disc = SoftDisc(32);
        ShadowRenderer.sprite = disc;
        ShadowRenderer.color = new Color(0f, 0f, 0f, 0.4f);

        CapeRenderer.sprite = PlayerSpriteAtlas.LoadSingle("Art/Characters/Layers/player_hood_cape")
                              ?? PlayerSpriteAtlas.LoadSingle("Art/Characters/Layers/player_hair_hood")
                              ?? disc;
        HairRenderer.sprite = PlayerSpriteAtlas.LoadSingle("Art/Characters/Layers/player_hair_hood")
                              ?? CapeRenderer.sprite;
        HeadRenderer.sprite = PlayerSpriteAtlas.LoadSingle("Art/Characters/Layers/player_head") ?? disc;
        LegsRenderer.sprite = PlayerSpriteAtlas.LoadSingle("Art/Characters/Layers/player_legs") ?? disc;
        ArmBackRenderer.sprite = PlayerSpriteAtlas.LoadSingle("Art/Characters/Layers/player_arm_l")
                                 ?? PlayerSpriteAtlas.LoadSingle("Art/Characters/Layers/player_arms_reach")
                                 ?? disc;
        ArmFrontRenderer.sprite = PlayerSpriteAtlas.LoadSingle("Art/Characters/Layers/player_arm_r")
                                  ?? ArmBackRenderer.sprite;
        FaceRenderer.sprite = PlayerSpriteAtlas.LoadSingle("Art/Characters/Layers/player_portrait") ?? disc;
        EyesRenderer.sprite = disc;
        EyesRenderer.color = new Color(0.05f, 0.05f, 0.08f, 0f);
    }

    private void EnsureRim()
    {
        var existing = transform.Find("PlayerRimLight");
        if (existing != null)
        {
            rimLight = existing.GetComponent<Light2D>();
            return;
        }

        var go = new GameObject("PlayerRimLight");
        go.transform.SetParent(VisualRoot, false);
        go.transform.localPosition = new Vector3(0.15f, 0.55f, 0f);
        rimLight = go.AddComponent<Light2D>();
        rimLight.lightType = Light2D.LightType.Point;
        rimLight.color = new Color(1f, 0.92f, 0.75f);
        rimLight.intensity = 0.55f;
        rimLight.pointLightOuterRadius = 1.6f;
        rimLight.pointLightInnerRadius = 0.1f;
    }

    private void LateUpdate()
    {
        float dt = Time.deltaTime;
        breathPhase += dt * (1.7f + MoveEnergy * 0.8f);

        // Facing: smooth flip without robotic snap (scale X through zero-ish)
        float desired = facing;
        if (player != null && Mathf.Abs(player.MoveInput) > 0.12f)
            desired = Mathf.Sign(player.MoveInput);
        else if (player != null && Mathf.Abs(player.Velocity.x) > 0.2f)
            desired = Mathf.Sign(player.Velocity.x);
        facing = Mathf.SmoothDamp(facing, desired, ref facingVel, 0.06f, 40f);

        float faceSign = facing >= 0f ? 1f : -1f;
        float faceAmt = Mathf.Clamp01(Mathf.Abs(facing));

        // Body squash from animator
        float breath = Mathf.Sin(breathPhase) * 0.018f * secondaryAmp;
        float breath2 = Mathf.Sin(breathPhase * 0.5f + 0.4f) * 0.008f * secondaryAmp;
        float sy = 1f + squashY + breath + breath2 + AirStretch * 0.08f;
        float sx = 1f - squashY * 0.65f - AirStretch * 0.04f + breath * 0.3f;
        VisualRoot.localScale = new Vector3(
            baseVisualScale.x * sx * faceSign * Mathf.Lerp(0.15f, 1f, faceAmt),
            baseVisualScale.y * sy,
            1f);

        // Lean into movement
        float vx = player != null ? player.Velocity.x : 0f;
        float targetLean = -MoveEnergy * 6.5f * faceSign - vx * 0.6f;
        if (player != null && !player.IsGrounded)
            targetLean += -vx * 0.35f;
        leanZ = Mathf.Lerp(leanZ, targetLean, dt * 8f);
        VisualRoot.localRotation = Quaternion.Euler(0f, 0f, leanZ * 0.35f);

        AnimateSecondary(dt, faceSign);
        AnimateShadow();
        AnimateGhost();
    }

    private void AnimateSecondary(float dt, float faceSign)
    {
        float t = Time.time;
        float energy = 0.5f + MoveEnergy;

        // Cape / hair follow-through
        if (Cape != null)
        {
            float flap = Mathf.Sin(t * 2.4f * energy + CyclePhase * Mathf.PI * 2f) * (5f + MoveEnergy * 10f);
            float drag = -MoveEnergy * 12f * faceSign;
            Cape.localRotation = Quaternion.Euler(0f, 0f, flap + drag);
            Cape.localPosition = new Vector3(
                -0.04f * faceSign + Mathf.Sin(t * 1.8f) * 0.015f,
                0.1f + Mathf.Sin(t * 2.1f) * 0.02f,
                0f);
            if (CapeRenderer != null)
            {
                var c = CapeRenderer.color;
                c.a = GhostMode ? 0.25f : 0.5f + MoveEnergy * 0.1f;
                CapeRenderer.color = c;
            }
        }

        if (Hair != null)
        {
            Hair.localRotation = Quaternion.Euler(0f, 0f,
                Mathf.Sin(t * 2.8f + 1f) * (4f + MoveEnergy * 6f) - MoveEnergy * 8f * faceSign);
        }

        // Arms swing opposite to walk phase
        float swing = Mathf.Sin(CyclePhase * Mathf.PI * 2f) * (12f + MoveEnergy * 16f);
        if (ArmFront != null)
            ArmFront.localRotation = Quaternion.Euler(0f, 0f, swing * 0.15f * secondaryAmp);
        if (ArmBack != null)
            ArmBack.localRotation = Quaternion.Euler(0f, 0f, -swing * 0.12f * secondaryAmp);

        // Torso micro-bob
        if (Torso != null)
        {
            float bob = Mathf.Abs(Mathf.Sin(CyclePhase * Mathf.PI * 2f)) * MoveEnergy * 0.03f;
            Torso.localPosition = new Vector3(0f, 0.28f + bob + Mathf.Sin(breathPhase) * 0.012f, 0f);
            Torso.localRotation = Quaternion.Euler(0f, 0f, swing * 0.04f);
        }

        // Head counter-rotate + emotion tilt
        if (Head != null)
        {
            float headTilt = -swing * 0.05f + EmotionIntensity * 4f;
            Head.localRotation = Quaternion.Euler(0f, 0f, headTilt + Mathf.Sin(t * 1.3f) * 1.5f);
            Head.localPosition = new Vector3(0.02f * faceSign, 0.38f + Mathf.Sin(breathPhase) * 0.01f, 0f);
        }

        // Blink only when mostly idle
        bool canBlink = MoveEnergy < 0.2f && (player == null || player.IsGrounded);
        if (!eyesClosed)
        {
            blinkTimer -= dt;
            if (canBlink && blinkTimer <= 0f)
            {
                eyesClosed = true;
                blinkClose = 0.08f;
                if (EyesRenderer != null)
                    EyesRenderer.color = new Color(0.06f, 0.05f, 0.08f, 0.75f);
            }
        }
        else
        {
            blinkClose -= dt;
            if (blinkClose <= 0f)
            {
                eyesClosed = false;
                blinkTimer = Random.Range(2.0f, 4.5f);
                if (EyesRenderer != null)
                    EyesRenderer.color = new Color(0.06f, 0.05f, 0.08f, 0f);
            }
        }
    }

    private void AnimateShadow()
    {
        if (Shadow == null || ShadowRenderer == null) return;
        bool grounded = player == null || player.IsGrounded;
        float squash = grounded ? 1f : 0.55f;
        float walk = player != null ? Mathf.Clamp01(player.HorizontalSpeed / 5f) : 0f;
        Shadow.localPosition = new Vector3(0f, -0.02f, 0.05f);
        Shadow.localScale = new Vector3(0.85f * squash * (1f + walk * 0.1f), 0.2f * squash, 1f);
        var c = ShadowRenderer.color;
        c.a = grounded ? 0.42f : 0.16f;
        ShadowRenderer.color = c;
    }

    private void AnimateGhost()
    {
        if (rimLight != null)
            rimLight.intensity = GhostMode
                ? 0.9f + Mathf.Sin(Time.time * 8f) * 0.2f
                : 0.5f + Mathf.Sin(Time.time * 2f) * 0.08f;

        if (GhostMode && BodyRenderer != null)
        {
            var c = BodyRenderer.color;
            float a = 0.48f + Mathf.Sin(Time.time * 9f) * 0.12f;
            BodyRenderer.color = new Color(0.4f, 0.95f, 0.7f, a);
        }
    }

    public void SetBodySprite(Sprite sprite)
    {
        if (BodyRenderer == null || sprite == null) return;
        BodyRenderer.sprite = sprite;
    }

    public void SetSquash(float amount) => squashY = amount;

    public void SetSecondaryAmplitude(float amp) => secondaryAmp = Mathf.Clamp01(amp);

    public void SetExpressionOverlay(Sprite faceSprite, float alpha)
    {
        if (FaceRenderer == null) return;
        if (faceSprite != null)
            FaceRenderer.sprite = faceSprite;
        var c = FaceRenderer.color;
        c.a = alpha;
        FaceRenderer.color = c;
    }

    public void PulseHit()
    {
        squashY = -0.12f;
        EmotionIntensity = 1f;
    }

    private static Transform EnsureChild(string name, Transform parent, Vector3 localPos)
    {
        var t = parent.Find(name);
        if (t != null) return t;
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        return go.transform;
    }

    private static SpriteRenderer EnsureSr(Transform t, int order)
    {
        var sr = t.GetComponent<SpriteRenderer>();
        if (sr == null) sr = t.gameObject.AddComponent<SpriteRenderer>();
        sr.sortingOrder = order;
        return sr;
    }

    private static void SetLayerAlpha(SpriteRenderer sr, float a)
    {
        if (sr == null) return;
        var c = sr.color;
        c.a = a;
        sr.color = c;
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
}
