using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// Builds the dual-path Player AnimatorController graph:
/// parameters mirror PlayerAnimParams; states map to PlayerSpriteAnimator.AnimState.
/// Runtime sprite frames stay on PlayerSpriteAnimator — this graph is for tooling,
/// VFX hooks, and future AnimationClip assignment.
/// </summary>
public static class PlayerAnimatorControllerBuilder
{
    private const string ControllerPath = "Assets/_Project/Animation/Player/PlayerAnimGraph.controller";
    private const string ResourcesCopyPath = "Assets/_Project/Resources/Animation/Player/PlayerAnimGraph.controller";

    [MenuItem("LockeKey/Animation/Build Player Animator Graph")]
    public static void BuildFromMenu()
    {
        var controller = Build();
        EditorUtility.DisplayDialog(
            "Player Animator Graph",
            controller != null
                ? $"Built:\n{ControllerPath}\n\nAlso copied to Resources for runtime load."
                : "Failed to build controller.",
            "OK");
    }

    /// <summary>Batch-mode entry: Unity -batchmode -executeMethod PlayerAnimatorControllerBuilder.BuildBatch</summary>
    public static void BuildBatch()
    {
        Build();
        if (Application.isBatchMode)
            EditorApplication.Exit(0);
    }

    public static AnimatorController Build()
    {
        EnsureFolders();

        if (File.Exists(ControllerPath))
            AssetDatabase.DeleteAsset(ControllerPath);

        var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        AddParameters(controller);
        BuildBaseLayer(controller);
        BuildAbilityLayer(controller);

        // Resources copy for runtime Bind without scene wiring
        EnsureFolder("Assets/_Project/Resources");
        EnsureFolder("Assets/_Project/Resources/Animation");
        EnsureFolder("Assets/_Project/Resources/Animation/Player");
        AssetDatabase.CopyAsset(ControllerPath, ResourcesCopyPath);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[PlayerAnimatorControllerBuilder] Built {ControllerPath}");
        return controller;
    }

    private static void AddParameters(AnimatorController c)
    {
        c.AddParameter(PlayerAnimParams.Speed, AnimatorControllerParameterType.Float);
        c.AddParameter(PlayerAnimParams.VerticalVelocity, AnimatorControllerParameterType.Float);
        c.AddParameter(PlayerAnimParams.MoveEnergy, AnimatorControllerParameterType.Float);
        c.AddParameter(PlayerAnimParams.Facing, AnimatorControllerParameterType.Float);
        c.AddParameter(PlayerAnimParams.GhostWarning, AnimatorControllerParameterType.Float);
        c.AddParameter(PlayerAnimParams.Grounded, AnimatorControllerParameterType.Bool);
        c.AddParameter(PlayerAnimParams.WallSliding, AnimatorControllerParameterType.Bool);
        c.AddParameter(PlayerAnimParams.Ghost, AnimatorControllerParameterType.Bool);
        c.AddParameter(PlayerAnimParams.Hide, AnimatorControllerParameterType.Bool);
        c.AddParameter(PlayerAnimParams.Mindscape, AnimatorControllerParameterType.Bool);
        c.AddParameter(PlayerAnimParams.StateId, AnimatorControllerParameterType.Int);

        c.AddParameter(PlayerAnimParams.TriggerLand, AnimatorControllerParameterType.Trigger);
        c.AddParameter(PlayerAnimParams.TriggerJump, AnimatorControllerParameterType.Trigger);
        c.AddParameter(PlayerAnimParams.TriggerInteract, AnimatorControllerParameterType.Trigger);
        c.AddParameter(PlayerAnimParams.TriggerHit, AnimatorControllerParameterType.Trigger);
        c.AddParameter(PlayerAnimParams.TriggerScare, AnimatorControllerParameterType.Trigger);
        c.AddParameter(PlayerAnimParams.TriggerMirror, AnimatorControllerParameterType.Trigger);
        c.AddParameter(PlayerAnimParams.TriggerGhostStart, AnimatorControllerParameterType.Trigger);
        c.AddParameter(PlayerAnimParams.TriggerGhostEnd, AnimatorControllerParameterType.Trigger);
        c.AddParameter(PlayerAnimParams.TriggerFootPlant, AnimatorControllerParameterType.Trigger);
        c.AddParameter(PlayerAnimParams.TriggerHideEnter, AnimatorControllerParameterType.Trigger);
        c.AddParameter(PlayerAnimParams.TriggerHideExit, AnimatorControllerParameterType.Trigger);
    }

    private static void BuildBaseLayer(AnimatorController c)
    {
        var sm = c.layers[0].stateMachine;
        sm.name = "Locomotion";

        var idle = AddEmptyState(sm, "Idle", new Vector3(300, 0, 0));
        var walk = AddEmptyState(sm, "Walk", new Vector3(300, 80, 0));
        var run = AddEmptyState(sm, "Run", new Vector3(300, 160, 0));
        var jump = AddEmptyState(sm, "JumpRise", new Vector3(550, -40, 0));
        var apex = AddEmptyState(sm, "JumpApex", new Vector3(550, 40, 0));
        var fall = AddEmptyState(sm, "Fall", new Vector3(550, 120, 0));
        var land = AddEmptyState(sm, "Land", new Vector3(550, 200, 0));
        var wall = AddEmptyState(sm, "WallSlide", new Vector3(800, 80, 0));
        var turn = AddEmptyState(sm, "Turn", new Vector3(80, 80, 0));

        sm.defaultState = idle;

        // Locomotion blend by Speed + Grounded
        Transition(idle, walk, t =>
        {
            t.AddCondition(AnimatorConditionMode.Greater, 0.12f, PlayerAnimParams.Speed);
            t.AddCondition(AnimatorConditionMode.If, 0, PlayerAnimParams.Grounded);
        });
        Transition(walk, run, t =>
        {
            t.AddCondition(AnimatorConditionMode.Greater, 3.5f, PlayerAnimParams.Speed);
            t.AddCondition(AnimatorConditionMode.If, 0, PlayerAnimParams.Grounded);
        });
        Transition(run, walk, t =>
        {
            t.AddCondition(AnimatorConditionMode.Less, 3.4f, PlayerAnimParams.Speed);
            t.AddCondition(AnimatorConditionMode.If, 0, PlayerAnimParams.Grounded);
        });
        Transition(walk, idle, t =>
        {
            t.AddCondition(AnimatorConditionMode.Less, 0.12f, PlayerAnimParams.Speed);
            t.AddCondition(AnimatorConditionMode.If, 0, PlayerAnimParams.Grounded);
        });
        Transition(run, idle, t =>
        {
            t.AddCondition(AnimatorConditionMode.Less, 0.12f, PlayerAnimParams.Speed);
            t.AddCondition(AnimatorConditionMode.If, 0, PlayerAnimParams.Grounded);
        });

        // Air
        AnyState(sm, jump, t => t.AddCondition(AnimatorConditionMode.If, 0, PlayerAnimParams.TriggerJump));
        Transition(jump, apex, t => t.AddCondition(AnimatorConditionMode.Less, 1.2f, PlayerAnimParams.VerticalVelocity));
        Transition(apex, fall, t => t.AddCondition(AnimatorConditionMode.Less, -0.35f, PlayerAnimParams.VerticalVelocity));
        Transition(fall, land, t => t.AddCondition(AnimatorConditionMode.If, 0, PlayerAnimParams.Grounded));
        AnyState(sm, land, t => t.AddCondition(AnimatorConditionMode.If, 0, PlayerAnimParams.TriggerLand));
        Transition(land, idle, t =>
        {
            t.hasExitTime = true;
            t.exitTime = 0.85f;
            t.duration = 0.05f;
        });

        // Wall slide
        Transition(fall, wall, t => t.AddCondition(AnimatorConditionMode.If, 0, PlayerAnimParams.WallSliding));
        Transition(wall, fall, t => t.AddCondition(AnimatorConditionMode.IfNot, 0, PlayerAnimParams.WallSliding));
        Transition(wall, land, t => t.AddCondition(AnimatorConditionMode.If, 0, PlayerAnimParams.Grounded));

        // Turn (short)
        AnyState(sm, turn, t =>
        {
            t.AddCondition(AnimatorConditionMode.Equals, (int)PlayerSpriteAnimator.AnimState.Turn, PlayerAnimParams.StateId);
        });
        Transition(turn, idle, t =>
        {
            t.hasExitTime = true;
            t.exitTime = 0.9f;
        });
    }

    private static void BuildAbilityLayer(AnimatorController c)
    {
        c.AddLayer("Ability");
        var layers = c.layers;
        var ability = layers[layers.Length - 1];
        ability.defaultWeight = 1f;
        ability.blendingMode = AnimatorLayerBlendingMode.Override;
        c.layers = layers;

        var sm = c.layers[c.layers.Length - 1].stateMachine;
        var empty = AddEmptyState(sm, "AbilityNone", new Vector3(200, 0, 0));
        var ghost = AddEmptyState(sm, "GhostPhase", new Vector3(200, 100, 0));
        var hide = AddEmptyState(sm, "Hide", new Vector3(200, 200, 0));
        var mind = AddEmptyState(sm, "Mindscape", new Vector3(450, 100, 0));
        var scare = AddEmptyState(sm, "Scare", new Vector3(450, 200, 0));
        var hit = AddEmptyState(sm, "Hit", new Vector3(450, 0, 0));
        var interact = AddEmptyState(sm, "Interact", new Vector3(700, 100, 0));
        var mirror = AddEmptyState(sm, "MirrorTravel", new Vector3(700, 200, 0));
        sm.defaultState = empty;

        AnyState(sm, ghost, t => t.AddCondition(AnimatorConditionMode.If, 0, PlayerAnimParams.TriggerGhostStart));
        Transition(ghost, empty, t => t.AddCondition(AnimatorConditionMode.If, 0, PlayerAnimParams.TriggerGhostEnd));
        Transition(ghost, empty, t => t.AddCondition(AnimatorConditionMode.IfNot, 0, PlayerAnimParams.Ghost));

        AnyState(sm, hide, t => t.AddCondition(AnimatorConditionMode.If, 0, PlayerAnimParams.TriggerHideEnter));
        Transition(hide, empty, t => t.AddCondition(AnimatorConditionMode.If, 0, PlayerAnimParams.TriggerHideExit));
        Transition(hide, empty, t => t.AddCondition(AnimatorConditionMode.IfNot, 0, PlayerAnimParams.Hide));

        AnyState(sm, mind, t => t.AddCondition(AnimatorConditionMode.If, 0, PlayerAnimParams.Mindscape));
        Transition(mind, empty, t => t.AddCondition(AnimatorConditionMode.IfNot, 0, PlayerAnimParams.Mindscape));

        AnyState(sm, scare, t => t.AddCondition(AnimatorConditionMode.If, 0, PlayerAnimParams.TriggerScare));
        Transition(scare, empty, t =>
        {
            t.hasExitTime = true;
            t.exitTime = 0.95f;
        });

        AnyState(sm, hit, t => t.AddCondition(AnimatorConditionMode.If, 0, PlayerAnimParams.TriggerHit));
        Transition(hit, empty, t =>
        {
            t.hasExitTime = true;
            t.exitTime = 0.9f;
        });

        AnyState(sm, interact, t => t.AddCondition(AnimatorConditionMode.If, 0, PlayerAnimParams.TriggerInteract));
        Transition(interact, empty, t =>
        {
            t.hasExitTime = true;
            t.exitTime = 0.9f;
        });

        AnyState(sm, mirror, t => t.AddCondition(AnimatorConditionMode.If, 0, PlayerAnimParams.TriggerMirror));
        Transition(mirror, empty, t =>
        {
            t.hasExitTime = true;
            t.exitTime = 0.85f;
        });

        // FootPlant pulse for VFX / secondary state machines
        var foot = AddEmptyState(sm, "FootPlantPulse", new Vector3(200, 300, 0));
        AnyState(sm, foot, t => t.AddCondition(AnimatorConditionMode.If, 0, PlayerAnimParams.TriggerFootPlant));
        Transition(foot, empty, t =>
        {
            t.hasExitTime = true;
            t.exitTime = 0.05f;
            t.duration = 0f;
        });
    }

    private static AnimatorState AddEmptyState(AnimatorStateMachine sm, string name, Vector3 pos)
    {
        var state = sm.AddState(name, pos);
        state.writeDefaultValues = true;
        // Empty motion — sprite director owns frames; clips can be assigned later
        state.motion = null;
        return state;
    }

    private static void Transition(AnimatorState from, AnimatorState to, System.Action<AnimatorStateTransition> configure)
    {
        var t = from.AddTransition(to);
        t.hasExitTime = false;
        t.duration = 0.08f;
        t.canTransitionToSelf = false;
        configure?.Invoke(t);
    }

    private static void AnyState(AnimatorStateMachine sm, AnimatorState to, System.Action<AnimatorStateTransition> configure)
    {
        var t = sm.AddAnyStateTransition(to);
        t.hasExitTime = false;
        t.duration = 0.06f;
        t.canTransitionToSelf = false;
        configure?.Invoke(t);
    }

    private static void EnsureFolders()
    {
        EnsureFolder("Assets/_Project");
        EnsureFolder("Assets/_Project/Animation");
        EnsureFolder("Assets/_Project/Animation/Player");
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        var parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
        var name = Path.GetFileName(path);
        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            EnsureFolder(parent);
        if (!string.IsNullOrEmpty(parent))
            AssetDatabase.CreateFolder(parent, name);
    }
}
