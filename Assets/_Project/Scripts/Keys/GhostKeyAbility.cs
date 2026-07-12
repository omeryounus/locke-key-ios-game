using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Implements the core Ghost Key mechanics, managing gesture detection,
/// soul-body separation, timer depletion, body vulnerability, and events.
/// </summary>
public class GhostKeyAbility : MonoBehaviour
{
    [Header("Ability Constants")]
    [SerializeField] private float ghostDuration = 45f;
    [SerializeField] private float returnMergeDistance = 0.5f;
    [SerializeField] private float swipeThreshold = 150f;
    [SerializeField] private float bodyCapturedReturnGraceTime = 10f;

    [Header("Entity References")]
    [SerializeField] private GameObject ghostPrefab;
    [SerializeField] private GameObject bodyVFXPrefab;
    [SerializeField] private GameObject mergeVFXPrefab;
    
    [Header("UI & Visuals")]
    [SerializeField] private Image uiCooldownRing;
    [SerializeField] private GameObject ghostKeyUIPanel;
    [SerializeField] private Behaviour ghostPostProcessVolume;

    [Header("Events")]
    public UnityEvent OnGhostActivate;
    public UnityEvent OnGhostReturn;
    public UnityEvent OnBodyCaptured;

    private PlayerController playerController;
    private Rigidbody2D playerRigidbody;
    private Animator playerAnimator;
    
    private GameObject activeGhostInstance;
    private bool isGhostActive;
    private float ghostTimer;
    private bool isReturning;
    
    // Swipe state
    private Vector2 swipeStartPos;
    private bool isSwiping;

    // Body vulnerability state
    private bool bodyCaptured;
    private float bodyCapturedTimer;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        playerRigidbody = GetComponent<Rigidbody2D>();
        playerAnimator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (!isGhostActive)
        {
            DetectSwipeGesture();
        }
        else
        {
            UpdateGhostTimer();
            if (isReturning)
            {
                PerformGhostReturnMerge();
            }
        }
    }

    /// <summary>
    /// Listens for a rapid swipe-up gesture on mobile screens to trigger soul separation.
    /// </summary>
    private void DetectSwipeGesture()
    {
        // 1. Mobile touch swipe gesture
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                swipeStartPos = touch.position;
                isSwiping = true;
            }
            else if (isSwiping && touch.phase == TouchPhase.Moved)
            {
                float deltaY = touch.position.y - swipeStartPos.y;
                if (deltaY > swipeThreshold)
                {
                    ActivateGhostForm();
                    isSwiping = false;
                }
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isSwiping = false;
            }
        }
        // 2. Editor/Desktop mouse drag fallback (left click or right click drag up)
        else
        {
            if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(0))
            {
                swipeStartPos = Input.mousePosition;
                isSwiping = true;
            }
            else if (isSwiping && (Input.GetMouseButton(1) || Input.GetMouseButton(0)))
            {
                float deltaY = Input.mousePosition.y - swipeStartPos.y;
                if (deltaY > swipeThreshold)
                {
                    ActivateGhostForm();
                    isSwiping = false;
                }
            }
            else if (Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(0))
            {
                isSwiping = false;
            }
        }
    }

    /// <summary>
    /// Detaches the player's soul from their body, instantiating the floating ghost prefab.
    /// </summary>
    public void ActivateGhostForm()
    {
        if (isGhostActive) return;

        isGhostActive = true;
        ghostTimer = ghostDuration;
        bodyCaptured = false;

        // 1. Spawn VFX
        if (bodyVFXPrefab != null)
        {
            Instantiate(bodyVFXPrefab, transform.position, Quaternion.identity);
        }

        // 2. Spawn Ghost Instance
        if (ghostPrefab != null)
        {
            activeGhostInstance = Instantiate(ghostPrefab, transform.position, Quaternion.identity);
            
            // Set up collision filters for ghost
            var ghostRb = activeGhostInstance.GetComponent<Rigidbody2D>();
            if (ghostRb != null)
            {
                // Disable ground collision for floating
                int defaultLayer = LayerMask.NameToLayer("Default");
                int groundLayer = LayerMask.NameToLayer("Ground");
                int ghostWallLayer = LayerMask.NameToLayer("GhostWall");
                int ghostPassableLayer = LayerMask.NameToLayer("GhostPassable");

                // Note: Ghost movement component handles ignoring Default/Ground layers at runtime
            }
        }

        // 3. Pause player controller physics and input
        if (playerRigidbody != null)
        {
            playerRigidbody.simulated = false;
        }
        
        if (playerController != null)
        {
            playerController.IsGhostPhasing = true;
            playerController.IsInteracting = true; // Stops body movement
        }

        // 4. Trigger Animations & VFX Post Process
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("IsGhost", true);
        }

        if (ghostPostProcessVolume != null)
        {
            ghostPostProcessVolume.enabled = true; // Fully desaturate/blue-tint
        }

        if (ghostKeyUIPanel != null)
        {
            ghostKeyUIPanel.SetActive(true);
        }

        OnGhostActivate?.Invoke();
    }

    /// <summary>
    /// Called when the UI return button is clicked or timer expires. Initiates return merge transition.
    /// </summary>
    public void ReturnToBody()
    {
        if (!isGhostActive || isReturning) return;
        isReturning = true;
    }

    /// <summary>
    /// Tracks ghost timer depletion and controls UI cooldown ring fills.
    /// </summary>
    private void UpdateGhostTimer()
    {
        if (isReturning) return;

        // Deplete normal timer
        ghostTimer = Mathf.Max(0f, ghostTimer - Time.deltaTime);

        if (uiCooldownRing != null)
        {
            uiCooldownRing.fillAmount = ghostTimer / ghostDuration;
        }

        // Handle body vulnerability timer if captured
        if (bodyCaptured)
        {
            bodyCapturedTimer -= Time.deltaTime;
            if (bodyCapturedTimer <= 0f)
            {
                LoseGameProgress();
            }
        }

        // Automatically trigger return when timer hits zero
        if (ghostTimer <= 0f)
        {
            ReturnToBody();
        }
    }

    /// <summary>
    /// Moves the ghost back towards the physical body. Merges them together upon arrival.
    /// </summary>
    private void PerformGhostReturnMerge()
    {
        if (activeGhostInstance == null)
        {
            CompleteMerge();
            return;
        }

        // Pull the ghost towards the physical body position
        Vector3 bodyPos = transform.position;
        Vector3 ghostPos = activeGhostInstance.transform.position;

        activeGhostInstance.transform.position = Vector3.MoveTowards(
            ghostPos, 
            bodyPos, 
            12f * Time.deltaTime
        );

        if (Vector3.Distance(activeGhostInstance.transform.position, bodyPos) <= returnMergeDistance)
        {
            CompleteMerge();
        }
    }

    /// <summary>
    /// Destroys the ghost prefab, restores normal controller input, and triggers clean merge VFX.
    /// </summary>
    private void CompleteMerge()
    {
        isGhostActive = false;
        isReturning = false;
        bodyCaptured = false;

        if (activeGhostInstance != null)
        {
            Destroy(activeGhostInstance);
        }

        // Spawn merge particles
        if (mergeVFXPrefab != null)
        {
            Instantiate(mergeVFXPrefab, transform.position, Quaternion.identity);
        }

        // Restore normal player movement
        if (playerRigidbody != null)
        {
            playerRigidbody.simulated = true;
        }

        if (playerController != null)
        {
            playerController.IsGhostPhasing = false;
            playerController.IsInteracting = false;
        }

        if (playerAnimator != null)
        {
            playerAnimator.SetBool("IsGhost", false);
        }

        if (ghostPostProcessVolume != null)
        {
            ghostPostProcessVolume.enabled = false;
        }

        if (ghostKeyUIPanel != null)
        {
            ghostKeyUIPanel.SetActive(false);
        }

        OnGhostReturn?.Invoke();
    }

    /// <summary>
    /// Triggers the vulnerability penalty when an enemy collides with the player's physical body.
    /// Gives the player 10 seconds to merge before failing.
    /// </summary>
    public void TriggerBodyCapture()
    {
        if (!isGhostActive || bodyCaptured) return;

        bodyCaptured = true;
        bodyCapturedTimer = bodyCapturedReturnGraceTime;
        
        OnBodyCaptured?.Invoke();
    }

    /// <summary>
    /// Reloads the scene or resets save progress if the timer runs out after capture.
    /// </summary>
    private void LoseGameProgress()
    {
        isGhostActive = false;
        // Reload current chapter from the last checkpoint
        ChapterSaveManager.ReloadChapterScene();
    }
}
