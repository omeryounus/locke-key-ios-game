using UnityEngine;

/// <summary>
/// Scrolls a background layer slower than the camera for 2.5D depth.
/// </summary>
public class ParallaxLayer : MonoBehaviour
{
    [SerializeField] private float scrollFactor = 0.2f;

    private Transform cameraTransform;
    private float lastCameraX;

    private void Start()
    {
        cameraTransform = Camera.main != null ? Camera.main.transform : null;
        if (cameraTransform != null)
            lastCameraX = cameraTransform.position.x;
    }

    private void LateUpdate()
    {
        if (cameraTransform == null)
            return;

        var deltaX = cameraTransform.position.x - lastCameraX;
        transform.position += new Vector3(deltaX * scrollFactor, 0f, 0f);
        lastCameraX = cameraTransform.position.x;
    }
}