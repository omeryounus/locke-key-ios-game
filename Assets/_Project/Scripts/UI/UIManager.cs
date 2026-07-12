using UnityEngine;

/// <summary>
/// Handles key selection UI, memory view overlays, and discovery notifications.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("References")]
    public KeyManager keyManager;

    private GameplayHUD hud;

    public void BindHUD(GameplayHUD gameplayHud)
    {
        hud = gameplayHud;
    }

    public void UpdateActiveKeyUI(KeyManager.KeyData key)
    {
        hud?.ShowToast($"Active key: {key.keyName}", 2f);
    }

    public void OpenMemoryView()
    {
        // Prefer portrait Mindscape puzzle; do not soft-open random text from Use Key.
        var portrait = FindFirstObjectByType<MemoryFragmentPuzzle>();
        if (portrait != null && !portrait.isSolved)
        {
            portrait.Interact();
            return;
        }

        ShowMemoryFragment(
            "A Fragment of Rendell",
            "You see a man standing before a door that should not exist.\n\n" +
            "\"Some doors are meant to stay shut,\" he whispers — but the key is already in his hand.\n\n" +
            "The memory fades, leaving only dread and wonder.");
    }

    public void ShowMemoryFragment(string title, string body, int panelIndex = 1)
    {
        hud?.ShowMemoryOverlay(title, body, panelIndex);
    }

    public void CloseMemoryView()
    {
        hud?.HideMemoryOverlay();
    }

    public void ShowKeyDiscoveryNotification(KeyManager.KeyData key)
    {
        hud?.ShowToast($"Discovered: {key.keyName}", 4f);
        hud?.FlashKeyDiscovered();
    }
}