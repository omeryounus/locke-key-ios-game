using UnityEngine;

/// <summary>
/// Smooth 2D camera follow for side-scrolling greybox levels.
/// </summary>
public class CameraFollow2D : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 1f, -10f);
    public float smoothSpeed = 6f;

    private void LateUpdate()
    {
        if (target == null) return;

        var desired = target.position + offset;
        desired.z = offset.z;
        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
    }
}