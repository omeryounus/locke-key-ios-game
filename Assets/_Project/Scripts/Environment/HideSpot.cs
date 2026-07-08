using UnityEngine;

/// <summary>
/// Foreground cover where the player can break Echo line-of-sight (Beat 5).
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class HideSpot : MonoBehaviour
{
    private static int occupants;

    public static bool IsPlayerHidden => occupants > 0;

    private void Awake()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() != null)
            occupants++;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() != null)
            occupants = Mathf.Max(0, occupants - 1);
    }
}