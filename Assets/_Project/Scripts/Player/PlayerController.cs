using System;
using UnityEngine;
#if UNITY_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Modern tap-to-move locomotion controller optimized for mobile portrait/landscape gameplay.
/// Manages destination-based movement, sprite orientation, dynamic shadow scaling, and animation triggers.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Locomotion Settings")]
    [SerializeField] private float speed = 5.6f;
    [SerializeField] private float arrivalThreshold = 0.05f;

    [Header("Ground Contact Shadow")]
    [SerializeField] private SpriteRenderer groundShadow;
    [SerializeField] private Vector2 shadowScaleMoving = new Vector2(1f, 0.5f);
    [SerializeField] private Vector2 shadowScaleStationary = new Vector2(1.2f, 0.3f);
    [SerializeField] private float shadowLerpSpeed = 10f;

    [Header("Animator Integration")]
    [SerializeField] private Animator animator;

    // Movement state
    private float targetX;
    private bool hasTarget;
    private bool isWalking;
    private bool facingRight = true;

    // Legacy fields/properties to maintain compilation across existing scripts
    public float moveSpeed => speed;
    public bool IsGhostPhasing { get; set; }
    public bool IsGrounded => true;
    public bool IsWallSliding => false;
    public bool JumpHeld => false;
    public float jumpForce => 0f;
    public float maxFallSpeed => 0f;
    public float GhostPhaseRemaining { get; set; }
    public bool AnimationDrivesFootsteps { get; set; }
    public float MoveInput => isWalking ? (facingRight ? 1f : -1f) : 0f;
    public Vector2 Velocity => new Vector2(isWalking ? (facingRight ? speed : -speed) : 0f, 0f);
    public float HorizontalSpeed => isWalking ? speed : 0f;
    public int WallSign => 0;
    public bool JustLanded => false;

    public void ActivateGhostPhase(float duration)
    {
        var ability = GetComponent<GhostKeyAbility>();
        if (ability != null)
        {
            ability.ActivateGhostForm();
        }
        else
        {
            IsGhostPhasing = true;
            GhostPhaseRemaining = duration;
        }
    }

    public bool TryMirrorTravel() => false;
    public void ManipulateShadows(bool active = true) {}
    public void Move(float amount)
    {
        if (Mathf.Abs(amount) > 0.01f)
        {
            targetX = transform.position.x + amount * speed * Time.deltaTime;
            hasTarget = true;
        }
    }
    public void Jump() {}
    public void SetJumpHeld(bool held) {}


    /// <summary>
    /// Prevents the player from moving when set to true (e.g., during active dialogue or interactions).
    /// </summary>
    public bool IsInteracting { get; set; }

    private void Awake()
    {
        // Target high-performance 60fps refresh rate on iOS devices
        Application.targetFrameRate = 60;
        
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        targetX = transform.position.x;
    }

    private void Update()
    {
        // 1. Process Input
        ProcessTouchInput();

        // 2. Perform Movement
        if (hasTarget && !IsInteracting)
        {
            MoveTowardsTarget();
        }
        else
        {
            StopMoving();
        }

        // 3. Update Visuals
        UpdateShadowScale();
    }

    /// <summary>
    /// Processes touch or click inputs to set the horizontal movement target destination.
    /// Supports both mouse clicks in Editor and touch events on iOS hardware.
    /// </summary>
    private void ProcessTouchInput()
    {
#if UNITY_INPUT_SYSTEM
        // New Input System direct touch/press sensing
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            Vector2 screenPos = Touchscreen.current.primaryTouch.position.ReadValue();
            SetTargetDestination(screenPos);
        }
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 screenPos = Mouse.current.position.ReadValue();
            SetTargetDestination(screenPos);
        }
#else
        // Fallback for legacy input configurations
        if (Input.GetMouseButtonDown(0))
        {
            SetTargetDestination(Input.mousePosition);
        }
#endif
    }

    /// <summary>
    /// Projects a screen position into the 2D world coordinates to establish a walk target.
    /// </summary>
    /// <param name="screenPos">The screen coordinates of the touch or mouse click event.</param>
    private void SetTargetDestination(Vector2 screenPos)
    {
        if (Camera.main == null) return;

        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(screenPos);
        targetX = worldPoint.x;
        hasTarget = true;
    }

    /// <summary>
    /// Interpolates the character position towards the horizontal target X position.
    /// </summary>
    private void MoveTowardsTarget()
    {
        float currentX = transform.position.x;
        float diff = targetX - currentX;

        if (Mathf.Abs(diff) <= arrivalThreshold)
        {
            StopMoving();
            return;
        }

        isWalking = true;
        
        // Face travel direction
        facingRight = diff > 0;
        transform.localScale = new Vector3(facingRight ? 1f : -1f, 1f, 1f);

        // MoveTowards coordinates
        float newX = Mathf.MoveTowards(currentX, targetX, speed * Time.deltaTime);
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);

        // Update Animator parameters
        if (animator != null)
        {
            animator.SetBool("IsWalking", true);
            animator.SetFloat("Speed", 1f);
        }
    }

    /// <summary>
    /// Immediately halts the walking state and updates animations.
    /// </summary>
    private void StopMoving()
    {
        isWalking = false;
        hasTarget = false;

        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
            animator.SetFloat("Speed", 0f);
        }
    }

    /// <summary>
    /// Smoothly interpolates the scale of the ground contact shadow based on the current movement state.
    /// </summary>
    private void UpdateShadowScale()
    {
        if (groundShadow == null) return;

        Vector2 targetScale = isWalking ? shadowScaleMoving : shadowScaleStationary;
        Vector3 currentScale = groundShadow.transform.localScale;
        
        groundShadow.transform.localScale = Vector3.Lerp(
            currentScale, 
            new Vector3(targetScale.x, targetScale.y, 1f), 
            Time.deltaTime * shadowLerpSpeed
        );
    }

    /// <summary>
    /// Dummy compatibility method to prevent compilation issues with VFX scripts.
    /// </summary>
    /// <param name="radius">The radius of the footstep noise sphere.</param>
    public void EmitFootstepNoise(float radius)
    {
        // Objective / noise system hooks
    }
}
