using UnityEngine;

/// <summary>
/// Owns the physical walking lane for Chapter 1. The painted room is scenery;
/// this component keeps the player, camera, saves, and interactables anchored
/// to a predictable floor instead of to a backdrop sprite's imported bounds.
/// </summary>
public class PlayableWorldFoundation : MonoBehaviour
{
    public const float WalkSurfaceY = -1.65f;
    public const float PlayerWalkY = -1.05f;
    public const float MinWalkX = -7.6f;
    public const float MaxWalkX = 12.4f;

    private const float GroundCenterY = -1.85f;
    private const float GroundHeight = 0.4f;

    private void Awake()
    {
        ConfigureGround();
        ConfigureBoundary("WorldLeftBoundary", MinWalkX - 0.25f);
        ConfigureBoundary("WorldRightBoundary", MaxWalkX + 0.25f);
    }

    public static Vector3 ClampToWalkablePosition(Vector3 position)
    {
        return new Vector3(
            Mathf.Clamp(position.x, MinWalkX, MaxWalkX),
            PlayerWalkY,
            0f);
    }

    private static void ConfigureGround()
    {
        var ground = GameObject.Find("Ground");
        if (ground == null)
            ground = new GameObject("Ground");

        ground.layer = LayerMask.NameToLayer("Ground");
        if (ground.layer < 0)
            ground.layer = 0;

        ground.transform.position = new Vector3(0f, GroundCenterY, 0f);
        ground.transform.localScale = Vector3.one;

        var collider = ground.GetComponent<BoxCollider2D>();
        if (collider == null)
            collider = ground.AddComponent<BoxCollider2D>();
        collider.isTrigger = false;
        collider.offset = Vector2.zero;
        collider.size = new Vector2(MaxWalkX - MinWalkX + 1f, GroundHeight);
    }

    private static void ConfigureBoundary(string name, float x)
    {
        var go = GameObject.Find(name);
        if (go == null)
            go = new GameObject(name);

        go.layer = LayerMask.NameToLayer("Ground");
        if (go.layer < 0)
            go.layer = 0;
        go.transform.position = new Vector3(x, 0.25f, 0f);
        go.transform.localScale = Vector3.one;

        var collider = go.GetComponent<BoxCollider2D>();
        if (collider == null)
            collider = go.AddComponent<BoxCollider2D>();
        collider.isTrigger = false;
        collider.offset = Vector2.zero;
        collider.size = new Vector2(0.35f, 4.8f);
    }
}
