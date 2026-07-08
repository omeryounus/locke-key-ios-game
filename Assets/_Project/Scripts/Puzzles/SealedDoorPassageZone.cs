using UnityEngine;

/// <summary>
/// Detects when the player crosses through the sealed door passage.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class SealedDoorPassageZone : MonoBehaviour
{
    private SealedDoorPuzzle door;

    public void Bind(SealedDoorPuzzle puzzle) => door = puzzle;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() != null)
            door?.NotifyPlayerCrossed();
    }
}