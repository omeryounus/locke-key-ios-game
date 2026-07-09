using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Ensures TitleScreenController is on the TitleScreen scene's root GameObject
/// and GrokUIFlowManager is on a persistent root in Chapter1, at build time and
/// whenever either scene is opened in the Editor.
/// Run via: LockeKey → Scene Setup → Patch Scenes
/// Also called automatically by MacOSBuilder before building.
/// </summary>
[InitializeOnLoad]
public static class ScenePatcher
{
    static ScenePatcher()
    {
        EditorApplication.delayCall += RunIfNeeded;
    }

    private static void RunIfNeeded()
    {
        // Only auto-patch in Editor when not already patched (avoids every-recompile noise)
        PatchTitleScene();
        PatchChapter1Scene();
    }

    [MenuItem("LockeKey/Scene Setup/Patch Scenes")]
    public static void PatchAll()
    {
        PatchTitleScene();
        PatchChapter1Scene();
        Debug.Log("[ScenePatcher] ✅ Both scenes patched.");
    }

    // ── TitleScreen ──────────────────────────────────────────────────────

    public static void PatchTitleScene()
    {
        const string scenePath = "Assets/_Project/Scenes/TitleScreen/TitleScreen.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

        bool dirty = false;

        // Single host for TitleScreenController (remove legacy duplicate on TitleScreen GO).
        var host = FindOrCreateGO(scene, "TitleScreenHost");
        dirty |= EnsureComponent<TitleScreenController>(host);
        dirty |= RemoveDuplicateTitleControllers(scene, host);

        // Ensure Camera exists with correct settings
        var cam = EnsureCamera(scene);

        if (dirty || cam != null)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        EditorSceneManager.CloseScene(scene, removeScene: true);
        Debug.Log("[ScenePatcher] TitleScreen patched.");
    }

    // ── Chapter1 ─────────────────────────────────────────────────────────

    public static void PatchChapter1Scene()
    {
        const string scenePath = "Assets/_Project/Scenes/Chapter1/Chapter1.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

        bool dirty = false;

        // GrokUIFlowManager — one singleton host
        var flowHost = FindOrCreateGO(scene, "GrokUIFlowManager");
        dirty |= EnsureComponent<GrokUIFlowManager>(flowHost);

        dirty |= EnsureGameplayCamera(scene);

        if (dirty)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        EditorSceneManager.CloseScene(scene, removeScene: true);
        Debug.Log("[ScenePatcher] Chapter1 patched.");
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private static GameObject FindOrCreateGO(Scene scene, string name)
    {
        foreach (var root in scene.GetRootGameObjects())
            if (root.name == name) return root;

        var go = new GameObject(name);
        SceneManager.MoveGameObjectToScene(go, scene);
        return go;
    }

    private static bool EnsureComponent<T>(GameObject go) where T : Component
    {
        if (go.GetComponent<T>() != null) return false;
        go.AddComponent<T>();
        Debug.Log($"[ScenePatcher] Added {typeof(T).Name} to '{go.name}'");
        return true;
    }

    private static bool RemoveDuplicateTitleControllers(Scene scene, GameObject host)
    {
        bool dirty = false;
        foreach (var root in scene.GetRootGameObjects())
        {
            if (root == host) continue;
            var dup = root.GetComponent<TitleScreenController>();
            if (dup == null) continue;
            Object.DestroyImmediate(dup);
            Debug.Log($"[ScenePatcher] Removed duplicate TitleScreenController from '{root.name}'");
            dirty = true;
        }

        return dirty;
    }

    private static bool EnsureGameplayCamera(Scene scene)
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            var cam = root.GetComponentInChildren<Camera>();
            if (cam == null) continue;
            if (cam.GetComponent<GameplayViewportCamera>() != null) return false;
            cam.gameObject.AddComponent<GameplayViewportCamera>();
            Debug.Log("[ScenePatcher] Added GameplayViewportCamera to Main Camera");
            return true;
        }

        return false;
    }

    private static Camera EnsureCamera(Scene scene)
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            var cam = root.GetComponentInChildren<Camera>();
            if (cam != null) return null; // already exists, no change needed
        }

        // No camera — create one
        var camGo = new GameObject("Main Camera");
        SceneManager.MoveGameObjectToScene(camGo, scene);
        camGo.tag = "MainCamera";
        var c = camGo.AddComponent<Camera>();
        c.clearFlags = CameraClearFlags.SolidColor;
        c.backgroundColor = Color.black;
        c.orthographic = true;
        camGo.AddComponent<AudioListener>();
        Debug.Log("[ScenePatcher] Created Main Camera in TitleScreen");
        return c;
    }
}
