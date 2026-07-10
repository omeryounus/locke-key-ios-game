using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Floating tutorial cards (not debug text). Each card shows title + body + optional check,
/// auto-fades after 3s or when the player completes the action.
/// </summary>
public class TutorialCoach : MonoBehaviour
{
    private enum Step { Move, CollectKey, Door, Interact, Done }

    private Step step = Step.Move;
    private CanvasGroup group;
    private Text titleText;
    private Text bodyText;
    private Text checkText;
    private Image cardBg;
    private TouchGameplayController gameplay;
    private float stepTimer;
    private float autoFadeAt = 3f;
    private bool fading;
    private bool built;
    private Font font;

    private void Start()
    {
        if (GameSettings.TutorialCompleted)
        {
            enabled = false;
            return;
        }

        gameplay = FindFirstObjectByType<TouchGameplayController>();
        StartCoroutine(BuildWhenCanvasReady());
    }

    private IEnumerator BuildWhenCanvasReady()
    {
        for (var i = 0; i < 90 && !built; i++)
        {
            var canvas = GameObject.Find("GameplayCanvas");
            if (canvas != null)
            {
                var content = canvas.transform.Find("Viewport/Content") ?? canvas.transform;
                Build(content, LockeUILayout.GetUIFont());
                built = true;
                ShowCard("Move", "Hold Left or Right", showCheck: false);
                break;
            }
            yield return null;
        }
    }

    private void Build(Transform parent, Font f)
    {
        font = f;
        var go = new GameObject("TutorialCard", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.52f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(260f, 88f);

        cardBg = go.GetComponent<Image>();
        TopHudLayout.ApplyGlass(cardBg);
        TopHudLayout.AddSoftBlurLayer(go.transform);
        group = go.GetComponent<CanvasGroup>();

        // Accent top line
        var line = new GameObject("Line", typeof(RectTransform), typeof(Image));
        line.transform.SetParent(go.transform, false);
        var lRect = line.GetComponent<RectTransform>();
        lRect.anchorMin = new Vector2(0.15f, 1f);
        lRect.anchorMax = new Vector2(0.85f, 1f);
        lRect.pivot = new Vector2(0.5f, 1f);
        lRect.sizeDelta = new Vector2(0f, 2f);
        line.GetComponent<Image>().color = GameSettings.AccentColor;
        line.GetComponent<Image>().raycastTarget = false;

        titleText = MakeText(go.transform, "Title", 16, FontStyle.Bold, Color.white, new Vector2(0.5f, 0.68f), 220f);
        bodyText = MakeText(go.transform, "Body", 13, FontStyle.Normal, LockeKeyUITheme.BodyText, new Vector2(0.5f, 0.38f), 230f);
        checkText = MakeText(go.transform, "Check", 18, FontStyle.Bold, LockeKeyUITheme.Success, new Vector2(0.5f, 0.14f), 40f);
        checkText.text = "✓";
        checkText.gameObject.SetActive(false);

        go.AddComponent<TutorialCoachProxy>().Bind(this);
    }

    private Text MakeText(Transform parent, string name, int size, FontStyle style, Color color, Vector2 anchor, float width)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<Text>();
        t.font = font;
        t.fontSize = size;
        t.fontStyle = style;
        t.color = color;
        t.alignment = TextAnchor.MiddleCenter;
        t.raycastTarget = false;
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = anchor;
        rect.sizeDelta = new Vector2(width, 28f);
        return t;
    }

    private void ShowCard(string title, string body, bool showCheck)
    {
        if (titleText != null) titleText.text = title;
        if (bodyText != null) bodyText.text = body;
        if (checkText != null) checkText.gameObject.SetActive(showCheck);
        if (group != null)
        {
            group.alpha = 1f;
            group.gameObject.SetActive(true);
            group.transform.localScale = Vector3.one * 0.92f;
        }
        stepTimer = 0f;
        fading = false;
        autoFadeAt = 3f;
    }

    public void TickProxy()
    {
        if (GameSettings.TutorialCompleted || group == null) return;
        stepTimer += Time.deltaTime;

        // Soft entrance scale
        if (group.transform.localScale.x < 0.99f)
            group.transform.localScale = Vector3.Lerp(group.transform.localScale, Vector3.one, Time.deltaTime * 8f);

        // Auto-fade each card after 3s (unless completed early)
        if (!fading && stepTimer >= autoFadeAt && step != Step.Done)
            StartCoroutine(FadeCard(0.45f, advanceOnComplete: false));

        switch (step)
        {
            case Step.Move:
                if (gameplay != null && Mathf.Abs(gameplay.MoveInput) > 0.2f && stepTimer > 0.35f)
                {
                    step = Step.CollectKey;
                    ShowCard("Move", "Hold Left or Right", showCheck: true);
                    StartCoroutine(AdvanceAfterCheck("Find the Key", "Walk to the glow · Tap Interact", Step.CollectKey));
                    FindFirstObjectByType<GameplayHUD>()?.SetControlVisibility(interact: true);
                }
                break;
            case Step.CollectKey:
            {
                var inv = FindFirstObjectByType<PlayerInventory>();
                if (inv != null && inv.HasHouseKey)
                {
                    step = Step.Door;
                    ShowCard("Key Found", "Head to the Front Door", showCheck: true);
                    StartCoroutine(AdvanceAfterCheck("Front Door", "Follow the glowing trail", Step.Door));
                }
                break;
            }
            case Step.Door:
            {
                var door = FindFirstObjectByType<StuckDoorPuzzle>();
                var player = FindFirstObjectByType<PlayerController>();
                if (door != null && player != null &&
                    Vector2.Distance(door.transform.position, player.transform.position) < 2.6f)
                {
                    step = Step.Interact;
                    ShowCard("Unlock", "Tap Interact", showCheck: false);
                    FindFirstObjectByType<GameplayHUD>()?.FlashInteractButton(2.5f);
                }
                break;
            }
            case Step.Interact:
            {
                var door = FindFirstObjectByType<StuckDoorPuzzle>();
                if (door != null && door.isSolved)
                {
                    step = Step.Done;
                    ShowCard("Unlocked", "Explore the library", showCheck: true);
                    StartCoroutine(FadeOutForever());
                }
                break;
            }
        }
    }

    private IEnumerator AdvanceAfterCheck(string nextTitle, string nextBody, Step stay)
    {
        yield return new WaitForSeconds(0.85f);
        if (step == stay || step == Step.CollectKey || step == Step.Door)
            ShowCard(nextTitle, nextBody, showCheck: false);
    }

    private IEnumerator FadeCard(float dur, bool advanceOnComplete)
    {
        fading = true;
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            if (group != null) group.alpha = 1f - t / dur;
            yield return null;
        }
        fading = false;
        if (group != null && step != Step.Done)
            group.alpha = 0f;
    }

    private IEnumerator FadeOutForever()
    {
        yield return new WaitForSeconds(1.8f);
        float t = 0f;
        while (t < 0.55f)
        {
            t += Time.deltaTime;
            if (group != null) group.alpha = 1f - t / 0.55f;
            yield return null;
        }
        GameSettings.TutorialCompleted = true;
        if (group != null) group.gameObject.SetActive(false);
        enabled = false;
    }
}

/// <summary>Top-level proxy so Unity can AddComponent it (nested MonoBehaviours are illegal).</summary>
public class TutorialCoachProxy : MonoBehaviour
{
    private TutorialCoach owner;
    public void Bind(TutorialCoach o) => owner = o;
    private void Update() => owner?.TickProxy();
}
