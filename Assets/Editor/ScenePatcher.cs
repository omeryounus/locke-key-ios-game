using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;

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

        // Setup the Parallax Background
        dirty |= SetupParallaxBackground(scene);

        // Setup the Hidden Key Puzzle
        dirty |= SetupHiddenKeyPuzzle(scene);

        if (dirty)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        EditorSceneManager.CloseScene(scene, removeScene: true);
        Debug.Log("[ScenePatcher] Chapter1 patched.");
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private static bool SetupParallaxBackground(Scene scene)
    {
        bool dirty = false;

        // 1. Find or create the controller object
        var controllerGo = FindOrCreateGO(scene, "ParallaxController");
        var parallax = controllerGo.GetComponent<ParallaxBackground>();
        if (parallax == null)
        {
            parallax = controllerGo.AddComponent<ParallaxBackground>();
            dirty = true;
        }

        // 2. Locate or create camera reference
        var camGo = GameObject.FindWithTag("MainCamera");
        Transform camTrans = camGo != null ? camGo.transform : null;

        // 3. Find parent GameObjects for the 4 layers in the scene
        GameObject groundGo = null;
        GameObject farGo = null;
        GameObject midGo = null;
        GameObject nearGo = null;

        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.name == "Ground") groundGo = root;
            else if (root.name == "ParallaxFar") farGo = root;
            else if (root.name == "ParallaxMid") midGo = root;
            else if (root.name == "ParallaxNear") nearGo = root;
        }

        if (groundGo == null) groundGo = FindOrCreateGO(scene, "Ground");
        if (farGo == null) farGo = FindOrCreateGO(scene, "ParallaxFar");
        if (midGo == null) midGo = FindOrCreateGO(scene, "ParallaxMid");
        if (nearGo == null) nearGo = FindOrCreateGO(scene, "ParallaxNear");

        // 4. Load the sprites from the Assets folder
        Sprite farSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Parallax/foyer_far.png");
        Sprite midSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Parallax/foyer_mid.png");
        Sprite nearSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Parallax/foyer_near.png");

        // 5. Gather current SpriteRenderer properties from Ground to preserve them
        var groundSR = groundGo.GetComponent<SpriteRenderer>();
        Sprite groundSprite = groundSR != null ? groundSR.sprite : null;
        Color groundColor = groundSR != null ? groundSR.color : new Color(0.18f, 0.16f, 0.22f, 1f);
        int groundSortingOrder = groundSR != null ? groundSR.sortingOrder : 0;
        int groundSortingLayerID = groundSR != null ? groundSR.sortingLayerID : 0;

        // 6. Setup SpriteA/B for all 4 layers
        SpriteRenderer groundSpriteA, groundSpriteB;
        PrepareLayerSprites(groundGo, groundSprite, groundColor, groundSortingOrder, groundSortingLayerID, out groundSpriteA, out groundSpriteB);

        SpriteRenderer farSpriteA, farSpriteB;
        PrepareLayerSprites(farGo, farSprite, new Color(0.55f, 0.6f, 0.75f, 0.85f), -30, 0, out farSpriteA, out farSpriteB);

        SpriteRenderer midSpriteA, midSpriteB;
        PrepareLayerSprites(midGo, midSprite, new Color(0.7f, 0.72f, 0.8f, 0.9f), -20, 0, out midSpriteA, out midSpriteB);

        SpriteRenderer nearSpriteA, nearSpriteB;
        PrepareLayerSprites(nearGo, nearSprite, new Color(0.85f, 0.82f, 0.78f, 0.75f), -5, 0, out nearSpriteA, out nearSpriteB);

        // 7. Write the values to the serialized ParallaxBackground component
        SerializedObject so = new SerializedObject(parallax);
        so.Update();

        var camProperty = so.FindProperty("targetCamera");
        if (camProperty.objectReferenceValue != camTrans)
        {
            camProperty.objectReferenceValue = camTrans;
            dirty = true;
        }

        var layersProperty = so.FindProperty("layers");
        if (layersProperty.arraySize != 4)
        {
            layersProperty.arraySize = 4;
            dirty = true;
        }

        ConfigureLayerProperty(layersProperty.GetArrayElementAtIndex(0), "Floor", groundGo, groundSpriteA, groundSpriteB, 0f);
        ConfigureLayerProperty(layersProperty.GetArrayElementAtIndex(1), "Far", farGo, farSpriteA, farSpriteB, 0.08f);
        ConfigureLayerProperty(layersProperty.GetArrayElementAtIndex(2), "MidArch", midGo, midSpriteA, midSpriteB, 0.18f);
        ConfigureLayerProperty(layersProperty.GetArrayElementAtIndex(3), "Props", nearGo, nearSpriteA, nearSpriteB, 0.32f);

        if (so.ApplyModifiedProperties())
        {
            dirty = true;
        }

        return dirty;
    }

    private static void PrepareLayerSprites(
        GameObject parentGo, 
        Sprite sprite, 
        Color color, 
        int sortingOrder, 
        int sortingLayerID,
        out SpriteRenderer spriteA, 
        out SpriteRenderer spriteB)
    {
        // 1. Remove old ParallaxLayer component if present
        var oldLayer = parentGo.GetComponent<ParallaxLayer>();
        if (oldLayer != null)
        {
            Object.DestroyImmediate(oldLayer);
        }

        // 2. Clear out any existing sprite renderer on the parent itself to keep it clean
        var parentSR = parentGo.GetComponent<SpriteRenderer>();
        if (parentSR != null)
        {
            Object.DestroyImmediate(parentSR);
        }

        // 3. Find or create child SpriteA
        GameObject childA = null;
        foreach (Transform child in parentGo.transform)
        {
            if (child.name == "SpriteA")
            {
                childA = child.gameObject;
                break;
            }
        }
        if (childA == null)
        {
            childA = new GameObject("SpriteA");
            childA.transform.SetParent(parentGo.transform);
        }
        childA.transform.localPosition = Vector3.zero;
        childA.transform.localScale = Vector3.one;

        spriteA = childA.GetComponent<SpriteRenderer>();
        if (spriteA == null)
        {
            spriteA = childA.AddComponent<SpriteRenderer>();
        }
        spriteA.sprite = sprite;
        spriteA.color = color;
        spriteA.sortingOrder = sortingOrder;
        if (sortingLayerID != 0)
        {
            spriteA.sortingLayerID = sortingLayerID;
        }

        // 4. Find or create child SpriteB
        GameObject childB = null;
        foreach (Transform child in parentGo.transform)
        {
            if (child.name == "SpriteB")
            {
                childB = child.gameObject;
                break;
            }
        }
        if (childB == null)
        {
            childB = new GameObject("SpriteB");
            childB.transform.SetParent(parentGo.transform);
        }
        childB.transform.localPosition = Vector3.zero;
        childB.transform.localScale = Vector3.one;

        spriteB = childB.GetComponent<SpriteRenderer>();
        if (spriteB == null)
        {
            spriteB = childB.AddComponent<SpriteRenderer>();
        }
        spriteB.sprite = sprite;
        spriteB.color = color;
        spriteB.sortingOrder = sortingOrder;
        if (sortingLayerID != 0)
        {
            spriteB.sortingLayerID = sortingLayerID;
        }
    }

    private static void ConfigureLayerProperty(
        SerializedProperty layerProp, 
        string name, 
        GameObject parentGo, 
        SpriteRenderer spriteA, 
        SpriteRenderer spriteB, 
        float parallaxFactor)
    {
        layerProp.FindPropertyRelative("name").stringValue = name;
        layerProp.FindPropertyRelative("layerParent").objectReferenceValue = parentGo;
        layerProp.FindPropertyRelative("spriteA").objectReferenceValue = spriteA;
        layerProp.FindPropertyRelative("spriteB").objectReferenceValue = spriteB;
        layerProp.FindPropertyRelative("parallaxFactor").floatValue = parallaxFactor;
    }

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

    private static bool SetupHiddenKeyPuzzle(Scene scene)
    {
        bool dirty = false;

        // 1. Find or create SecretWallPanel
        var panelGo = FindOrCreateGO(scene, "SecretWallPanel");
        panelGo.transform.position = new Vector3(6.2f, -0.6f, 0f);
        panelGo.transform.localScale = Vector3.one;

        // 2. Set tag and layer (11 is Interactable layer)
        if (panelGo.layer != 11)
        {
            panelGo.layer = 11;
            dirty = true;
        }

        // 3. Setup BoxCollider2D
        var col = panelGo.GetComponent<BoxCollider2D>();
        if (col == null)
        {
            col = panelGo.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1f, 1.5f);
            dirty = true;
        }

        // 4. Setup HiddenKeyPuzzle component
        var puzzle = panelGo.GetComponent<HiddenKeyPuzzle>();
        if (puzzle == null)
        {
            puzzle = panelGo.AddComponent<HiddenKeyPuzzle>();
            dirty = true;
        }

        // 5. Setup GlowVisual child
        SpriteRenderer glowSR = null;
        GameObject visualGo = null;
        foreach (Transform child in panelGo.transform)
        {
            if (child.name == "GlowVisual")
            {
                visualGo = child.gameObject;
                break;
            }
        }
        if (visualGo == null)
        {
            visualGo = new GameObject("GlowVisual");
            visualGo.transform.SetParent(panelGo.transform);
            visualGo.transform.localPosition = Vector3.zero;
            visualGo.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
            dirty = true;
        }
        glowSR = visualGo.GetComponent<SpriteRenderer>();
        if (glowSR == null)
        {
            glowSR = visualGo.AddComponent<SpriteRenderer>();
            Sprite keySprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Parallax/foyer_near.png");
            glowSR.sprite = keySprite;
            glowSR.color = new Color(0f, 0.8f, 1f, 0f); // invisible by default
            glowSR.sortingOrder = 5;
            dirty = true;
        }

        // 6. Setup GlowLight child
        Light2D glowLight = null;
        GameObject lightGo = null;
        foreach (Transform child in panelGo.transform)
        {
            if (child.name == "GlowLight")
            {
                lightGo = child.gameObject;
                break;
            }
        }
        if (lightGo == null)
        {
            lightGo = new GameObject("GlowLight");
            lightGo.transform.SetParent(panelGo.transform);
            lightGo.transform.localPosition = new Vector3(0f, 0f, 0f);
            glowLight = lightGo.AddComponent<Light2D>();
            glowLight.lightType = Light2D.LightType.Point;
            glowLight.color = new Color(0f, 0.8f, 1f);
            glowLight.intensity = 0f;
            glowLight.pointLightOuterRadius = 3f;
            dirty = true;
        }
        else
        {
            glowLight = lightGo.GetComponent<Light2D>();
        }

        // 7. Write components into fields via SerializedObject
        SerializedObject so = new SerializedObject(puzzle);
        so.Update();

        var glowSRProp = so.FindProperty("hiddenObjectGlow");
        if (glowSRProp.objectReferenceValue != glowSR)
        {
            glowSRProp.objectReferenceValue = glowSR;
            dirty = true;
        }

        var glowLightProp = so.FindProperty("glowLight");
        if (glowLightProp.objectReferenceValue != glowLight)
        {
            glowLightProp.objectReferenceValue = glowLight;
            dirty = true;
        }

        var puzzleIdProp = so.FindProperty("puzzleID");
        if (puzzleIdProp.stringValue != "chapter1_hidden_key")
        {
            puzzleIdProp.stringValue = "chapter1_hidden_key";
            dirty = true;
        }

        if (so.ApplyModifiedProperties())
        {
            dirty = true;
        }

        return dirty;
    }
}
