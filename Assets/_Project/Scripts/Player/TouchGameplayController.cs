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
    private bool jumpRequested;
    private bool interactRequested;
    private bool useKeyRequested;

    private void Update()
    {
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

        if (inventory != null && !inventory.HasHouseKey)
            return "Find the house key on the left";

        if (keyManager?.currentActiveKey == null)
            return "Unlock the stuck door, then claim the Ghost Key";

        if (keyManager.currentActiveKey.abilityType == KeyManager.KeyAbilityType.GhostPhase)
            return "Push the bookshelf aside, open the sealed door, flee the Echo";

        return "Claim the Head Key and study the family portrait";
    }

    private void ReadKeyboard()
    {
        var kbMove = Input.GetAxisRaw("Horizontal");
        if (Mathf.Abs(kbMove) > 0.01f)
            moveInput = kbMove;
        else if (Input.touchCount == 0)
            moveInput = 0f;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            jumpRequested = true;

        if (Input.GetKeyDown(KeyCode.E))
            interactRequested = true;

        if (Input.GetKeyDown(KeyCode.K))
            useKeyRequested = true;
    }
}