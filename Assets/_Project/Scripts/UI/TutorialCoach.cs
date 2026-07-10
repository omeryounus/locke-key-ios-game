using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Progressive one-shot tutorial: move → door → interact. Fades forever after completion.
/// </summary>
public class TutorialCoach : MonoBehaviour
{
    private enum Step { Move, CollectKey, Door, Interact, Done }

    private Step step = Step.Move;
    private CanvasGroup group;
    private Text label;
    private TouchGameplayController gameplay;
    private float stepTimer;
    private bool built;

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
                Show("Hold ◀ ▶ to move");
                break;
            }

            yield return null;
        }
    }

    private void Build(Transform parent, Font font)
    {
        var go = new GameObject("TutorialCoach", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.55f);
        rect.anchorMax = new Vector2(0.5f, 0.55f);
        rect.sizeDelta = new Vector2(300f, 44f);
        go.GetComponent<Image>().color = new Color(0.05f, 0.06f, 0.1f, 0.9f);
        group = go.GetComponent<CanvasGroup>();

        var textGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
        textGo.transform.SetParent(go.transform, false);
        label = textGo.GetComponent<Text>();
        label.font = font;
        label.fontSize = 15;
        label.fontStyle = FontStyle.Bold;
        label.color = Color.white;
        label.alignment = TextAnchor.MiddleCenter;
        var tRect = textGo.GetComponent<RectTransform>();
        tRect.anchorMin = Vector2.zero;
        tRect.anchorMax = Vector2.one;
        tRect.offsetMin = tRect.offsetMax = Vector2.zero;

        // Attach component to coach object for lifecycle
        var self = go.AddComponent<TutorialCoachProxy>();
        self.Bind(this);
    }

    private void Show(string msg)
    {
        if (label != null) label.text = msg;
        if (group != null)
        {
            group.alpha = 1f;
            group.gameObject.SetActive(true);
        }
        stepTimer = 0f;
    }

    public void TickProxy()
    {
        if (GameSettings.TutorialCompleted || group == null) return;
        stepTimer += Time.deltaTime;

        switch (step)
        {
            case Step.Move:
                if (gameplay != null && Mathf.Abs(gameplay.MoveInput) > 0.2f && stepTimer > 0.4f)
                {
                    step = Step.CollectKey;
                    Show("Walk to the glowing key · Tap Interact");
                    FindFirstObjectByType<GameplayHUD>()?.SetControlVisibility(interact: true);
                }
                break;
            case Step.CollectKey:
            {
                var inv = FindFirstObjectByType<PlayerInventory>();
                if (inv != null && inv.HasHouseKey)
                {
                    step = Step.Door;
                    Show("Go to the highlighted Front Door");
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
                    Show("Tap Interact to unlock");
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
                    StartCoroutine(FadeOutForever());
                }
                break;
            }
        }
    }

    private IEnumerator FadeOutForever()
    {
        Show("Nice — explore the library ahead");
        yield return new WaitForSeconds(2.2f);
        float t = 0f;
        while (t < 0.6f)
        {
            t += Time.deltaTime;
            if (group != null) group.alpha = 1f - t / 0.6f;
            yield return null;
        }

        GameSettings.TutorialCompleted = true;
        if (group != null) group.gameObject.SetActive(false);
        enabled = false;
    }

    /// <summary>Lives on the built UI object and forwards Update.</summary>
    private class TutorialCoachProxy : MonoBehaviour
    {
        private TutorialCoach owner;
        public void Bind(TutorialCoach o) => owner = o;
        private void Update() => owner?.TickProxy();
    }
}
