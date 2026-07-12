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
            // House Key door unlock gets deliberate confidence reach
            var inventory = FindFirstObjectByType<PlayerInventory>();
            if (NearestInteractable is StuckDoorPuzzle && inventory != null && inventory.HasHouseKey)
            {
                FindFirstObjectByType<PlayerSpriteAnimator>()?.PlayHouseKeyInteract(0.55f);
            }
            else
            {
                FindFirstObjectByType<PlayerSpriteAnimator>()?.PlayInteractPose(0.4f);
            }

            // Show contextual hint toast when available
            var hint = NearestInteractable.InteractionHint;
            if (!string.IsNullOrEmpty(hint) && NearestInteractable is PuzzleBase pb && !pb.isSolved)
            {
                // Don't spam full hint every interact — only soft rattle
            }

            NearestInteractable.Interact();
            GameHaptics.TriggerHapticLight();
            FindFirstObjectByType<CameraFollow2D>()?.Shake(0.07f, 0.18f);
            FindFirstObjectByType<CameraFollow2D>()?.Pulse(0.06f, 0.15f);
            return;
        }

        if (nothingToastCooldown <= 0f)
        {
            nothingToastCooldown = 1.4f;
            var hud = FindFirstObjectByType<GameplayHUD>();
            // Prefer nearest locked interactable hint if any in range
            var almost = FindNearestAnyHint();
            if (!string.IsNullOrEmpty(almost))
                hud?.ShowToast(almost, 2.4f);
            else
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

    private string FindNearestAnyHint()
    {
        string best = null;
        float bestD = interactRadius * 1.35f;
        foreach (var item in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
        {
            if (item is not IInteractable inter) continue;
            var dist = Vector2.Distance(transform.position, item.transform.position);
            if (dist > bestD) continue;
            var hint = inter.InteractionHint;
            if (string.IsNullOrEmpty(hint)) continue;
            bestD = dist;
            best = hint;
        }
        return best;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
