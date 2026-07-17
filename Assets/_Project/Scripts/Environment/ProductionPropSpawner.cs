using UnityEngine;

/// <summary>
/// Spawns production prop sprites from Resources/Art/Props into the foyer set-dressing.
/// Replaces geometric placeholders when art is available.
/// </summary>
public class ProductionPropSpawner : MonoBehaviour
{
    private void Start()
    {
        // Delay one frame so FoyerEnvironmentBuilder can create roots first
        Invoke(nameof(Dress), 0.05f);
    }

    private void Dress()
    {
        if (GameObject.Find("ProductionProps") != null) return;
        var root = new GameObject("ProductionProps");

        Spawn(root.transform, "prop_carpet", new Vector3(0.2f, -1.45f, 0.11f), new Vector3(2.8f, 0.55f, 1f), 2);
        Spawn(root.transform, "prop_bookshelf", new Vector3(3.5f, 0.15f, 0.2f), new Vector3(1.1f, 1.9f, 1f), 5);
        Spawn(root.transform, "prop_chandelier", new Vector3(0.15f, 2.05f, 0.14f), new Vector3(1.2f, 1.0f, 1f), 8);
        Spawn(root.transform, "prop_candle", new Vector3(-2.9f, -0.45f, 0.25f), new Vector3(0.45f, 0.55f, 1f), 7);
        Spawn(root.transform, "prop_candle", new Vector3(0.9f, -0.65f, 0.25f), new Vector3(0.4f, 0.5f, 1f), 7);
        Spawn(root.transform, "prop_grandfather_clock", new Vector3(-3.6f, 0.05f, 0.2f), new Vector3(0.7f, 1.8f, 1f), 5);
        Spawn(root.transform, "prop_portrait_frame", new Vector3(-1.5f, 1.2f, 0.28f), new Vector3(0.75f, 0.9f, 1f), 4);
        Spawn(root.transform, "prop_portrait_frame", new Vector3(1.4f, 1.25f, 0.28f), new Vector3(0.65f, 0.8f, 1f), 4);
        Spawn(root.transform, "prop_broken_chair", new Vector3(2.2f, -1.15f, 0.18f), new Vector3(0.7f, 0.55f, 1f), 4);

        // Door art if present
        var door = FindFirstObjectByType<StuckDoorPuzzle>();
        if (door != null)
        {
            var doorSpr = Resources.Load<Sprite>("Art/Environments/door_front_production");
            var sr = door.GetComponent<SpriteRenderer>() ?? door.GetComponentInChildren<SpriteRenderer>();
            if (doorSpr != null && sr != null)
            {
                sr.sprite = doorSpr;
                sr.color = Color.white;
                // Keep the door as a room-scale objective, never a screen-filling plate.
                door.transform.localScale = Vector3.one * 0.92f;
            }
        }
    }

    private static void Spawn(Transform parent, string resourceName, Vector3 pos, Vector3 scale, int order)
    {
        var spr = Resources.Load<Sprite>($"Art/Props/{resourceName}");
        if (spr == null) return;
        var go = new GameObject(resourceName);
        go.transform.SetParent(parent);
        go.transform.position = pos;
        go.transform.localScale = scale;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = spr;
        sr.sortingOrder = order;
        sr.color = Color.white;
    }
}
