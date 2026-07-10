using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Clear mini-map with labeled player (P) and objective (!) markers.
/// </summary>
public class MiniMapHUD : MonoBehaviour
{
    private RectTransform playerPip;
    private RectTransform objectivePip;
    private Text playerLabel;
    private Text objectiveLabel;
    private Image keyPip;
    private Image trailLine;
    private float mapWorldMinX = -6f;
    private float mapWorldMaxX = 12f;
    private Font font;

    public static MiniMapHUD Ensure(Transform canvasRoot, Font font)
    {
        var existing = Object.FindFirstObjectByType<MiniMapHUD>();
        if (existing != null) return existing;
        var go = new GameObject("MiniMap", typeof(RectTransform), typeof(MiniMapHUD));
        go.transform.SetParent(canvasRoot, false);
        var hud = go.GetComponent<MiniMapHUD>();
        hud.Build(font);
        return hud;
    }

    private void Build(Font f)
    {
        font = f ?? LockeUILayout.GetUIFont();
        var rect = GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-10f, -52f);
        rect.sizeDelta = new Vector2(124f, 84f);

        var bg = gameObject.AddComponent<Image>();
        bg.color = new Color(0.04f, 0.05f, 0.09f, 0.88f);
        var outline = gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(GameSettings.AccentColor.r, GameSettings.AccentColor.g, GameSettings.AccentColor.b, 0.45f);
        outline.effectDistance = new Vector2(1.2f, -1.2f);

        // Header
        var header = MakeUiText("MAP", 10, FontStyle.Bold, LockeKeyUITheme.CaptionText,
            new Vector2(0.5f, 0.88f), new Vector2(100f, 14f));

        // Corridor floor
        var floor = new GameObject("Floor", typeof(RectTransform), typeof(Image));
        floor.transform.SetParent(transform, false);
        var fRect = floor.GetComponent<RectTransform>();
        fRect.anchorMin = new Vector2(0.08f, 0.22f);
        fRect.anchorMax = new Vector2(0.92f, 0.28f);
        fRect.offsetMin = fRect.offsetMax = Vector2.zero;
        floor.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.18f);
        floor.GetComponent<Image>().raycastTarget = false;

        // Path trail between player and objective
        var trailGo = new GameObject("Trail", typeof(RectTransform), typeof(Image));
        trailGo.transform.SetParent(transform, false);
        trailLine = trailGo.GetComponent<Image>();
        trailLine.color = new Color(GameSettings.AccentColor.r, GameSettings.AccentColor.g, GameSettings.AccentColor.b, 0.35f);
        trailLine.raycastTarget = false;
        var tRect = trailGo.GetComponent<RectTransform>();
        tRect.sizeDelta = new Vector2(40f, 2f);
        tRect.pivot = new Vector2(0f, 0.5f);

        playerPip = CreatePip("Player", new Color(0.35f, 0.85f, 1f, 1f), 12f);
        objectivePip = CreatePip("Objective", GameSettings.AccentColor, 14f);

        playerLabel = MakeUiText("P", 9, FontStyle.Bold, Color.white,
            new Vector2(0.5f, 0.5f), new Vector2(12f, 12f));
        playerLabel.transform.SetParent(playerPip, false);
        var pl = playerLabel.GetComponent<RectTransform>();
        pl.anchorMin = Vector2.zero; pl.anchorMax = Vector2.one;
        pl.offsetMin = pl.offsetMax = Vector2.zero;

        objectiveLabel = MakeUiText("!", 11, FontStyle.Bold, Color.black,
            new Vector2(0.5f, 0.5f), new Vector2(14f, 14f));
        objectiveLabel.transform.SetParent(objectivePip, false);
        var ol = objectiveLabel.GetComponent<RectTransform>();
        ol.anchorMin = Vector2.zero; ol.anchorMax = Vector2.one;
        ol.offsetMin = ol.offsetMax = Vector2.zero;

        var keyGo = new GameObject("KeyPip", typeof(RectTransform), typeof(Image));
        keyGo.transform.SetParent(transform, false);
        keyPip = keyGo.GetComponent<Image>();
        keyPip.color = LockeKeyUITheme.LKGold;
        var kRect = keyGo.GetComponent<RectTransform>();
        kRect.anchorMin = kRect.anchorMax = new Vector2(0.14f, 0.78f);
        kRect.sizeDelta = new Vector2(11f, 11f);

        var legend = MakeUiText("P you   ! goal", 8, FontStyle.Normal, LockeKeyUITheme.CaptionText,
            new Vector2(0.55f, 0.1f), new Vector2(90f, 12f));
    }

    private Text MakeUiText(string text, int size, FontStyle style, Color color, Vector2 anchor, Vector2 sizeDelta)
    {
        var go = new GameObject("T_" + text, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(transform, false);
        var t = go.GetComponent<Text>();
        t.font = font;
        t.fontSize = size;
        t.fontStyle = style;
        t.color = color;
        t.text = text;
        t.alignment = TextAnchor.MiddleCenter;
        t.raycastTarget = false;
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = anchor;
        rect.sizeDelta = sizeDelta;
        return t;
    }

    private RectTransform CreatePip(string name, Color color, float size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(transform, false);
        var img = go.GetComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(size, size);
        return rect;
    }

    private void Update()
    {
        var player = FindFirstObjectByType<PlayerController>();
        var guide = FindFirstObjectByType<ObjectiveGuideController>();
        var inv = FindFirstObjectByType<PlayerInventory>();

        Vector2 pPos = Vector2.zero;
        Vector2 oPos = Vector2.zero;

        if (player != null && playerPip != null)
        {
            pPos = WorldToMap(player.transform.position);
            playerPip.anchoredPosition = pPos;
        }

        if (guide != null && guide.CurrentTarget != null && objectivePip != null)
        {
            objectivePip.gameObject.SetActive(true);
            oPos = WorldToMap(guide.CurrentTarget.position);
            objectivePip.anchoredPosition = oPos;
            float s = 1f + Mathf.Sin(Time.time * 4.5f) * 0.22f;
            objectivePip.localScale = Vector3.one * s;

            if (trailLine != null)
            {
                trailLine.gameObject.SetActive(true);
                var mid = (pPos + oPos) * 0.5f;
                var dir = oPos - pPos;
                float len = dir.magnitude;
                trailLine.rectTransform.anchoredPosition = pPos;
                trailLine.rectTransform.sizeDelta = new Vector2(Mathf.Max(4f, len), 2.5f);
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                trailLine.rectTransform.localRotation = Quaternion.Euler(0f, 0f, angle);
                var c = GameSettings.AccentColor;
                trailLine.color = new Color(c.r, c.g, c.b, 0.25f + Mathf.Sin(Time.time * 3f) * 0.1f);
            }
        }
        else if (objectivePip != null)
        {
            objectivePip.gameObject.SetActive(false);
            if (trailLine != null) trailLine.gameObject.SetActive(false);
        }

        if (keyPip != null)
        {
            bool has = inv != null && inv.HasHouseKey;
            keyPip.enabled = has;
            if (has)
                keyPip.color = new Color(1f, 0.85f, 0.3f, 0.75f + Mathf.Sin(Time.time * 3f) * 0.25f);
        }
    }

    private Vector2 WorldToMap(Vector3 world)
    {
        float t = Mathf.InverseLerp(mapWorldMinX, mapWorldMaxX, world.x);
        float x = Mathf.Lerp(-46f, 46f, t);
        float y = -6f;
        return new Vector2(x, y);
    }
}
