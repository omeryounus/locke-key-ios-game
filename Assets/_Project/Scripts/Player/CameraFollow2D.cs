using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Production 2D camera: smooth follow, look-ahead, soft vertical lag,
/// screen pulse, and portrait viewport letterboxing.
/// </summary>
public class CameraFollow2D : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 1.05f, -10f);
    public float smoothSpeed = 7.5f;
    public float arrivalIntroOffsetX = -2.5f;
    public float lookAheadDistance = 1.15f;
    public float lookAheadSmooth = 4.5f;
    public float verticalSmooth = 5.5f;

    [SerializeField] private bool fitPortraitViewport = true;
    [SerializeField] private float portraitOrthoSize = 4.2f;

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

    private void LateUpdate()
    {
        ApplyPortraitViewport();

        if (target != null)
        {
            if (targetBody == null)
                targetBody = target.GetComponent<Rigidbody2D>();

            var vx = targetBody != null ? targetBody.linearVelocity.x : 0f;
            var desiredLook = Mathf.Clamp(vx / 4.2f, -1f, 1f) * lookAheadDistance;
            lookAheadX = Mathf.Lerp(lookAheadX, desiredLook, lookAheadSmooth * Time.deltaTime);

            var desired = target.position + offset + introOffset;
            desired.x += lookAheadX;
            desired.z = offset.z;

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

            var speed = introActive ? smoothSpeed * 0.65f : smoothSpeed;
            transform.position = Vector3.Lerp(transform.position, desired, speed * Time.deltaTime);

            if (introActive)
                introOffset = Vector3.Lerp(introOffset, Vector3.zero, Time.deltaTime * 0.85f);
        }

        if (cam == null) return;

        var baseSize = fitPortraitViewport ? portraitOrthoSize : 5f;
        if (pulseTimer > 0f)
        {
            pulseTimer -= Time.deltaTime;
            var envelope = Mathf.Clamp01(pulseTimer);
            cam.orthographicSize = baseSize + Mathf.Sin(Time.time * 28f) * pulseStrength * envelope;
        }
        else if (fitPortraitViewport)
        {
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, baseSize, Time.deltaTime * 10f);
        }
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
