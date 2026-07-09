using UnityEngine;

/// <summary>
/// A reflective mirror portal used by the Mirror Key ability.
/// Connects to a paired MirrorSurface in another room.
/// </summary>
public class MirrorSurface : MonoBehaviour
{
    [Header("Mirror Portal Settings")]
    public MirrorSurface destinationMirror;
    public Vector3 spawnOffset = new Vector3(1f, 0f, 0f); // Spawn to the side to avoid overlapping colliders
    public bool isReflective = true;

    private void Awake()
    {
        // Add a box collider 2D if none exists, set it as trigger
        var col = GetComponent<Collider2D>();
        if (col == null)
        {
            var boxCol = gameObject.AddComponent<BoxCollider2D>();
            boxCol.isTrigger = true;
            boxCol.size = new Vector2(1.5f, 2.5f);
        }
    }

    /// <summary>
    /// Returns the coordinate point where the player will be teleported.
    /// </summary>
    public Vector3 GetTravelPosition()
    {
        return transform.position + spawnOffset;
    }
}
