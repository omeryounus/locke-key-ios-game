using UnityEngine;

/// <summary>
/// Finds nearby interactables, highlights the nearest, and triggers interaction.
/// </summary>
public class InteractionController : MonoBehaviour
{
    [SerializeField] private float interactRadius = 3.0f;

    public IInteractable NearestInteractable { get; private set; }

    private InteractableHighlight currentHighlight;
    private Component currentHighlightSource;
    private float nothingToastCooldown;

    private void Update()
    {
        if (nothingToastCooldown > 0f)
            nothingToastCooldown -= Time.deltaTime;

        var next = FindNearestInteractable(out var source);
        if (!ReferenceEquals(NearestInteractable, next))
        {
            if (currentHighlight != null)
                currentHighlight.SetHighlighted(false);

            NearestInteractable = next;
            currentHighlightSource = source;
            currentHighlight = InteractableHighlight.Ensure(source);
            currentHighlight?.SetHighlighted(next != null && next.CanInteract);
        }
        else if (currentHighlight != null && NearestInteractable != null)
        {
            currentHighlight.SetHighlighted(NearestInteractable.CanInteract);
        }
    }

    public void TryInteract()
    {
        if (NearestInteractable != null && NearestInteractable.CanInteract)
        {
            NearestInteractable.Interact();
            GameHaptics.TriggerHapticLight();
            return;
        }

        if (nothingToastCooldown <= 0f)
        {
            nothingToastCooldown = 1.4f;
            var hud = FindFirstObjectByType<GameplayHUD>();
            hud?.ShowToast("Nothing to interact with nearby.", 1.8f);
            FindFirstObjectByType<GameAudioController>()?.PlayDoorRattle();
        }
    }

    private IInteractable FindNearestInteractable(out Component source)
    {
        IInteractable closest = null;
        source = null;
        var closestDist = interactRadius;

        ConsiderAll<PuzzleBase>(ref closest, ref closestDist, ref source);
        ConsiderAll<HouseKeyPickup>(ref closest, ref closestDist, ref source);
        ConsiderAll<GhostKeyPickup>(ref closest, ref closestDist, ref source);
        ConsiderAll<HeadKeyPickup>(ref closest, ref closestDist, ref source);

        return closest;
    }

    private void ConsiderAll<T>(ref IInteractable closest, ref float closestDist, ref Component source)
        where T : MonoBehaviour, IInteractable
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
                source = item;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
