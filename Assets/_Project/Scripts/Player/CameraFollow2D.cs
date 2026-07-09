using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Smooth 2D camera follow for side-scrolling greybox levels.
/// Optionally restricts output to the portrait gameplay strip on landscape devices.
/// </summary>
public class CameraFollow2D : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 1f, -10f);
    public float smoothSpeed = 6f;
    public float arrivalIntroOffsetX = -2.5f;

    [SerializeField] private bool fitPortraitViewport = true;
    [SerializeField] private float portraitOrthoSize = 4.2f;

    private bool introActive;
    private Vector3 introOffset;
    private float pulseTimer;
    private float pulseStrength;
    private Camera cam;
    private Rect lastViewportRect;
    private Vector2Int lastScreen;

    private void Awake() => cam = GetComponent<Camera>();

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
        pulseStrength = strength;
        pulseTimer = duration;
    }

    private void LateUpdate()
    {
        ApplyPortraitViewport();

        if (target != null)
        {
            var desired = target.position + offset + introOffset;
            desired.z = offset.z;
            var speed = introActive ? smoothSpeed * 0.65f : smoothSpeed;
            transform.position = Vector3.Lerp(transform.position, desired, speed * Time.deltaTime);

            if (introActive)
                introOffset = Vector3.Lerp(introOffset, Vector3.zero, Time.deltaTime * 0.8f);
        }

        if (cam == null) return;

        if (pulseTimer > 0f)
        {
            pulseTimer -= Time.deltaTime;
            var baseSize = fitPortraitViewport ? portraitOrthoSize : 5f;
            cam.orthographicSize = baseSize + Mathf.Sin(Time.time * 24f) * pulseStrength * pulseTimer;
        }
        else if (fitPortraitViewport)
        {
            cam.orthographicSize = portraitOrthoSize;
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