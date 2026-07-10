using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Runtime cinematic grade without Editor Volume assets:
/// vignette, soft bloom-like center lift, color contrast, subtle fog veil.
/// True URP Bloom/AO/SSR still preferred via Volume profiles when authored.
/// </summary>
public class CinematicPostProcessOverlay : MonoBehaviour
{
    private Image vignette;
    private Image bloomLift;
    private Image fogVeil;
    private Image grade;
    private CanvasGroup group;
    private float pulse;

    private void Start()
    {
        StartCoroutine(BuildWhenReady());
    }

    private System.Collections.IEnumerator BuildWhenReady()
    {
        for (var i = 0; i < 120; i++)
        {
            var canvas = GameObject.Find("GameplayCanvas");
            if (canvas != null)
            {
                Build(canvas.transform);
                yield break;
            }
            yield return null;
        }
    }

    private void Build(Transform canvasRoot)
    {
        // Full-screen overlay canvas sibling (high sort, no raycast)
        var go = new GameObject("CinematicPostFX", typeof(RectTransform), typeof(CanvasGroup));
        go.transform.SetParent(canvasRoot, false);
        go.transform.SetAsLastSibling();
        var rect = go.GetComponent<RectTransform>();
        LockeUILayout.Stretch(rect);
        group = go.GetComponent<CanvasGroup>();
        group.blocksRaycasts = false;
        group.interactable = false;
        group.alpha = 1f;

        // Color grade (warm shadows / cool highlights approximation)
        grade = Fullscreen(go.transform, "Grade", new Color(0.15f, 0.08f, 0.12f, 0.08f));
        // Soft fog
        fogVeil = Fullscreen(go.transform, "FogVeil", new Color(0.45f, 0.5f, 0.65f, 0.04f));
        // Center bloom lift (inverse vignette)
        bloomLift = Fullscreen(go.transform, "BloomLift", new Color(1f, 0.92f, 0.75f, 0.03f));
        // Vignette
        vignette = Fullscreen(go.transform, "Vignette", new Color(0.02f, 0.02f, 0.05f, 0.35f));
        // Use radial-ish feel via stretched edges — approximate with solid + we'll pulse alpha
    }

    private static Image Fullscreen(Transform parent, string name, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        LockeUILayout.Stretch(go.GetComponent<RectTransform>());
        var img = go.GetComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    private void Update()
    {
        pulse += Time.deltaTime;
        if (fogVeil != null)
        {
            var c = fogVeil.color;
            c.a = 0.03f + Mathf.Sin(pulse * 0.35f) * 0.015f;
            fogVeil.color = c;
        }

        if (bloomLift != null)
        {
            var c = bloomLift.color;
            c.a = 0.025f + Mathf.Sin(pulse * 0.8f) * 0.01f;
            bloomLift.color = c;
        }

        // Ghost phase intensifies green grade
        var player = FindFirstObjectByType<PlayerController>();
        if (grade != null && player != null)
        {
            if (player.IsGhostPhasing)
                grade.color = Color.Lerp(grade.color, new Color(0.1f, 0.35f, 0.2f, 0.12f), Time.deltaTime * 3f);
            else
                grade.color = Color.Lerp(grade.color, new Color(0.15f, 0.08f, 0.12f, 0.08f), Time.deltaTime * 2f);
        }
    }
}
