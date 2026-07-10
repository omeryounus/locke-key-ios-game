using UnityEngine;

/// <summary>
/// Dual-path bridge: mirrors PlayerSpriteAnimator + physics into a Unity Animator
/// graph (parameters / triggers / StateId). Sprite frames stay owned by
/// PlayerSpriteAnimator; this layer is for Inspector graphs, VFX state hooks,
/// and future AnimationClip swaps without rewriting gameplay.
/// </summary>
[DefaultExecutionOrder(10)]
[RequireComponent(typeof(PlayerController))]
public class PlayerAnimatorGraphDriver : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private bool createAnimatorIfMissing = true;
    [SerializeField] private RuntimeAnimatorController controllerOverride;

    private PlayerController player;
    private PlayerSpriteAnimator spriteAnim;
    private PlayerCharacterRig rig;
    private PlayerSpriteAnimator.AnimState lastState;
    private bool lastGrounded = true;
    private bool lastGhost;
    private bool lastHide;
    private bool lastMindscape;
    private bool hasAnimator;

    public Animator Graph => animator;
    public bool HasGraph => hasAnimator && animator != null && animator.runtimeAnimatorController != null;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
        spriteAnim = GetComponent<PlayerSpriteAnimator>();
        rig = GetComponent<PlayerCharacterRig>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator == null && createAnimatorIfMissing)
            animator = gameObject.AddComponent<Animator>();

        if (controllerOverride != null && animator != null)
            animator.runtimeAnimatorController = controllerOverride;
        else if (animator != null && animator.runtimeAnimatorController == null)
        {
            var builtIn = Resources.Load<RuntimeAnimatorController>("Animation/Player/PlayerAnimGraph");
            if (builtIn != null)
                animator.runtimeAnimatorController = builtIn;
        }

        hasAnimator = animator != null;
        if (hasAnimator)
        {
            animator.applyRootMotion = false;
            animator.updateMode = AnimatorUpdateMode.Normal;
            animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
        }

        if (spriteAnim != null)
            spriteAnim.OnAnimEvent += HandleSpriteEvent;
    }

    private void OnDestroy()
    {
        if (spriteAnim != null)
            spriteAnim.OnAnimEvent -= HandleSpriteEvent;
    }

    private void LateUpdate()
    {
        if (!hasAnimator || animator == null || player == null) return;
        if (animator.runtimeAnimatorController == null) return;

        var vel = player.Velocity;
        float speed = Mathf.Abs(vel.x);
        float energy = Mathf.Clamp01(speed / Mathf.Max(0.1f, player.moveSpeed));

        animator.SetFloat(PlayerAnimParams.SpeedHash, speed);
        animator.SetFloat(PlayerAnimParams.VerticalVelocityHash, vel.y);
        animator.SetFloat(PlayerAnimParams.MoveEnergyHash, energy);
        animator.SetBool(PlayerAnimParams.GroundedHash, player.IsGrounded);
        animator.SetBool(PlayerAnimParams.WallSlidingHash, player.IsWallSliding);
        animator.SetBool(PlayerAnimParams.GhostHash, player.IsGhostPhasing);
        animator.SetBool(PlayerAnimParams.HideHash, HideSpot.IsPlayerHidden);
        animator.SetFloat(PlayerAnimParams.FacingHash, rig != null ? rig.Facing : 1f);
        animator.SetFloat(PlayerAnimParams.GhostWarningHash, player.IsGhostPhasing
            ? (player.GhostPhaseRemaining < 1f ? 1f - player.GhostPhaseRemaining : 0f)
            : 0f);

        if (spriteAnim != null)
        {
            var st = spriteAnim.State;
            animator.SetInteger(PlayerAnimParams.StateIdHash, PlayerAnimParams.ToStateId(st));
            animator.SetBool(PlayerAnimParams.MindscapeHash, st == PlayerSpriteAnimator.AnimState.Mindscape);

            // Edge triggers for graph Any-State transitions
            if (st != lastState)
            {
                FireStateEdge(lastState, st);
                lastState = st;
            }
        }

        if (player.IsGrounded && !lastGrounded)
            SafeTrigger(PlayerAnimParams.LandHash);
        if (!player.IsGrounded && lastGrounded && vel.y > 0.5f)
            SafeTrigger(PlayerAnimParams.JumpHash);

        if (player.IsGhostPhasing && !lastGhost)
            SafeTrigger(PlayerAnimParams.GhostStartHash);
        if (!player.IsGhostPhasing && lastGhost)
            SafeTrigger(PlayerAnimParams.GhostEndHash);

        bool hide = HideSpot.IsPlayerHidden;
        if (hide && !lastHide) SafeTrigger(PlayerAnimParams.HideEnterHash);
        if (!hide && lastHide) SafeTrigger(PlayerAnimParams.HideExitHash);

        lastGrounded = player.IsGrounded;
        lastGhost = player.IsGhostPhasing;
        lastHide = hide;
        lastMindscape = spriteAnim != null && spriteAnim.State == PlayerSpriteAnimator.AnimState.Mindscape;
    }

    private void FireStateEdge(PlayerSpriteAnimator.AnimState from, PlayerSpriteAnimator.AnimState to)
    {
        switch (to)
        {
            case PlayerSpriteAnimator.AnimState.Interact:
            case PlayerSpriteAnimator.AnimState.HouseKeyInteract:
                SafeTrigger(PlayerAnimParams.InteractHash);
                break;
            case PlayerSpriteAnimator.AnimState.Hit:
                SafeTrigger(PlayerAnimParams.HitHash);
                break;
            case PlayerSpriteAnimator.AnimState.ScareFlinch:
                SafeTrigger(PlayerAnimParams.ScareHash);
                break;
            case PlayerSpriteAnimator.AnimState.MirrorTravel:
                SafeTrigger(PlayerAnimParams.MirrorHash);
                break;
            case PlayerSpriteAnimator.AnimState.Land:
                SafeTrigger(PlayerAnimParams.LandHash);
                break;
            case PlayerSpriteAnimator.AnimState.JumpRise:
                SafeTrigger(PlayerAnimParams.JumpHash);
                break;
        }
    }

    private void HandleSpriteEvent(PlayerSpriteAnimator.AnimEventKind kind)
    {
        if (!hasAnimator || animator == null || animator.runtimeAnimatorController == null) return;
        if (kind == PlayerSpriteAnimator.AnimEventKind.FootPlant)
            SafeTrigger(PlayerAnimParams.FootPlantHash);
        else if (kind == PlayerSpriteAnimator.AnimEventKind.LandImpact)
            SafeTrigger(PlayerAnimParams.LandHash);
    }

    private void SafeTrigger(int hash)
    {
        if (animator == null || animator.runtimeAnimatorController == null) return;
        // Only fire when parameter exists (avoids console spam before graph is built)
        for (var i = 0; i < animator.parameterCount; i++)
        {
            var p = animator.GetParameter(i);
            if (p.nameHash == hash && p.type == AnimatorControllerParameterType.Trigger)
            {
                animator.SetTrigger(hash);
                return;
            }
        }
    }

    /// <summary>Assign a controller at runtime (e.g. after Editor generate + Resources copy).</summary>
    public void BindController(RuntimeAnimatorController controller)
    {
        if (animator == null)
            animator = gameObject.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;
        hasAnimator = true;
    }
}
