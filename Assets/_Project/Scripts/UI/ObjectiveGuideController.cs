using UnityEngine;

/// <summary>
/// World-space guidance: pulsing target outline, bouncing arrow, floor footprints
/// toward the current chapter objective.
/// </summary>
public class ObjectiveGuideController : MonoBehaviour
{
    private Transform player;
    private Transform target;
    private string targetLabel = "";
    private GameObject arrowGo;
    private SpriteRenderer arrowSr;
    private GameObject ringGo;
    private SpriteRenderer ringSr;
    private Transform footRoot;
    private Sprite softSprite;
    private float bob;
    private readonly Vector3[] footSlots = new Vector3[6];
    private SpriteRenderer[] footRenderers;

    public Transform CurrentTarget => target;

    private void Awake()
    {
        player = FindFirstObjectByType<PlayerController>()?.transform;
        softSprite = CreateDisc(32);
        BuildVisuals();
    }

    private void Update()
    {
        if (player == null)
            player = FindFirstObjectByType<PlayerController>()?.transform;

        ResolveTargetFromBeat();
        UpdateVisuals();
    }

    public void SetTarget(Transform t, string label = "")
    {
        target = t;
        targetLabel = label ?? "";
        if (arrowGo != null) arrowGo.SetActive(t != null);
        if (ringGo != null) ringGo.SetActive(t != null);
        if (footRoot != null) footRoot.gameObject.SetActive(t != null);
    }

    private void ResolveTargetFromBeat()
    {
        var beat = FindFirstObjectByType<ChapterBeatDirector>();
        if (beat == null) return;

        Transform next = null;
        string label = "";

        switch (beat.CurrentBeat)
        {
            case ChapterBeatDirector.Beat.Arrival:
                next = FindFirstObjectByType<HouseKeyPickup>()?.transform;
                label = "House Key";
                break;
            case ChapterBeatDirector.Beat.StuckDoor:
                next = FindFirstObjectByType<StuckDoorPuzzle>()?.transform;
                label = "Front Door";
                break;
            case ChapterBeatDirector.Beat.Library:
            {
                var km = FindFirstObjectByType<KeyManager>();
                bool hasGhost = km != null && km.ownedKeys.Exists(k => k.abilityType == KeyManager.KeyAbilityType.GhostPhase);
                if (!hasGhost)
                {
                    var shelf = FindFirstObjectByType<CollapsedBookshelfPuzzle>();
                    if (shelf != null && !shelf.isSolved)
                    {
                        next = shelf.transform;
                        label = "Bookshelf";
                    }
                    else
                    {
                        next = FindFirstObjectByType<GhostKeyPickup>()?.transform;
                        label = "Ghost Key";
                    }
                }
                else
                {
                    next = FindFirstObjectByType<SealedDoorPuzzle>()?.transform;
                    label = "Sealed Door";
                }
                break;
            }
            case ChapterBeatDirector.Beat.GhostKeyUse:
                next = FindFirstObjectByType<SealedDoorPuzzle>()?.transform;
                label = "Sealed Door";
                break;
            case ChapterBeatDirector.Beat.EchoEncounter:
                next = GameObject.Find("HideArch")?.transform
                       ?? FindFirstObjectByType<HideSpot>()?.transform;
                label = "Hide";
                break;
            case ChapterBeatDirector.Beat.Aftermath:
                next = FindFirstObjectByType<HeadKeyPickup>()?.transform
                       ?? FindFirstObjectByType<MemoryFragmentPuzzle>()?.transform;
                label = "Head Key";
                break;
        }

        if (next != target)
            SetTarget(next, label);
    }

    private void UpdateVisuals()
    {
        if (target == null || player == null)
        {
            if (arrowGo != null) arrowGo.SetActive(false);
            if (ringGo != null) ringGo.SetActive(false);
            return;
        }

        bob += Time.deltaTime * 3.2f;
        var pulse = 0.85f + Mathf.Sin(Time.time * 3.5f) * 0.15f;
        var accent = GameSettings.AccentColor;

        // Ring around target
        if (ringGo != null)
        {
            ringGo.SetActive(true);
            ringGo.transform.position = target.position + Vector3.up * 0.15f;
            ringGo.transform.localScale = Vector3.one * (1.35f * pulse);
            if (ringSr != null)
                ringSr.color = new Color(accent.r, accent.g, accent.b, 0.35f + Mathf.Sin(Time.time * 4f) * 0.2f);
        }

        // Arrow above target
        if (arrowGo != null)
        {
            arrowGo.SetActive(true);
            arrowGo.transform.position = target.position + Vector3.up * (1.55f + Mathf.Sin(bob) * 0.12f);
            if (arrowSr != null)
                arrowSr.color = new Color(accent.r, accent.g, accent.b, 0.9f);
        }

        // Footprints along path
        if (footRoot != null && footRenderers != null)
        {
            footRoot.gameObject.SetActive(true);
            var from = player.position;
            var to = target.position;
            var footDist = Vector2.Distance(from, to);
            // Hide footprints when very close (keep running rest of visuals)
            bool show = footDist > 1.4f;
            footRoot.gameObject.SetActive(show);
            if (show)
            {
                for (var i = 0; i < footRenderers.Length; i++)
                {
                    float t = (i + 1f) / (footRenderers.Length + 1f);
                    var p = Vector3.Lerp(from, to, t);
                    p.y = Mathf.Min(from.y, to.y) - 0.55f;
                    // Marching opacity
                    float march = Mathf.Repeat(Time.time * 0.55f + t, 1f);
                    footRenderers[i].transform.position = p;
                    footRenderers[i].color = new Color(accent.r, accent.g, accent.b, 0.12f + march * 0.28f);
                    footRenderers[i].transform.localScale = Vector3.one * (0.18f + (i % 2) * 0.03f);
                }
            }
        }

        float dist = Vector2.Distance(player.position, target.position);
        // Camera zoom when near objective
        if (dist < 2.8f)
            FindFirstObjectByType<CameraFollow2D>()?.SetInterestZoom(Mathf.Lerp(0.86f, 0.95f, dist / 2.8f));
        else
            FindFirstObjectByType<CameraFollow2D>()?.SetInterestZoom(1f);

        // Distance hint on objective tracker title via toast is too noisy; drive HUD distance pip
        var tracker = FindFirstObjectByType<ObjectiveTrackerHUD>();
        // footprints denser when farther
        if (footRenderers != null)
        {
            int showCount = dist > 6f ? footRenderers.Length : dist > 3f ? 4 : 2;
            for (var i = 0; i < footRenderers.Length; i++)
                if (footRenderers[i] != null)
                    footRenderers[i].enabled = i < showCount && dist > 1.4f;
        }
        _ = tracker;
    }

    private void BuildVisuals()
    {
        arrowGo = new GameObject("ObjectiveArrow");
        arrowSr = arrowGo.AddComponent<SpriteRenderer>();
        arrowSr.sprite = CreateArrowSprite();
        arrowSr.sortingOrder = 40;
        arrowGo.transform.localScale = Vector3.one * 0.55f;

        ringGo = new GameObject("ObjectiveRing");
        ringSr = ringGo.AddComponent<SpriteRenderer>();
        ringSr.sprite = softSprite;
        ringSr.sortingOrder = 5;
        ringGo.transform.localScale = Vector3.one * 1.4f;

        footRoot = new GameObject("ObjectiveFootprints").transform;
        footRenderers = new SpriteRenderer[6];
        for (var i = 0; i < footRenderers.Length; i++)
        {
            var go = new GameObject($"Foot_{i}", typeof(SpriteRenderer));
            go.transform.SetParent(footRoot);
            footRenderers[i] = go.GetComponent<SpriteRenderer>();
            footRenderers[i].sprite = softSprite;
            footRenderers[i].sortingOrder = 3;
        }

        arrowGo.SetActive(false);
        ringGo.SetActive(false);
        footRoot.gameObject.SetActive(false);
    }

    private static Sprite CreateDisc(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var mid = (size - 1) * 0.5f;
        for (var y = 0; y < size; y++)
        for (var x = 0; x < size; x++)
        {
            var d = Vector2.Distance(new Vector2(x, y), new Vector2(mid, mid)) / mid;
            float a = d < 0.75f ? Mathf.Clamp01((d - 0.55f) / 0.2f) : Mathf.Clamp01(1f - (d - 0.75f) / 0.25f);
            if (d < 0.55f) a = 0f;
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    private static Sprite CreateArrowSprite()
    {
        const int w = 32, h = 40;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        for (var y = 0; y < h; y++)
        for (var x = 0; x < w; x++)
            tex.SetPixel(x, y, Color.clear);

        // Simple chevron pointing down
        for (var y = 8; y < 32; y++)
        {
            var t = (y - 8) / 24f;
            var half = Mathf.Lerp(2, 12, t);
            for (var x = (int)(16 - half); x <= (int)(16 + half); x++)
            {
                if (x < 0 || x >= w) continue;
                var edge = Mathf.Abs(x - 16) > half - 2;
                tex.SetPixel(x, y, edge ? Color.white : new Color(1f, 1f, 1f, 0.85f));
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.2f), 40f);
    }

    public string TargetLabel => targetLabel;
}
