using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Cinematic 2D camera: smooth follow, idle sway, look-ahead, interest zoom,
/// interaction shake, and portrait viewport letterboxing.
/// </summary>
public class CameraFollow2D : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 1.05f, -10f);
    public float smoothSpeed = 7.5f;
    public float arrivalIntroOffsetX = -1.8f;
    public float lookAheadDistance = 1.05f;
    public float lookAheadSmooth = 4.5f;
    public float verticalSmooth = 5.5f;
    public float idleSwayAmount = 0.045f;
    public float idleSwaySpeed = 0.55f;

    [SerializeField] private bool fitPortraitViewport = true;
    [SerializeField] private float portraitOrthoSize = 4.15f;

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
    private float interestZoomVel;
    private float shakeTimer;
    private float shakeStrength;
    private Vector3 shakeOffset;
    private Vector3 smoothPos;
    private bool hasSmoothPos;
    private float autoZoom = 1f;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (target != null)
            targetBody = target.GetComponent<Rigidbody2D>();
    }

    private void OnEnable() => ApplyPortraitViewport();

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
        shakeStrength = Mathf.Max(shakeStrength, strength);
        shakeTimer = Mathf.Max(shakeTimer, duration);
    }

    /// <summary>1 = normal, &lt;1 = zoom in toward subject.</summary>
    public void SetInterestZoom(float zoom)
    {
        interestZoom = Mathf.Clamp(zoom, 0.78f, 1.18f);
    }

    private void LateUpdate()
    {
        ApplyPortraitViewport();
        UpdateAutoInterestZoom();

        if (target != null)
        {
            if (targetBody == null)
                targetBody = target.GetComponent<Rigidbody2D>();

            var vx = targetBody != null ? targetBody.linearVelocity.x : 0f;
            var desiredLook = Mathf.Clamp(vx / 4.2f, -1f, 1f) * lookAheadDistance;
            // Extra cinematic lead when moving fast
            desiredLook *= 1f + Mathf.Clamp01(Mathf.Abs(vx) / 5f) * 0.25f;
            lookAheadX = Mathf.Lerp(lookAheadX, desiredLook, lookAheadSmooth * Time.deltaTime);

            var desired = target.position + offset + introOffset;
            desired.x += lookAheadX;
            desired.z = offset.z;

            // Idle camera sway (living lens)
            float swayX = Mathf.Sin(Time.time * idleSwaySpeed) * idleSwayAmount;
            float swayY = Mathf.Sin(Time.time * idleSwaySpeed * 0.73f + 1.2f) * idleSwayAmount * 0.65f;
            // Reduce sway when moving
            float swayMul = 1f - Mathf.Clamp01(Mathf.Abs(vx) / 3.5f) * 0.7f;
            desired.x += swayX * swayMul;
            desired.y += swayY * swayMul;

            if (!hasY)
            {
                currentY = desired.y;
                hasY = true;
            }
            else
            {
                currentY = Mathf.Lerp(currentY, desired.y, verticalSmooth * Time.deltaTime);
            }

            desired.y = currentY;

            if (shakeTimer > 0f)
            {
                shakeTimer -= Time.deltaTime;
                float env = Mathf.Clamp01(shakeTimer);
                shakeOffset = new Vector3(
                    (Mathf.PerlinNoise(Time.time * 32f, 0.1f) - 0.5f) * 2f,
                    (Mathf.PerlinNoise(0.2f, Time.time * 32f) - 0.5f) * 2f,
                    0f) * shakeStrength * env;
            }
            else
            {
                shakeOffset = Vector3.Lerp(shakeOffset, Vector3.zero, Time.deltaTime * 10f);
            }

            desired += shakeOffset;

            // Critically damped-ish smooth follow (feels more cinematic than raw Lerp)
            if (!hasSmoothPos)
            {
                smoothPos = desired;
                hasSmoothPos = true;
            }

            var speed = introActive ? smoothSpeed * 0.55f : smoothSpeed;
            // Frame-rate independent exponential smooth
            float k = 1f - Mathf.Exp(-speed * Time.deltaTime);
            smoothPos = Vector3.Lerp(smoothPos, desired, k);
            transform.position = smoothPos;

            if (introActive)
                introOffset = Vector3.Lerp(introOffset, Vector3.zero, Time.deltaTime * 0.85f);
        }

        if (cam == null) return;

        float targetZoom = interestZoom * autoZoom;
        float zoomK = 1f - Mathf.Exp(-6f * Time.deltaTime);
        float currentZoomFactor = cam.orthographicSize / (fitPortraitViewport ? portraitOrthoSize : 5f);
        // blend toward target
        var baseSize = (fitPortraitViewport ? portraitOrthoSize : 5f) * Mathf.Lerp(currentZoomFactor, targetZoom, zoomK);

        if (pulseTimer > 0f)
        {
            pulseTimer -= Time.deltaTime;
            var envelope = Mathf.Clamp01(pulseTimer);
            cam.orthographicSize = baseSize + Mathf.Sin(Time.time * 28f) * pulseStrength * envelope;
        }
        else if (fitPortraitViewport)
        {
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, baseSize, Time.deltaTime * 6f);
        }
    }

    private void UpdateAutoInterestZoom()
    {
        // Zoom when near highlighted objective / interactables
        float desired = 1f;
        var guide = FindFirstObjectByType<ObjectiveGuideController>();
        if (target != null && guide != null && guide.CurrentTarget != null)
        {
            float d = Vector2.Distance(target.position, guide.CurrentTarget.position);
            if (d < 3.2f)
                desired = Mathf.Lerp(0.86f, 1f, d / 3.2f);
        }

        var interaction = FindFirstObjectByType<InteractionController>();
        if (interaction != null && interaction.NearestInteractable != null && interaction.NearestInteractable.CanInteract)
            desired = Mathf.Min(desired, 0.9f);

        autoZoom = Mathf.Lerp(autoZoom, desired, Time.deltaTime * 2.5f);
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
