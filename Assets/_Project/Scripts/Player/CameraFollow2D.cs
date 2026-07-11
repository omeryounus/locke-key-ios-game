using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Commercial 2D follow camera: dead-zone tracking, exponential smooth,
/// look-ahead, interest zoom, shake/pulse. Avoids per-frame scene searches.
/// </summary>
public class CameraFollow2D : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 1.0f, -10f);
    [Tooltip("Higher = snappier follow. 8–12 feels mobile-premium.")]
    public float smoothSpeed = 9.5f;
    public float arrivalIntroOffsetX = -1.6f;
    public float lookAheadDistance = 0.95f;
    public float lookAheadSmooth = 5.5f;
    public float verticalSmooth = 7.5f;
    public float idleSwayAmount = 0.028f;
    public float idleSwaySpeed = 0.4f;
    [Tooltip("World-unit dead zone before camera recenters (reduces micro-jitter).")]
    public float deadZoneX = 0.18f;
    public float deadZoneY = 0.22f;

    [SerializeField] private bool fitPortraitViewport = true;
    [SerializeField] private float portraitOrthoSize = 4.15f;
    [SerializeField] private float minOrtho = 3.4f;
    [SerializeField] private float maxOrtho = 5.2f;

    private bool introActive;
    private Vector3 introOffset;
    private float pulseTimer;
    private float pulseStrength;
    private float lookAheadX;
    private float currentY;
    private bool hasY;
    private Camera cam;
    private Rect lastViewportRect;
    private Vector2Int lastScreen;
    private Rigidbody2D targetBody;
    private float interestZoom = 1f;
    private float shakeTimer;
    private float shakeStrength;
    private Vector3 shakeOffset;
    private Vector3 smoothPos;
    private bool hasSmoothPos;
    private float autoZoom = 1f;
    private Vector3 deadZoneCenter;

    // Cached — never Find in LateUpdate every frame
    private ObjectiveGuideController cachedGuide;
    private InteractionController cachedInteraction;
    private float cacheRefresh;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (target != null)
            targetBody = target.GetComponent<Rigidbody2D>();
        RefreshCaches(force: true);
    }

    private void OnEnable() => ApplyPortraitViewport();

    private void RefreshCaches(bool force = false)
    {
        cacheRefresh -= Time.unscaledDeltaTime;
        if (!force && cacheRefresh > 0f) return;
        cacheRefresh = 1.25f;
        if (cachedGuide == null) cachedGuide = FindFirstObjectByType<ObjectiveGuideController>();
        if (cachedInteraction == null) cachedInteraction = FindFirstObjectByType<InteractionController>();
    }

    public void BeginArrivalIntro()
    {
        introActive = true;
        introOffset = new Vector3(arrivalIntroOffsetX, 0f, 0f);
    }

    public void EndArrivalIntro()
    {
        introActive = false;
        introOffset = Vector3.zero;
    }

    public void Pulse(float strength, float duration)
    {
        pulseStrength = Mathf.Max(pulseStrength, strength);
        pulseTimer = Mathf.Max(pulseTimer, duration);
    }

    public void Shake(float strength, float duration)
    {
        // Cap shake so UI readability never fails on mobile
        shakeStrength = Mathf.Max(shakeStrength, Mathf.Min(strength, 0.22f));
        shakeTimer = Mathf.Max(shakeTimer, duration);
    }

    public void SetInterestZoom(float zoom)
    {
        interestZoom = Mathf.Clamp(zoom, 0.82f, 1.12f);
    }

    private void LateUpdate()
    {
        ApplyPortraitViewport();
        RefreshCaches();
        UpdateAutoInterestZoom();

        if (target != null)
        {
            if (targetBody == null)
                targetBody = target.GetComponent<Rigidbody2D>();

            var vx = targetBody != null ? targetBody.linearVelocity.x : 0f;
            float speedAbs = Mathf.Abs(vx);

            // Look-ahead only when meaningfully moving — kills idle jitter
            float desiredLook = 0f;
            if (speedAbs > 0.35f)
            {
                desiredLook = Mathf.Clamp(vx / 4.5f, -1f, 1f) * lookAheadDistance;
                desiredLook *= 1f + Mathf.Clamp01(speedAbs / 5.5f) * 0.18f;
            }
            float lookK = 1f - Mathf.Exp(-lookAheadSmooth * Time.deltaTime);
            lookAheadX = Mathf.Lerp(lookAheadX, desiredLook, lookK);

            var desired = target.position + offset + introOffset;
            desired.x += lookAheadX;
            desired.z = offset.z;

            // Dead zone: don't chase micro player motion
            if (!hasSmoothPos)
            {
                deadZoneCenter = desired;
            }
            else
            {
                var delta = desired - deadZoneCenter;
                if (Mathf.Abs(delta.x) > deadZoneX)
                    deadZoneCenter.x = desired.x - Mathf.Sign(delta.x) * deadZoneX;
                if (Mathf.Abs(delta.y) > deadZoneY)
                    deadZoneCenter.y = desired.y - Mathf.Sign(delta.y) * deadZoneY;
                desired.x = Mathf.Lerp(desired.x, deadZoneCenter.x, 0.35f);
                desired.y = Mathf.Lerp(desired.y, deadZoneCenter.y, 0.45f);
            }

            // Idle sway only when nearly still
            float swayMul = Mathf.Clamp01(1f - speedAbs / 2.2f);
            if (swayMul > 0.05f && !introActive)
            {
                desired.x += Mathf.Sin(Time.time * idleSwaySpeed) * idleSwayAmount * swayMul;
                desired.y += Mathf.Sin(Time.time * idleSwaySpeed * 0.73f + 1.2f) * idleSwayAmount * 0.5f * swayMul;
            }

            if (!hasY)
            {
                currentY = desired.y;
                hasY = true;
            }
            else
            {
                float yK = 1f - Mathf.Exp(-verticalSmooth * Time.deltaTime);
                currentY = Mathf.Lerp(currentY, desired.y, yK);
            }
            desired.y = currentY;

            if (shakeTimer > 0f)
            {
                shakeTimer -= Time.deltaTime;
                float env = Mathf.Clamp01(shakeTimer);
                // Smooth noise (not pure random) — less nauseating on mobile
                shakeOffset = new Vector3(
                    (Mathf.PerlinNoise(Time.time * 28f, 0.1f) - 0.5f) * 2f,
                    (Mathf.PerlinNoise(0.2f, Time.time * 28f) - 0.5f) * 2f,
                    0f) * shakeStrength * env;
            }
            else
            {
                shakeOffset = Vector3.Lerp(shakeOffset, Vector3.zero, 1f - Mathf.Exp(-12f * Time.deltaTime));
            }
            desired += shakeOffset;

            if (!hasSmoothPos)
            {
                smoothPos = desired;
                hasSmoothPos = true;
            }

            float speed = introActive ? smoothSpeed * 0.5f : smoothSpeed;
            // Faster catch-up when far (prevents player leaving frame)
            float dist = Vector2.Distance(new Vector2(smoothPos.x, smoothPos.y), new Vector2(desired.x, desired.y));
            if (dist > 1.5f) speed *= 1.35f;
            if (dist > 3f) speed *= 1.6f;

            float k = 1f - Mathf.Exp(-speed * Time.deltaTime);
            smoothPos = Vector3.Lerp(smoothPos, desired, k);
            // Hard clamp Z
            smoothPos.z = offset.z;
            transform.position = smoothPos;

            if (introActive)
                introOffset = Vector3.Lerp(introOffset, Vector3.zero, 1f - Mathf.Exp(-0.9f * Time.deltaTime));
        }

        if (cam == null) return;

        float baseSize = fitPortraitViewport ? portraitOrthoSize : 5f;
        float targetSize = Mathf.Clamp(baseSize * interestZoom * autoZoom, minOrtho, maxOrtho);

        if (pulseTimer > 0f)
        {
            pulseTimer -= Time.deltaTime;
            var envelope = Mathf.Clamp01(pulseTimer);
            // Softer pulse — less sinusoid pop
            cam.orthographicSize = targetSize + pulseStrength * envelope * 0.85f;
        }
        else
        {
            float zK = 1f - Mathf.Exp(-5.5f * Time.deltaTime);
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, zK);
        }
    }

    private void UpdateAutoInterestZoom()
    {
        float desired = 1f;
        if (target != null && cachedGuide != null && cachedGuide.CurrentTarget != null)
        {
            float d = Vector2.Distance(target.position, cachedGuide.CurrentTarget.position);
            if (d < 2.8f)
                desired = Mathf.Lerp(0.9f, 1f, d / 2.8f);
        }

        if (cachedInteraction != null
            && cachedInteraction.NearestInteractable != null
            && cachedInteraction.NearestInteractable.CanInteract)
            desired = Mathf.Min(desired, 0.93f);

        float k = 1f - Mathf.Exp(-2.2f * Time.deltaTime);
        autoZoom = Mathf.Lerp(autoZoom, desired, k);
    }

    private void ApplyPortraitViewport()
    {
        if (!fitPortraitViewport || cam == null) return;
        if (SceneManager.GetActiveScene().name == "TitleScreen") return;

        var screen = new Vector2Int(Screen.width, Screen.height);
        var vp = LockeUILayout.ComputeViewportSize(screen.x, screen.y);
        var rect = new Rect(
            (screen.x - vp.x) * 0.5f / screen.x,
            (screen.y - vp.y) * 0.5f / screen.y,
            vp.x / screen.x,
            vp.y / screen.y);

        if (rect == lastViewportRect && screen == lastScreen) return;

        lastViewportRect = rect;
        lastScreen = screen;
        cam.rect = rect;
    }
}
