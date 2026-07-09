#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Opens hi-fi UX JPEG references from Assets/ArtSource/ux for side-by-side comparison.
/// </summary>
public static class UxReferenceMenu
{
    [MenuItem("LockeKey/UX Reference/Open S0 Splash")]
    public static void OpenS0() => OpenReference(UxReferencePaths.S0Splash);

    [MenuItem("LockeKey/UX Reference/Open S1 Story Reel")]
    public static void OpenS1() => OpenReference(UxReferencePaths.S1StoryReel);

    [MenuItem("LockeKey/UX Reference/Open S2 Chapter Map")]
    public static void OpenS2() => OpenReference(UxReferencePaths.S2ChapterMap);

    [MenuItem("LockeKey/UX Reference/Open S3 Foyer HUD")]
    public static void OpenS3() => OpenReference(UxReferencePaths.S3FoyerHud);

    [MenuItem("LockeKey/UX Reference/Open S4 Key Discovery")]
    public static void OpenS4() => OpenReference(UxReferencePaths.S4KeyDiscovery);

    [MenuItem("LockeKey/UX Reference/Open S5 Lock Puzzle")]
    public static void OpenS5() => OpenReference(UxReferencePaths.S5LockPuzzle);

    [MenuItem("LockeKey/UX Reference/Open S6 Key Ring")]
    public static void OpenS6() => OpenReference(UxReferencePaths.S6KeyRing);

    [MenuItem("LockeKey/UX Reference/Open Design System")]
    public static void OpenDesignSystem() => OpenReference(UxReferencePaths.DesignSystem);

    [MenuItem("LockeKey/UX Reference/Open Landscape Frame")]
    public static void OpenLandscapeFrame() => OpenReference(UxReferencePaths.LandscapeFrame);

    [MenuItem("LockeKey/UX Reference/Compare Side by Side...")]
    public static void OpenComparer()
    {
        UxReferenceComparerWindow.ShowWindow();
    }

    private static void OpenReference(string assetPath)
    {
        if (!File.Exists(assetPath))
        {
            Debug.LogError($"[UxReference] Missing file: {assetPath}");
            EditorUtility.DisplayDialog("UX Reference", $"File not found:\n{assetPath}", "OK");
            return;
        }

        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        if (texture == null)
        {
            Debug.LogError($"[UxReference] Could not load texture: {assetPath}");
            return;
        }

        Selection.activeObject = texture;
        EditorGUIUtility.PingObject(texture);
    }
}

/// <summary>Side-by-side UX reference viewer for layout QA.</summary>
public class UxReferenceComparerWindow : EditorWindow
{
    private static readonly (string Label, string Path)[] Screens =
    {
        ("S0 Splash", UxReferencePaths.S0Splash),
        ("S1 Story Reel", UxReferencePaths.S1StoryReel),
        ("S2 Chapter Map", UxReferencePaths.S2ChapterMap),
        ("S3 Foyer HUD", UxReferencePaths.S3FoyerHud),
        ("S4 Key Discovery", UxReferencePaths.S4KeyDiscovery),
        ("S5 Lock Puzzle", UxReferencePaths.S5LockPuzzle),
        ("S6 Key Ring", UxReferencePaths.S6KeyRing),
        ("Design System", UxReferencePaths.DesignSystem),
        ("Landscape Frame", UxReferencePaths.LandscapeFrame),
    };

    private int leftIndex;
    private int rightIndex = 1;

    public static void ShowWindow()
    {
        var window = GetWindow<UxReferenceComparerWindow>("UX Compare");
        window.minSize = new Vector2(720f, 420f);
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Side-by-Side UX Reference", EditorStyles.boldLabel);
        EditorGUILayout.Space(4f);

        EditorGUILayout.BeginHorizontal();
        leftIndex = EditorGUILayout.Popup("Left", leftIndex, GetLabels());
        rightIndex = EditorGUILayout.Popup("Right", rightIndex, GetLabels());
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(8f);

        var left = LoadTexture(Screens[leftIndex].Path);
        var right = LoadTexture(Screens[rightIndex].Path);

        EditorGUILayout.BeginHorizontal();
        DrawTexturePanel(left, Screens[leftIndex].Label);
        DrawTexturePanel(right, Screens[rightIndex].Label);
        EditorGUILayout.EndHorizontal();
    }

    private static string[] GetLabels()
    {
        var labels = new string[Screens.Length];
        for (int i = 0; i < Screens.Length; i++)
            labels[i] = Screens[i].Label;
        return labels;
    }

    private static Texture2D LoadTexture(string path) =>
        File.Exists(path) ? AssetDatabase.LoadAssetAtPath<Texture2D>(path) : null;

    private static void DrawTexturePanel(Texture2D texture, string label)
    {
        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        EditorGUILayout.LabelField(label, EditorStyles.miniBoldLabel);

        if (texture == null)
        {
            EditorGUILayout.HelpBox("Reference image missing.", MessageType.Warning);
        }
        else
        {
            float aspect = (float)texture.width / texture.height;
            float width = (position.width - 24f) * 0.5f;
            float height = Mathf.Min(position.height - 80f, width / aspect);
            var rect = GUILayoutUtility.GetRect(width, height, GUILayout.ExpandWidth(true));
            GUI.DrawTexture(rect, texture, ScaleMode.ScaleToFit);
        }

        EditorGUILayout.EndVertical();
    }
}
#endif