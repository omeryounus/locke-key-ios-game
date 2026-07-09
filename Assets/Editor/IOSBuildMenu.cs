using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Editor and CI entry point for iOS builds.
/// </summary>
public static class IOSBuildMenu
{
    private const string OutputPath = "Builds/iOS";

    [MenuItem("LockeKey/Build/iOS")]
    public static void BuildFromMenu()
    {
        BuildIOS();
    }

    /// <summary>
    /// Batch entry: Unity -batchmode -executeMethod IOSBuildMenu.BuildIOS
    /// Waits for script compile before building.
    /// </summary>
    public static void BuildIOS()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        EditorApplication.update += WaitForCompilationThenBuild;
    }

    private static void WaitForCompilationThenBuild()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            return;

        EditorApplication.update -= WaitForCompilationThenBuild;

        if (EditorUtility.scriptCompilationFailed)
        {
            Debug.LogError(
                "iOS build aborted: script compilation failed. " +
                "Open the Editor console or log file and search for 'error CS'.");
            if (Application.isBatchMode)
                EditorApplication.Exit(1);
            return;
        }

        RunBuild();
    }

    private static void RunBuild()
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

        Directory.CreateDirectory(OutputPath);

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = OutputPath,
            target = BuildTarget.iOS,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        var summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"iOS build succeeded: {summary.outputPath} ({summary.totalSize} bytes)");
            if (Application.isBatchMode)
                EditorApplication.Exit(0);
            return;
        }

        Debug.LogError($"iOS build failed: {summary.result}");
        if (Application.isBatchMode)
            EditorApplication.Exit(1);
    }
}