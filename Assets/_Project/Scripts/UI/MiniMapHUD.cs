using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Compact chapter mini-map: player, objective marker, key collected pip.
/// </summary>
public class MiniMapHUD : MonoBehaviour
{
    private RectTransform playerPip;
    private RectTransform objectivePip;
    private Image keyPip;
    private Image compass;
    private float mapWorldMinX = -6f;
    private float mapWorldMaxX = 12f;

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

    private void Build(Font font)
    {
        var rect = GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-10f, -54f);
        rect.sizeDelta = new Vector2(110f, 72f);

        var bg = gameObject.AddComponent<Image>();
        bg.color = new Color(0.04f, 0.05f, 0.08f, 0.82f);
        var outline = gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(GameSettings.AccentColor.r, GameSettings.AccentColor.g, GameSettings.AccentColor.b, 0.35f);
        outline.effectDistance = new Vector2(1f, -1f);

        // Floor line
        var floor = new GameObject("Floor", typeof(RectTransform), typeof(Image));
        floor.transform.SetParent(transform, false);
        var fRect = floor.GetComponent<RectTransform>();
        fRect.anchorMin = new Vector2(0.08f, 0.28f);
        fRect.anchorMax = new Vector2(0.92f, 0.32f);
        fRect.offsetMin = fRect.offsetMax = Vector2.zero;
        floor.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.15f);
        floor.GetComponent<Image>().raycastTarget = false;

        playerPip = CreatePip("Player", new Color(0.4f, 0.85f, 1f), 10f);
        objectivePip = CreatePip("Objective", GameSettings.AccentColor, 12f);

        var keyGo = new GameObject("KeyPip", typeof(RectTransform), typeof(Image));
        keyGo.transform.SetParent(transform, false);
        keyPip = keyGo.GetComponent<Image>();
        keyPip.color = LockeKeyUITheme.LKGold;
        var kRect = keyGo.GetComponent<RectTransform>();
        kRect.anchorMin = kRect.anchorMax = new Vector2(0.12f, 0.78f);
        kRect.sizeDelta = new Vector2(10f, 10f);

        var label = new GameObject("Label", typeof(RectTransform), typeof(Text));
        label.transform.SetParent(transform, false);
        var t = label.GetComponent<Text>();
        t.font = font ?? LockeUILayout.GetUIFont();
        t.fontSize = 9;
        t.color = LockeKeyUITheme.CaptionText;
        t.text = "MAP";
        t.alignment = TextAnchor.UpperCenter;
        var lRect = label.GetComponent<RectTransform>();
        lRect.anchorMin = new Vector2(0f, 0.7f);
        lRect.anchorMax = new Vector2(1f, 1f);
        lRect.offsetMin = lRect.offsetMax = Vector2.zero;
        t.raycastTarget = false;

        // Compass N
        var nGo = new GameObject("Compass", typeof(RectTransform), typeof(Text));
        nGo.transform.SetParent(transform, false);
        compass = null;
        var nt = nGo.GetComponent<Text>();
        nt.font = font ?? LockeUILayout.GetUIFont();
        nt.fontSize = 10;
        nt.fontStyle = FontStyle.Bold;
        nt.color = GameSettings.AccentColor;
        nt.text = "►";
        nt.alignment = TextAnchor.MiddleCenter;
        var nRect = nGo.GetComponent<RectTransform>();
        nRect.anchorMin = nRect.anchorMax = new Vector2(0.88f, 0.78f);
        nRect.sizeDelta = new Vector2(16f, 16f);
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

        if (player != null && playerPip != null)
            playerPip.anchoredPosition = WorldToMap(player.transform.position);

        if (guide != null && guide.CurrentTarget != null && objectivePip != null)
        {
            objectivePip.gameObject.SetActive(true);
            objectivePip.anchoredPosition = WorldToMap(guide.CurrentTarget.position);
            // Pulse
            float s = 1f + Mathf.Sin(Time.time * 4f) * 0.2f;
            objectivePip.localScale = Vector3.one * s;
        }
        else if (objectivePip != null)
        {
            objectivePip.gameObject.SetActive(false);
        }

        if (keyPip != null)
        {
            bool has = inv != null && inv.HasHouseKey;
            keyPip.enabled = has;
            if (has)
                keyPip.color = new Color(1f, 0.85f, 0.3f, 0.7f + Mathf.Sin(Time.time * 3f) * 0.3f);
        }
    }

    private Vector2 WorldToMap(Vector3 world)
    {
        float t = Mathf.InverseLerp(mapWorldMinX, mapWorldMaxX, world.x);
        float x = Mathf.Lerp(-42f, 42f, t);
        float y = -8f;
        return new Vector2(x, y);
    }
}
