using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Top-right compact minimap with player (P) and objective (!) markers.
/// Tap opens chapter map.
/// </summary>
public class MiniMapHUD : MonoBehaviour
{
    private RectTransform playerPip;
    private RectTransform objectivePip;
    private Image trailLine;
    private Image card;
    private float mapWorldMinX = -6f;
    private float mapWorldMaxX = 12f;
    private Font font;

    public static MiniMapHUD Ensure(Transform canvasRoot, Font font)
    {
        var existing = Object.FindFirstObjectByType<MiniMapHUD>();
        if (existing != null)
        {
            existing.Relayout();
            return existing;
        }

        var go = new GameObject("MiniMap", typeof(RectTransform), typeof(MiniMapHUD), typeof(Button));
        go.transform.SetParent(canvasRoot, false);
        var hud = go.GetComponent<MiniMapHUD>();
        hud.Build(font);
        return hud;
    }

    private void Build(Font f)
    {
        font = f ?? LockeUILayout.GetUIFont();
        var rect = GetComponent<RectTransform>();
        TopHudLayout.PlaceMinimap(rect);

        card = gameObject.AddComponent<Image>();
        TopHudLayout.ApplyGlass(card);
        TopHudLayout.AddSoftBlurLayer(transform);

        var btn = GetComponent<Button>();
        btn.targetGraphic = card;
        btn.onClick.AddListener(() => GrokUIFlowManager.Instance?.ShowChapterMap());
        UIButtonFeedback.Ensure(gameObject);

        // Floor guide
        var floor = new GameObject("Floor", typeof(RectTransform), typeof(Image));
        floor.transform.SetParent(transform, false);
        var fRect = floor.GetComponent<RectTransform>();
        fRect.anchorMin = new Vector2(0.1f, 0.28f);
        fRect.anchorMax = new Vector2(0.9f, 0.34f);
        fRect.offsetMin = fRect.offsetMax = Vector2.zero;
        floor.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.12f);
        floor.GetComponent<Image>().raycastTarget = false;

        var trailGo = new GameObject("Trail", typeof(RectTransform), typeof(Image));
        trailGo.transform.SetParent(transform, false);
        trailLine = trailGo.GetComponent<Image>();
        trailLine.raycastTarget = false;
        trailLine.color = new Color(GameSettings.AccentColor.r, GameSettings.AccentColor.g, GameSettings.AccentColor.b, 0.3f);
        var tRect = trailGo.GetComponent<RectTransform>();
        tRect.sizeDelta = new Vector2(20f, 2f);
        tRect.pivot = new Vector2(0f, 0.5f);

        playerPip = CreatePip("Player", new Color(0.4f, 0.88f, 1f), 10f, "P", Color.white);
        objectivePip = CreatePip("Objective", GameSettings.AccentColor, 12f, "!", Color.black);

        var label = MakeLabel("MAP", 8, LockeKeyUITheme.CaptionText, new Vector2(0.5f, 0.82f));
    }

    public void Relayout()
    {
        TopHudLayout.PlaceMinimap(GetComponent<RectTransform>());
        if (card != null) TopHudLayout.ApplyGlass(card);
    }

    private RectTransform CreatePip(string name, Color color, float size, string glyph, Color glyphColor)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(transform, false);
        var img = go.GetComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(size, size);

        var tGo = new GameObject("G", typeof(RectTransform), typeof(Text));
        tGo.transform.SetParent(go.transform, false);
        var t = tGo.GetComponent<Text>();
        t.font = font;
        t.fontSize = 8;
        t.fontStyle = FontStyle.Bold;
        t.color = glyphColor;
        t.text = glyph;
        t.alignment = TextAnchor.MiddleCenter;
        t.raycastTarget = false;
        var tr = tGo.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero;
        tr.anchorMax = Vector2.one;
        tr.offsetMin = tr.offsetMax = Vector2.zero;
        return rect;
    }

    private Text MakeLabel(string text, int size, Color color, Vector2 anchor)
    {
        var go = new GameObject("Label", typeof(RectTransform), typeof(Text));
        go.transform.SetParent(transform, false);
        var t = go.GetComponent<Text>();
        t.font = font;
        t.fontSize = size;
        t.color = color;
        t.text = text;
        t.alignment = TextAnchor.MiddleCenter;
        t.raycastTarget = false;
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = anchor;
        rect.sizeDelta = new Vector2(60f, 12f);
        return t;
    }

    private void Update()
    {
        var player = FindFirstObjectByType<PlayerController>();
        var guide = FindFirstObjectByType<ObjectiveGuideController>();

        Vector2 pPos = Vector2.zero;
        if (player != null && playerPip != null)
        {
            pPos = WorldToMap(player.transform.position);
            playerPip.anchoredPosition = pPos;
        }

        if (guide != null && guide.CurrentTarget != null && objectivePip != null)
        {
            objectivePip.gameObject.SetActive(true);
            var oPos = WorldToMap(guide.CurrentTarget.position);
            objectivePip.anchoredPosition = oPos;
            objectivePip.localScale = Vector3.one * (1f + Mathf.Sin(Time.time * 4f) * 0.15f);

            if (trailLine != null)
            {
                trailLine.gameObject.SetActive(true);
                var dir = oPos - pPos;
                trailLine.rectTransform.anchoredPosition = pPos;
                trailLine.rectTransform.sizeDelta = new Vector2(Mathf.Max(3f, dir.magnitude), 2f);
                trailLine.rectTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
            }
        }
        else
        {
            if (objectivePip != null) objectivePip.gameObject.SetActive(false);
            if (trailLine != null) trailLine.gameObject.SetActive(false);
        }
    }

    private Vector2 WorldToMap(Vector3 world)
    {
        float t = Mathf.InverseLerp(mapWorldMinX, mapWorldMaxX, world.x);
        return new Vector2(Mathf.Lerp(-36f, 36f, t), -4f);
    }
}
