using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Editor entry point to build the macOS standalone target.
/// </summary>
public static class MacBuildMenu
{
    private const string OutputPath = "Builds/Mac/LockeKey.app";

    [MenuItem("LockeKey/Build/Mac Standalone")]
    public static void BuildMac()
    {
        var scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        if (scenes.Length == 0)
        {
            Debug.LogError("No enabled scenes in Build Settings.");
            if (Application.isBatchMode)
                EditorApplication.Exit(1);
            return;
        }

        var dir = Path.GetDirectoryName(OutputPath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = OutputPath,
            target = BuildTarget.StandaloneOSX,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        var summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"Mac build succeeded: {summary.outputPath} ({summary.totalSize} bytes)");
            if (Application.isBatchMode)
                EditorApplication.Exit(0);
            return;
        }

        Debug.LogError($"Mac build failed: {summary.result}");
        if (Application.isBatchMode)
            EditorApplication.Exit(1);
    }
}
