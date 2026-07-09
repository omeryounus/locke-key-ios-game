using UnityEngine;

/// <summary>
/// Restricts the main camera to the portrait gameplay strip and tunes ortho size
/// so backgrounds and the player read clearly on landscape devices.
/// </summary>
[RequireComponent(typeof(Camera))]
public class GameplayViewportCamera : MonoBehaviour
{
    [SerializeField] private float portraitOrthoSize = 4.2f;
    [SerializeField] private bool applyOnTitleScene;

    private Camera cam;
    private Rect lastRect;
    private Vector2Int lastScreen;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void OnEnable() => Apply();

    private void LateUpdate()
    {
        var screen = new Vector2Int(Screen.width, Screen.height);
        if (screen == lastScreen) return;
        lastScreen = screen;
        Apply();
    }

    private void Apply()
    {
        if (cam == null) return;

        if (!applyOnTitleScene && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "TitleScreen")
            return;

        var vp = LockeUILayout.ComputeViewportSize(Screen.width, Screen.height);
        var rect = new Rect(
            (Screen.width - vp.x) * 0.5f / Screen.width,
            (Screen.height - vp.y) * 0.5f / Screen.height,
            vp.x / Screen.width,
            vp.y / Screen.height);

        if (rect == lastRect && Mathf.Approximately(cam.orthographicSize, portraitOrthoSize))
            return;

        lastRect = rect;
        cam.rect = rect;
        cam.orthographicSize = portraitOrthoSize;
    }
}