using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Compact map action. The full chapter map remains available on tap; gameplay is not
/// obscured by a miniature representation that players cannot read at phone scale.
/// </summary>
public class MiniMapHUD : MonoBehaviour
{
    private Image card;
    private Text label;

    public static MiniMapHUD Ensure(Transform canvasRoot, Font font)
    {
        var existing = Object.FindFirstObjectByType<MiniMapHUD>();
        if (existing != null)
        {
            existing.Relayout();
            return existing;
        }

        var go = new GameObject("MapButton", typeof(RectTransform), typeof(MiniMapHUD), typeof(Image), typeof(Button));
        go.transform.SetParent(canvasRoot, false);
        var hud = go.GetComponent<MiniMapHUD>();
        hud.Build(font);
        return hud;
    }

    private void Build(Font font)
    {
        TopHudLayout.PlaceMinimap(GetComponent<RectTransform>());
        card = GetComponent<Image>();
        TopHudLayout.ApplyGlass(card, deep: true);

        var button = GetComponent<Button>();
        button.targetGraphic = card;
        button.onClick.AddListener(() => GrokUIFlowManager.Instance?.ShowChapterMap());
        UIButtonFeedback.Ensure(gameObject);

        var labelGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
        labelGo.transform.SetParent(transform, false);
        label = labelGo.GetComponent<Text>();
        label.font = font ?? LockeUILayout.GetUIFont();
        label.fontSize = 10;
        label.fontStyle = FontStyle.Bold;
        label.color = LockeKeyUITheme.BodyText;
        label.alignment = TextAnchor.MiddleCenter;
        label.text = "MAP";
        label.raycastTarget = false;
        LockeUILayout.Stretch(labelGo.GetComponent<RectTransform>());
    }

    public void Relayout()
    {
        TopHudLayout.PlaceMinimap(GetComponent<RectTransform>());
        if (card != null) TopHudLayout.ApplyGlass(card, deep: true);
    }
}
