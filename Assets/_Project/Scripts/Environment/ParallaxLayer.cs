using UnityEngine;

/// <summary>
/// Camera-relative parallax. scrollFactor 0 = locked to world, 1 = locked to camera.
/// </summary>
public class ParallaxLayer : MonoBehaviour
{
    [SerializeField] private float scrollFactor = 1f;
    [SerializeField] private bool lockYToCamera = true;
    [SerializeField] private float yOffset;

    private Transform cameraTransform;
    private Vector3 origin;
    private float originCamX;
    private float originCamY;
    private bool initialized;

    public void Configure(float factor, bool followCameraY = true, float verticalOffset = 0f)
    {
        scrollFactor = factor;
        lockYToCamera = followCameraY;
        yOffset = verticalOffset;
        initialized = false;
    }

    private void LateUpdate()
    {
        if (cameraTransform == null)
        {
            var cam = Camera.main;
            if (cam == null) return;
            cameraTransform = cam.transform;
        }

        if (!initialized)
        {
            origin = transform.position;
            originCamX = cameraTransform.position.x;
            originCamY = cameraTransform.position.y;
            initialized = true;
        }

        float dx = cameraTransform.position.x - originCamX;
        float x = origin.x + dx * scrollFactor;
        float y = lockYToCamera
            ? cameraTransform.position.y + yOffset
            : origin.y + (cameraTransform.position.y - originCamY) * scrollFactor;

        transform.position = new Vector3(x, y, origin.z);
    }
}
