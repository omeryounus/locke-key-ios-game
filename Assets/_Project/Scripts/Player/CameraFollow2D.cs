using UnityEngine;

/// <summary>
/// Smooth 2D camera follow for side-scrolling greybox levels.
/// </summary>
public class CameraFollow2D : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 1f, -10f);
    public float smoothSpeed = 6f;
    public float arrivalIntroOffsetX = -2.5f;

    private bool introActive;
    private Vector3 introOffset;

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

    private void LateUpdate()
    {
        if (target == null) return;

        var desired = target.position + offset + introOffset;
        desired.z = offset.z;
        var speed = introActive ? smoothSpeed * 0.65f : smoothSpeed;
        transform.position = Vector3.Lerp(transform.position, desired, speed * Time.deltaTime);

        if (introActive)
            introOffset = Vector3.Lerp(introOffset, Vector3.zero, Time.deltaTime * 0.8f);
    }
}