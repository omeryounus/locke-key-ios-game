using UnityEngine;

/// <summary>
/// Finds nearby interactables and triggers interaction (tap / E key).
/// </summary>
public class InteractionController : MonoBehaviour
{
    [SerializeField] private float interactRadius = 2.5f;

    public IInteractable NearestInteractable { get; private set; }

    private void Update()
    {
        NearestInteractable = FindNearestInteractable();
    }

    public void TryInteract()
    {
        if (NearestInteractable != null && NearestInteractable.CanInteract)
        {
            NearestInteractable.Interact();
            return;
        }

        Debug.Log("Nothing to interact with nearby.");
    }

    private IInteractable FindNearestInteractable()
    {
        IInteractable closest = null;
        var closestDist = interactRadius;

        ConsiderAll<PuzzleBase>(ref closest, ref closestDist);
        ConsiderAll<HouseKeyPickup>(ref closest, ref closestDist);
        ConsiderAll<GhostKeyPickup>(ref closest, ref closestDist);
        ConsiderAll<HeadKeyPickup>(ref closest, ref closestDist);

        return closest;
    }

    private void ConsiderAll<T>(ref IInteractable closest, ref float closestDist) where T : MonoBehaviour, IInteractable
    {
        var items = FindObjectsByType<T>(FindObjectsSortMode.None);
        foreach (var item in items)
        {
            if (!item.CanInteract) continue;

            var dist = Vector2.Distance(transform.position, item.transform.position);
            if (dist <= closestDist)
            {
                closestDist = dist;
                closest = item;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}