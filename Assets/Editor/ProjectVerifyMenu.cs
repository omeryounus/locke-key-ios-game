using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Batch-mode project verification for pre-commit / CI.
/// </summary>
public static class ProjectVerifyMenu
{
    private const string IconLibraryPath = "Assets/_Project/Resources/Art/UI/UIIconLibrary.asset";
    private const string Chapter1ScenePath = "Assets/_Project/Scenes/Chapter1/Chapter1.unity";

    [MenuItem("LockeKey/Verify/Project")]
    public static void VerifyFromMenu()
    {
        if (!RunChecks(logSuccess: true))
            EditorUtility.DisplayDialog("Project Verification", "Verification failed. See Console for details.", "OK");
    }

    /// <summary>
    /// Entry point for: Unity -batchmode -executeMethod ProjectVerifyMenu.VerifyProject
    /// </summary>
    public static void VerifyProject()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        EditorApplication.update += WaitForCompilation;
    }

    private static void WaitForCompilation()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            return;

        EditorApplication.update -= WaitForCompilation;

        bool ok = RunChecks(logSuccess: true);
        if (Application.isBatchMode)
            EditorApplication.Exit(ok ? 0 : 1);
    }

    private static bool RunChecks(bool logSuccess)
    {
        if (EditorUtility.scriptCompilationFailed)
        {
            Debug.LogError("Project verification failed: script compilation errors.");
            return false;
        }

        if (!File.Exists(IconLibraryPath))
        {
            Debug.LogError($"Project verification failed: missing {IconLibraryPath}");
            return false;
        }

        var iconLibrary = AssetDatabase.LoadAssetAtPath<ScriptableObject>(IconLibraryPath);
        if (iconLibrary == null)
        {
            Debug.LogError($"Project verification failed: could not load {IconLibraryPath} (check .meta GUID).");
            return false;
        }

        if (!File.Exists(Chapter1ScenePath))
        {
            Debug.LogError($"Project verification failed: missing {Chapter1ScenePath}");
            return false;
        }

        var enabledScenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();
        if (enabledScenes.Length == 0 || !enabledScenes.Contains(Chapter1ScenePath))
        {
            Debug.LogError("Project verification failed: Chapter1 scene is not enabled in Build Settings.");
            return false;
        }

        if (logSuccess)
            Debug.Log("Project verification passed: compile OK, UIIconLibrary loaded, Chapter1 in build settings.");
        return true;
    }

    [MenuItem("LockeKey/Verify/iOS Build")]
    public static void VerifyIOSBuildFromMenu()
    {
        if (!RunIOSBuild())
            EditorUtility.DisplayDialog("iOS Build Verification", "iOS build failed. See Console for details.", "OK");
    }

    /// <summary>
    /// Entry point for: Unity -batchmode -executeMethod ProjectVerifyMenu.VerifyIOSBuild
    /// </summary>
    public static void VerifyIOSBuild()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        EditorApplication.update += WaitForCompilationThenBuild;
    }

    private static void WaitForCompilationThenBuild()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            return;

        EditorApplication.update -= WaitForCompilationThenBuild;

        if (!RunChecks(logSuccess: false))
        {
            if (Application.isBatchMode)
                EditorApplication.Exit(1);
            return;
        }

        bool ok = RunIOSBuild();
        if (Application.isBatchMode)
            EditorApplication.Exit(ok ? 0 : 1);
    }

    private static bool RunIOSBuild()
    {
        const string outputPath = "Builds/iOS";
        var scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        Directory.CreateDirectory(outputPath);

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputPath,
            target = BuildTarget.iOS,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"iOS build verification succeeded: {report.summary.outputPath}");
            return true;
        }

        Debug.LogError($"iOS build verification failed: {report.summary.result}");
        return false;
    }
}