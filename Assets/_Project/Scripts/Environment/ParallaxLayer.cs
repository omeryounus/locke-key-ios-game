using UnityEngine;

/// <summary>
/// Scrolls a background layer slower than the camera for 2.5D depth.
/// </summary>
public class ParallaxLayer : MonoBehaviour
{
    [SerializeField] private float scrollFactor = 0.2f;

    public void Configure(float factor) => scrollFactor = factor;

    private Transform cameraTransform;

    private void Start()
    {
        cameraTransform = Camera.main != null ? Camera.main.transform : null;
    }

    private void LateUpdate()
    {
        if (cameraTransform == null)
            return;

        transform.position = new Vector3(cameraTransform.position.x * (1f - scrollFactor), transform.position.y, transform.position.z);
    }
}