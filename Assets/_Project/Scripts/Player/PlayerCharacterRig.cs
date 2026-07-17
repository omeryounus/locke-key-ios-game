using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Production 2.5D layered rig for the gothic Keyhouse teen.
/// Hierarchy: Hip → Torso/Arms/Cape/Head/Face + Legs, independent cape spring chain,
/// foot shadow, key prop, and ability-driven visual modes (ghost / hide / scare / mindscape).
/// </summary>
[DisallowMultipleComponent]
public class PlayerCharacterRig : MonoBehaviour
{
    public enum VisualMode { Normal, Ghost, Hide, ScareFreeze, Mindscape, MirrorFlash }

    public Transform VisualRoot { get; private set; }
    public Transform Hip { get; private set; }
    public Transform Torso { get; private set; }
    public Transform CapeRoot { get; private set; }
    public Transform CapeTip { get; private set; }
    public Transform Head { get; private set; }
    public Transform Hair { get; private set; }
    public Transform Face { get; private set; }
    public Transform Eyes { get; private set; }
    public Transform Brows { get; private set; }
    public Transform ArmL { get; private set; }
    public Transform ArmR { get; private set; }
    public Transform KeyProp { get; private set; }
    public Transform LegL { get; private set; }
    public Transform LegR { get; private set; }
    public Transform Shadow { get; private set; }

    public SpriteRenderer BodyRenderer { get; private set; }
    public SpriteRenderer CapeRenderer { get; private set; }
    public SpriteRenderer CapeTipRenderer { get; private set; }
    public SpriteRenderer HairRenderer { get; private set; }
    public SpriteRenderer FaceRenderer { get; private set; }
    public SpriteRenderer EyesRenderer { get; private set; }
    public SpriteRenderer BrowsRenderer { get; private set; }
    public SpriteRenderer ArmLRenderer { get; private set; }
    public SpriteRenderer ArmRRenderer { get; private set; }
    public SpriteRenderer KeyPropRenderer { get; private set; }
    public SpriteRenderer ShadowRenderer { get; private set; }

    private Light2D rimLight;
    private PlayerController player;
    // The supplied hood/cape image is a multi-part concept sheet, not independently
    // cropped rig art. Keep the clean full-body atlas as the production character.
    [SerializeField] private bool useLayeredArt;
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
    private float capeAngle;
    private float capeTipAngle;
    private float capeVel;
    private float capeTipVel;
    private float ghostPulse;
    private float waryTimer = 8f;
    private float waryActive;
    private float shakenBreathBoost;
    private VisualMode mode = VisualMode.Normal;
    private Color bodyBase = new(1.12f, 1.12f, 1.12f, 1f);

    public float CyclePhase { get; set; }
    public float AirStretch { get; set; }
    public float MoveEnergy { get; set; }
    public float EmotionIntensity { get; set; }
    public float GhostWarning { get; set; } // 0..1 last second of phase
    public bool GhostMode => mode == VisualMode.Ghost;
    public float Facing => facing;
    public Vector3 BaseVisualScale => baseVisualScale;
    public VisualMode Mode => mode;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
        DisableLegacyVisualLayers(transform);
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

    /// <summary>
    /// Older presentation components create their own cape, hood, and shadow renderers.
    /// The modern rig owns those layers, so leave legacy helpers inactive instead of
    /// letting them render a second character over the animated body.
    /// </summary>
    public static void DisableLegacyVisualLayers(Transform root)
    {
        if (root == null) return;

        foreach (var name in new[] { "BlinkLid", "CloakFlutter", "HoodSway", "PlayerShadow" })
        {
            var legacy = root.Find(name);
            if (legacy != null)
                legacy.gameObject.SetActive(false);
        }
    }

    private void BuildHierarchy()
    {
        BodyRenderer = GetComponent<SpriteRenderer>();
        if (BodyRenderer == null)
            BodyRenderer = gameObject.AddComponent<SpriteRenderer>();
        BodyRenderer.sortingOrder = 20;
        bodyBase = BodyRenderer.color.a > 0.1f ? BodyRenderer.color : bodyBase;

        VisualRoot = EnsureChild("VisualRoot", transform, Vector3.zero);
        Shadow = EnsureChild("FootShadow", VisualRoot, new Vector3(0f, -0.02f, 0.05f));
        Hip = EnsureChild("Hip", VisualRoot, new Vector3(0f, 0.05f, 0f));
        LegL = EnsureChild("LegL", Hip, new Vector3(-0.08f, -0.05f, 0f));
        LegR = EnsureChild("LegR", Hip, new Vector3(0.08f, -0.05f, 0f));
        Torso = EnsureChild("Torso", Hip, new Vector3(0f, 0.22f, 0f));
        ArmL = EnsureChild("ArmL", Torso, new Vector3(-0.14f, 0.06f, 0f));
        ArmR = EnsureChild("ArmR", Torso, new Vector3(0.14f, 0.04f, 0f));
        KeyProp = EnsureChild("KeyProp", ArmR, new Vector3(0.12f, -0.08f, 0f));
        // Independent cape spring chain (not parented under torso rotation fully — hip based)
        CapeRoot = EnsureChild("CapeRoot", Hip, new Vector3(-0.04f, 0.28f, 0f));
        CapeTip = EnsureChild("CapeTip", CapeRoot, new Vector3(-0.08f, -0.2f, 0f));
        Head = EnsureChild("Head", Torso, new Vector3(0.02f, 0.36f, 0f));
        Hair = EnsureChild("Hair", Head, new Vector3(-0.02f, 0.1f, 0f));
        Face = EnsureChild("Face", Head, new Vector3(0.04f, 0.02f, 0f));
        Eyes = EnsureChild("Eyes", Face, new Vector3(0.02f, 0.05f, 0f));
        Brows = EnsureChild("Brows", Face, new Vector3(0.02f, 0.1f, 0f));

        ShadowRenderer = EnsureSr(Shadow, 5);
        CapeRenderer = EnsureSr(CapeRoot, 17);
        CapeTipRenderer = EnsureSr(CapeTip, 16);
        ArmLRenderer = EnsureSr(ArmL, 19);
        ArmRRenderer = EnsureSr(ArmR, 22);
        KeyPropRenderer = EnsureSr(KeyProp, 23);
        HairRenderer = EnsureSr(Hair, 24);
        FaceRenderer = EnsureSr(Face, 25);
        EyesRenderer = EnsureSr(Eyes, 26);
        BrowsRenderer = EnsureSr(Brows, 27);

        Shadow.localScale = new Vector3(0.9f, 0.22f, 1f);
        CapeRoot.localScale = new Vector3(0.85f, 0.9f, 1f);
        CapeTip.localScale = new Vector3(0.7f, 0.55f, 1f);
        ArmL.localScale = ArmR.localScale = new Vector3(0.32f, 0.38f, 1f);
        KeyProp.localScale = new Vector3(0.28f, 0.28f, 1f);
        Head.localScale = new Vector3(0.4f, 0.4f, 1f);
        Hair.localScale = new Vector3(0.55f, 0.5f, 1f);
        Face.localScale = new Vector3(0.26f, 0.26f, 1f);
        Eyes.localScale = new Vector3(0.32f, 0.1f, 1f);
        Brows.localScale = new Vector3(0.3f, 0.06f, 1f);

        // Layers mostly support secondary motion; body atlas carries primary silhouette
        SetAlpha(CapeRenderer, 0.52f);
        SetAlpha(CapeTipRenderer, 0.35f);
        SetAlpha(ArmLRenderer, 0f);
        SetAlpha(ArmRRenderer, 0f);
        SetAlpha(KeyPropRenderer, 0f);
        SetAlpha(HairRenderer, 0.32f);
        SetAlpha(FaceRenderer, 0f);
        SetAlpha(EyesRenderer, 0f);
        SetAlpha(BrowsRenderer, 0f);
    }

    private void LoadLayerSprites()
    {
        var disc = SoftDisc(32);
        ShadowRenderer.sprite = disc;
        ShadowRenderer.color = new Color(0f, 0f, 0f, 0.4f);

        if (!useLayeredArt)
        {
            DisableLayeredRenderers();
            return;
        }

        var cape = PlayerSpriteAtlas.LoadSingle("Art/Characters/Layers/player_hood_cape")
                   ?? PlayerSpriteAtlas.LoadSingle("Art/Characters/Layers/player_hair_hood")
                   ?? disc;
        CapeRenderer.sprite = cape;
        CapeTipRenderer.sprite = cape;
        HairRenderer.sprite = PlayerSpriteAtlas.LoadSingle("Art/Characters/Layers/player_hair_hood") ?? cape;
        FaceRenderer.sprite = PlayerSpriteAtlas.LoadSingle("Art/Characters/Layers/player_portrait")
                              ?? PlayerSpriteAtlas.LoadSingle("Art/Characters/Layers/player_head")
                              ?? disc;
        ArmLRenderer.sprite = PlayerSpriteAtlas.LoadSingle("Art/Characters/Layers/player_arm_l")
                              ?? PlayerSpriteAtlas.LoadSingle("Art/Characters/Layers/player_arms_reach")
                              ?? disc;
        ArmRRenderer.sprite = PlayerSpriteAtlas.LoadSingle("Art/Characters/Layers/player_arm_r")
                              ?? ArmLRenderer.sprite;
        KeyPropRenderer.sprite = disc;
        EyesRenderer.sprite = disc;
        BrowsRenderer.sprite = disc;
        EyesRenderer.color = new Color(0.05f, 0.04f, 0.07f, 0f);
        BrowsRenderer.color = new Color(0.12f, 0.08f, 0.1f, 0f);
    }

    private void DisableLayeredRenderers()
    {
        foreach (var renderer in new[]
                 {
                     CapeRenderer, CapeTipRenderer, HairRenderer, FaceRenderer,
                     EyesRenderer, BrowsRenderer, ArmLRenderer, ArmRRenderer, KeyPropRenderer
                 })
        {
            if (renderer != null)
                renderer.enabled = false;
        }
    }

    private void EnsureRim()
    {
        var existing = transform.Find("VisualRoot/PlayerRimLight");
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

    public void SetMode(VisualMode m) => mode = m;

    public void SetBodySprite(Sprite sprite)
    {
        if (BodyRenderer != null && sprite != null)
            BodyRenderer.sprite = sprite;
    }

    public void SetSquash(float amount) => squashY = amount;
    public void SetSecondaryAmplitude(float amp) => secondaryAmp = Mathf.Clamp01(amp);

    public void SetExpressionOverlay(Sprite faceSprite, float alpha)
    {
        if (FaceRenderer == null) return;
        if (faceSprite != null) FaceRenderer.sprite = faceSprite;
        SetAlpha(FaceRenderer, alpha);
    }

    public void SetKeyPropVisible(bool on, Color? tint = null)
    {
        if (KeyPropRenderer == null) return;
        SetAlpha(KeyPropRenderer, on ? 0.9f : 0f);
        if (tint.HasValue)
        {
            var c = tint.Value;
            c.a = on ? 0.9f : 0f;
            KeyPropRenderer.color = c;
        }
    }

    public void PulseHit()
    {
        squashY = -0.14f;
        EmotionIntensity = 1f;
        shakenBreathBoost = 1.4f;
    }

    public void BeginScareFreeze()
    {
        mode = VisualMode.ScareFreeze;
        secondaryAmp = 0f; // breath-hold stillness
        EmotionIntensity = 1f;
        if (EyesRenderer != null)
        {
            Eyes.localScale = new Vector3(0.4f, 0.16f, 1f); // wide eyes
            EyesRenderer.color = new Color(0.05f, 0.04f, 0.07f, 0.55f);
        }
    }

    public void EndScareFreeze()
    {
        if (mode == VisualMode.ScareFreeze)
            mode = VisualMode.Normal;
        secondaryAmp = 1f;
        shakenBreathBoost = 1.6f; // shallow fast recover breath
        if (Eyes != null) Eyes.localScale = new Vector3(0.32f, 0.1f, 1f);
        if (EyesRenderer != null) EyesRenderer.color = new Color(0.05f, 0.04f, 0.07f, 0f);
    }

    private void LateUpdate()
    {
        float dt = Time.deltaTime;
        float breathRate = (1.7f + MoveEnergy * 0.8f) * (1f + shakenBreathBoost * 0.5f);
        if (mode == VisualMode.ScareFreeze)
            breathRate = 0f; // freeze alive-when-still
        else
            breathPhase += dt * breathRate;

        shakenBreathBoost = Mathf.MoveTowards(shakenBreathBoost, 0f, dt * 0.35f);

        // Face input first (responsive), then velocity — never flip on micro drift
        float desired = facing;
        if (player != null && Mathf.Abs(player.MoveInput) > 0.15f)
            desired = Mathf.Sign(player.MoveInput);
        else if (player != null && Mathf.Abs(player.Velocity.x) > 0.45f)
            desired = Mathf.Sign(player.Velocity.x);
        // SmoothDamp time ~2–3 frames at 60fps for snappy but non-snapping turns
        facing = Mathf.SmoothDamp(facing, desired, ref facingVel, 0.045f, 50f);
        float faceSign = facing >= 0f ? 1f : -1f;
        float faceAmt = Mathf.Clamp01(Mathf.Abs(facing));

        float breath = mode == VisualMode.ScareFreeze
            ? 0f
            : Mathf.Sin(breathPhase) * 0.018f * secondaryAmp;
        float breath2 = mode == VisualMode.ScareFreeze
            ? 0f
            : Mathf.Sin(breathPhase * 0.5f + 0.4f) * 0.008f * secondaryAmp;

        // Hide: crouch scale
        float hideSquash = mode == VisualMode.Hide ? -0.08f : 0f;
        float ghostFloat = mode == VisualMode.Ghost ? Mathf.Sin(Time.time * 3.2f) * 0.015f : 0f;

        float sy = 1f + squashY + breath + breath2 + AirStretch * 0.08f + hideSquash;
        float sx = 1f - squashY * 0.65f - AirStretch * 0.04f + breath * 0.3f;
        VisualRoot.localScale = new Vector3(
            baseVisualScale.x * sx * faceSign * Mathf.Lerp(0.15f, 1f, faceAmt),
            baseVisualScale.y * sy,
            1f);
        VisualRoot.localPosition = new Vector3(0f, ghostFloat, 0f);

        float vx = player != null ? player.Velocity.x : 0f;
        float targetLean = -MoveEnergy * 6.5f * faceSign - vx * 0.55f;
        if (player != null && player.IsWallSliding)
            targetLean = wallSignLean(faceSign) * 12f;
        if (player != null && !player.IsGrounded)
            targetLean += -vx * 0.35f;
        if (mode == VisualMode.Hide)
            targetLean *= 0.3f;
        leanZ = Mathf.Lerp(leanZ, targetLean, dt * 8f);
        VisualRoot.localRotation = Quaternion.Euler(0f, 0f, leanZ * 0.35f);

        AnimateCapeSpring(dt, faceSign);
        AnimateSecondary(dt, faceSign);
        AnimateWaryPersonality(dt, faceSign);
        AnimateShadow();
        AnimateAbilityVisuals(dt);
    }

    private float wallSignLean(float faceSign)
    {
        if (player == null) return faceSign;
        return player.WallSign; // press into wall
    }

    private void AnimateCapeSpring(float dt, float faceSign)
    {
        if (CapeRoot == null) return;

        // Spring target lags torso/lean — independent chain
        float target = -leanZ * 1.4f - MoveEnergy * 14f * faceSign;
        if (mode == VisualMode.Hide)
            target = 8f * faceSign; // gathered close
        if (mode == VisualMode.ScareFreeze)
            target = 18f * faceSign; // snaps protective inward
        if (mode == VisualMode.Ghost)
            target += Mathf.Sin(Time.time * 4f) * 8f;

        // 2-bone spring with overshoot
        float stiffness = 28f;
        float damp = 7f;
        float acc = (target - capeAngle) * stiffness - capeVel * damp;
        capeVel += acc * dt;
        capeAngle += capeVel * dt;

        float tipTarget = capeAngle * 1.35f + Mathf.Sin(Time.time * 2.6f + CyclePhase * 6f) * (4f + MoveEnergy * 6f);
        float tipAcc = (tipTarget - capeTipAngle) * 34f - capeTipVel * 8f;
        capeTipVel += tipAcc * dt;
        capeTipAngle += capeTipVel * dt;

        CapeRoot.localRotation = Quaternion.Euler(0f, 0f, capeAngle);
        if (CapeTip != null)
            CapeTip.localRotation = Quaternion.Euler(0f, 0f, capeTipAngle - capeAngle);

        float capeA = mode switch
        {
            VisualMode.Ghost => 0.28f + GhostWarning * 0.2f,
            VisualMode.Hide => 0.62f,
            _ => 0.48f + MoveEnergy * 0.12f
        };
        SetAlpha(CapeRenderer, capeA);
        SetAlpha(CapeTipRenderer, capeA * 0.7f);
    }

    private void AnimateSecondary(float dt, float faceSign)
    {
        float t = Time.time;
        float swing = Mathf.Sin(CyclePhase * Mathf.PI * 2f) * (12f + MoveEnergy * 16f);
        if (mode == VisualMode.ScareFreeze || mode == VisualMode.Hide)
            swing = 0f;

        if (ArmR != null)
            ArmR.localRotation = Quaternion.Euler(0f, 0f, swing * 0.14f * secondaryAmp);
        if (ArmL != null)
            ArmL.localRotation = Quaternion.Euler(0f, 0f, -swing * 0.12f * secondaryAmp);

        if (Torso != null)
        {
            float bob = Mathf.Abs(Mathf.Sin(CyclePhase * Mathf.PI * 2f)) * MoveEnergy * 0.03f;
            float breathY = mode == VisualMode.ScareFreeze ? 0f : Mathf.Sin(breathPhase) * 0.012f * secondaryAmp;
            Torso.localPosition = new Vector3(0f, 0.22f + bob + breathY, 0f);
            Torso.localRotation = Quaternion.Euler(0f, 0f, swing * 0.04f);
        }

        if (Head != null)
        {
            float headTilt = -swing * 0.05f + EmotionIntensity * 4f;
            if (mode == VisualMode.Mindscape) headTilt = -8f;
            if (mode == VisualMode.Hide) headTilt = 6f; // peek tilt
            Head.localRotation = Quaternion.Euler(0f, 0f, headTilt + Mathf.Sin(t * 1.3f) * 1.2f * secondaryAmp);
            Head.localPosition = new Vector3(0.02f * faceSign, 0.36f + Mathf.Sin(breathPhase) * 0.01f * secondaryAmp, 0f);
        }

        if (Hair != null)
        {
            Hair.localRotation = Quaternion.Euler(0f, 0f,
                Mathf.Sin(t * 2.8f + 1f) * (4f + MoveEnergy * 6f) * secondaryAmp - MoveEnergy * 8f * faceSign);
        }

        // Blink — suppressed during scare freeze / mindscape
        bool canBlink = MoveEnergy < 0.25f && mode != VisualMode.ScareFreeze
                        && (player == null || player.IsGrounded);
        if (mode == VisualMode.Mindscape) canBlink = false;

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
                blinkTimer = Random.Range(2f, 4.5f);
                if (EyesRenderer != null && mode != VisualMode.ScareFreeze)
                    EyesRenderer.color = new Color(0.06f, 0.05f, 0.08f, 0f);
            }
        }
    }

    private void AnimateWaryPersonality(float dt, float faceSign)
    {
        // Brave-but-vulnerable tell: glance over shoulder / hand-to-key every 8–10s while idle
        if (mode != VisualMode.Normal || MoveEnergy > 0.15f)
        {
            waryTimer = Mathf.Max(waryTimer, 6f);
            waryActive = 0f;
            return;
        }

        if (waryActive > 0f)
        {
            waryActive -= dt;
            if (Head != null)
                Head.localRotation *= Quaternion.Euler(0f, 0f, -12f * faceSign * Mathf.Sin(waryActive * 6f));
            if (ArmR != null)
                ArmR.localRotation = Quaternion.Euler(0f, 0f, 18f); // touch key at belt
            SetKeyPropVisible(true, new Color(0.95f, 0.8f, 0.35f, 0.85f));
            if (waryActive <= 0f)
                SetKeyPropVisible(false);
            return;
        }

        waryTimer -= dt;
        if (waryTimer <= 0f)
        {
            waryActive = 0.7f;
            waryTimer = Random.Range(8f, 11f);
        }
    }

    private void AnimateShadow()
    {
        if (Shadow == null || ShadowRenderer == null) return;
        bool grounded = player == null || player.IsGrounded;
        float heightFactor = 1f;
        if (player != null && !grounded)
            heightFactor = Mathf.Clamp(1f - Mathf.Abs(player.Velocity.y) * 0.04f, 0.45f, 1f);

        float walk = player != null ? Mathf.Clamp01(player.HorizontalSpeed / 5f) : 0f;
        Shadow.localScale = new Vector3(0.85f * heightFactor * (1f + walk * 0.1f), 0.2f * heightFactor, 1f);
        var c = ShadowRenderer.color;
        // Snap full opacity on grounded contact feel
        c.a = grounded ? 0.44f : 0.14f;
        if (player != null && player.JustLanded) c.a = 0.55f;
        ShadowRenderer.color = c;
    }

    private void AnimateAbilityVisuals(float dt)
    {
        ghostPulse += dt;
        if (BodyRenderer == null) return;

        switch (mode)
        {
            case VisualMode.Ghost:
            {
                float warn = GhostWarning;
                float flicker = 0.45f + Mathf.Sin(Time.time * (9f + warn * 14f)) * (0.12f + warn * 0.1f);
                BodyRenderer.color = new Color(0.38f, 0.95f, 0.68f, flicker);
                if (rimLight != null)
                {
                    rimLight.color = Color.Lerp(new Color(0.4f, 1f, 0.7f), Color.white, warn);
                    rimLight.intensity = 0.85f + warn * 0.6f + Mathf.Sin(Time.time * (6f + warn * 10f)) * 0.15f;
                }
                break;
            }
            case VisualMode.Mindscape:
                BodyRenderer.color = Color.Lerp(BodyRenderer.color, new Color(0.85f, 0.75f, 1f, 0.95f), dt * 4f);
                if (rimLight != null)
                {
                    rimLight.color = new Color(0.7f, 0.45f, 1f);
                    rimLight.intensity = 0.7f + Mathf.Sin(Time.time * 2f) * 0.1f;
                }
                break;
            case VisualMode.MirrorFlash:
                BodyRenderer.color = Color.Lerp(BodyRenderer.color, Color.white, dt * 12f);
                if (rimLight != null) rimLight.intensity = 1.4f;
                break;
            case VisualMode.Hide:
                BodyRenderer.color = Color.Lerp(BodyRenderer.color, bodyBase * 0.85f, dt * 3f);
                break;
            default:
                if (player == null || !player.IsGhostPhasing)
                    BodyRenderer.color = Color.Lerp(BodyRenderer.color, bodyBase, dt * 6f);
                if (rimLight != null)
                {
                    rimLight.color = new Color(1f, 0.92f, 0.75f);
                    rimLight.intensity = 0.5f + Mathf.Sin(Time.time * 2f) * 0.08f;
                }
                break;
        }
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

    private static void SetAlpha(SpriteRenderer sr, float a)
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
