using UnityEngine;

/// <summary>
/// Shared Animator parameter names for the dual-path Player graph.
/// Runtime director remains the sprite authority; Animator mirrors state for
/// tooling, VFX hooks, and future clip swaps.
/// </summary>
public static class PlayerAnimParams
{
    public const string Speed = "Speed";
    public const string VerticalVelocity = "VerticalVelocity";
    public const string MoveEnergy = "MoveEnergy";
    public const string Grounded = "Grounded";
    public const string WallSliding = "WallSliding";
    public const string Ghost = "Ghost";
    public const string Hide = "Hide";
    public const string Mindscape = "Mindscape";
    public const string Facing = "Facing";
    public const string StateId = "StateId";
    public const string GhostWarning = "GhostWarning";

    public const string TriggerLand = "Land";
    public const string TriggerJump = "Jump";
    public const string TriggerInteract = "Interact";
    public const string TriggerHit = "Hit";
    public const string TriggerScare = "Scare";
    public const string TriggerMirror = "Mirror";
    public const string TriggerGhostStart = "GhostStart";
    public const string TriggerGhostEnd = "GhostEnd";
    public const string TriggerFootPlant = "FootPlant";
    public const string TriggerHideEnter = "HideEnter";
    public const string TriggerHideExit = "HideExit";

    public static readonly int SpeedHash = Animator.StringToHash(Speed);
    public static readonly int VerticalVelocityHash = Animator.StringToHash(VerticalVelocity);
    public static readonly int MoveEnergyHash = Animator.StringToHash(MoveEnergy);
    public static readonly int GroundedHash = Animator.StringToHash(Grounded);
    public static readonly int WallSlidingHash = Animator.StringToHash(WallSliding);
    public static readonly int GhostHash = Animator.StringToHash(Ghost);
    public static readonly int HideHash = Animator.StringToHash(Hide);
    public static readonly int MindscapeHash = Animator.StringToHash(Mindscape);
    public static readonly int FacingHash = Animator.StringToHash(Facing);
    public static readonly int StateIdHash = Animator.StringToHash(StateId);
    public static readonly int GhostWarningHash = Animator.StringToHash(GhostWarning);

    public static readonly int LandHash = Animator.StringToHash(TriggerLand);
    public static readonly int JumpHash = Animator.StringToHash(TriggerJump);
    public static readonly int InteractHash = Animator.StringToHash(TriggerInteract);
    public static readonly int HitHash = Animator.StringToHash(TriggerHit);
    public static readonly int ScareHash = Animator.StringToHash(TriggerScare);
    public static readonly int MirrorHash = Animator.StringToHash(TriggerMirror);
    public static readonly int GhostStartHash = Animator.StringToHash(TriggerGhostStart);
    public static readonly int GhostEndHash = Animator.StringToHash(TriggerGhostEnd);
    public static readonly int FootPlantHash = Animator.StringToHash(TriggerFootPlant);
    public static readonly int HideEnterHash = Animator.StringToHash(TriggerHideEnter);
    public static readonly int HideExitHash = Animator.StringToHash(TriggerHideExit);

    public static int ToStateId(PlayerSpriteAnimator.AnimState state) => (int)state;
}
