using System;
using UnityEditor;
using UnityEngine;

/// <summary>
/// CLI build script invoked by the macOS headless build command.
/// Usage:
///   Unity -quit -batchmode -projectPath . \
///         -executeMethod MacOSBuilder.Build \
///         -buildOutput /path/to/output/LockeKey.app \
///         -logFile build.log
/// </summary>
public static class MacOSBuilder
{
    public static void Build()
    {
        // Step 0: ensure TitleScreenController and GrokUIFlowManager are in their scenes
        Debug.Log("[MacOSBuilder] Patching scenes...");
        ScenePatcher.PatchTitleScene();
        ScenePatcher.PatchChapter1Scene();

        // Output path: can be overridden by -buildOutput arg
        string outputPath = GetArg("-buildOutput") ?? "Builds/macOS/LockeKey.app";

        // Gather scenes in build order
        string[] scenes = GetEnabledScenePaths();
        if (scenes.Length == 0)
        {
            Debug.LogError("[MacOSBuilder] No scenes found in Build Settings. Aborting.");
            EditorApplication.Exit(1);
            return;
        }

        Debug.Log($"[MacOSBuilder] Building {scenes.Length} scenes → {outputPath}");
        foreach (var s in scenes)
            Debug.Log($"  + {s}");

        var buildOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputPath,
            target = BuildTarget.StandaloneOSX,
            options = BuildOptions.None,
        };

        var report = BuildPipeline.BuildPlayer(buildOptions);

        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"[MacOSBuilder] ✅ Build succeeded — {report.summary.totalSize / 1024 / 1024} MB");
            EditorApplication.Exit(0);
        }
        else
        {
            Debug.LogError($"[MacOSBuilder] ❌ Build FAILED — {report.summary.totalErrors} error(s)");
            EditorApplication.Exit(1);
        }
    }

    private static string[] GetEnabledScenePaths()
    {
        var scenes = new System.Collections.Generic.List<string>();
        foreach (var scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
                scenes.Add(scene.path);
        }
        return scenes.ToArray();
    }

    private static string GetArg(string name)
    {
        var args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == name)
                return args[i + 1];
        }
        return null;
    }
}
