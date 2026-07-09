#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Phase A4 gate: spawn viewport + toast + primary button in the active scene.
/// </summary>
public static class LockeUITestCanvasMenu
{
    [MenuItem("LockeKey/UI/Spawn A4 Test Canvas")]
    public static void SpawnTestCanvas()
    {
        if (GameObject.Find("A4TestCanvas") != null)
        {
            if (!EditorUtility.DisplayDialog("A4 Test Canvas",
                    "A4TestCanvas already exists. Replace it?", "Replace", "Cancel"))
                return;

            Object.DestroyImmediate(GameObject.Find("A4TestCanvas"));
        }

        EnsureEventSystem();

        var flow = LockeUILayout.CreateFlowCanvas("A4TestCanvas", 250);
        var root = LockeUILayout.GetContentRoot(flow);
        var font = flow.Font ?? LockeUILayout.GetUIFont();

        var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(root, false);
        LockeUILayout.Stretch(panel.GetComponent<RectTransform>());
        panel.GetComponent<Image>().color = LockeKeyUITheme.LKInk;

        LockeUIComponents.AddText(panel.transform, "Title", font, LockeKeyUITheme.TitleSize,
            FontStyle.Bold, LockeKeyUITheme.LKGold, new Vector2(0.5f, 0.62f),
            "A4 Gate — Locke UI", new Vector2(LockeKeyUITheme.RefWidth - 40f, 40f), TextAnchor.MiddleCenter);

        var toastText = LockeUIComponents.CreateToastHost(panel.transform, font, out var toastCg);

        LockeUIComponents.CreatePrimaryButton(panel.transform, font, "Hello",
            new Vector2(0.5f, 0.38f),
            () =>
            {
                if (toastText != null)
                {
                    toastText.text = "Hello from A4 test canvas";
                    toastCg.alpha = 1f;
                }
            }, 220f);

        Selection.activeGameObject = flow.Canvas.gameObject;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[A4] Test canvas spawned: viewport + toast + Hello primary button.");
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null)
            return;

        var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        Undo.RegisterCreatedObjectUndo(es, "Create EventSystem");
    }
}
#endif