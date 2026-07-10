using UnityEngine;

/// <summary>
/// Touch-first controls with keyboard fallback for Editor testing.
/// </summary>
public class TouchGameplayController : MonoBehaviour
{
    [Header("References")]
    public PlayerController player;
    public KeyManager keyManager;
    public InteractionController interaction;
    public PlayerInventory inventory;

    private float moveInput;
    private bool inputLocked;

    public float MoveInput => moveInput;

    public void SetInputLocked(bool locked)
    {
        inputLocked = locked;
        if (locked)
            moveInput = 0f;
    }
    private bool jumpRequested;
    private bool interactRequested;
    private bool useKeyRequested;
    private bool isKeyboardActive;

    private void Update()
    {
        if (inputLocked)
            return;

        ReadKeyboard();

        if (player != null)
            player.Move(moveInput);

        if (jumpRequested && player != null)
        {
            player.Jump();
            jumpRequested = false;
        }

        if (interactRequested)
        {
            interaction?.TryInteract();
            interactRequested = false;
        }

        if (useKeyRequested)
        {
            keyManager?.UseActiveKey();
            useKeyRequested = false;
        }
    }

    public void SetMoveInput(float value) => moveInput = value;
    public void RequestJump() => jumpRequested = true;
    public void RequestInteract() => interactRequested = true;
    public void RequestUseKey() => useKeyRequested = true;

    public string GetKeyStatusLabel()
    {
        return keyManager?.currentActiveKey != null
            ? $"Key: {keyManager.currentActiveKey.keyName}"
            : "Key: none";
    }

    public bool HasHouseKey => inventory != null && inventory.HasHouseKey;

    public string GetHouseKeyLabel()
    {
        return HasHouseKey ? "House key: yes" : "House key: no";
    }

    public string GetHintLabel()
    {
        var hint = interaction?.NearestInteractable?.InteractionHint;
        if (!string.IsNullOrEmpty(hint))
            return hint;

        var beat = FindFirstObjectByType<ChapterBeatDirector>();
        if (beat != null)
        {
            return beat.CurrentBeat switch
            {
                ChapterBeatDirector.Beat.Arrival => "→ Walk left to the glowing House Key, then tap Interact",
                ChapterBeatDirector.Beat.StuckDoor => inventory != null && inventory.HasHouseKey
                    ? "→ Walk to the front door and tap Interact to unlock it"
                    : "→ Pick up the glowing House Key first",
                ChapterBeatDirector.Beat.Library => keyManager?.ownedKeys.Exists(k => k.abilityType == KeyManager.KeyAbilityType.GhostPhase) == true
                    ? "→ Walk right to the sealed door with the Ghost Key"
                    : "→ Tap Interact on the collapsed bookshelf to clear it",
                ChapterBeatDirector.Beat.GhostKeyUse => "→ At the sealed door: tap Use Key, then walk through",
                ChapterBeatDirector.Beat.EchoEncounter => "→ Hide in the arch or run through the passage",
                _ => "→ Claim the Head Key, then Interact with the portrait"
            };
        }

        if (inventory != null && !inventory.HasHouseKey)
            return "Find the house key on the left";

        return "Explore Keyhouse";
    }

    private void ReadKeyboard()
    {
        var kbMove = Input.GetAxisRaw("Horizontal");
        if (Mathf.Abs(kbMove) > 0.01f)
        {
            moveInput = kbMove;
            isKeyboardActive = true;
        }
        else if (isKeyboardActive)
        {
            moveInput = 0f;
            isKeyboardActive = false;
        }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            jumpRequested = true;
            player?.SetJumpHeld(true);
        }

        if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow))
            player?.SetJumpHeld(false);

        if (Input.GetKeyDown(KeyCode.E))
            interactRequested = true;

        if (Input.GetKeyDown(KeyCode.K))
            useKeyRequested = true;
    }
}