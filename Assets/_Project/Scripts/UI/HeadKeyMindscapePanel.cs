using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Head Key Mindscape Chronological Memory Reconstruction Puzzle UI.
/// Programmatically builds and handles the touch deduction gameplay on iOS.
/// </summary>
public class HeadKeyMindscapePanel : MonoBehaviour
{
    private struct MemoryShard
    {
        public int id; // 1, 2, 3 chronologically
        public string text;
    }

    private UIManager uiManager;
    private EventBus eventBus;

    private GameObject rootPanel;
    private Text feedbackText;
    private Button[] shardButtons = new Button[3];
    private Text[] shardTexts = new Text[3];

    private List<MemoryShard> shards = new();
    private List<int> userSequence = new();
    private bool isSolved;
    private Font defaultFont;

    private readonly Color normalColor = new(0.18f, 0.20f, 0.28f, 1f);
    private readonly Color correctColor = new(0.15f, 0.45f, 0.25f, 1f);
    private readonly Color errorColor = new(0.55f, 0.15f, 0.20f, 1f);

    public void Initialize(UIManager manager)
    {
        uiManager = manager;
        eventBus = Resources.Load<EventBus>("EventBus");
        defaultFont = GetDefaultFont();

        SetupShards();
        BuildUI();
        eventBus?.MindscapeEntered();
    }

    private void SetupShards()
    {
        shards.Add(new MemoryShard { id = 1, text = "1. Rendell finds the Whispering Iron in the basement of Keyhouse." });
        shards.Add(new MemoryShard { id = 2, text = "2. Rendell crafts the Omega Key and seals the Black Door in the caves." });
        shards.Add(new MemoryShard { id = 3, text = "3. Rendell hides the remaining keys across the house to protect his children." });
    }

    private void BuildUI()
    {
        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        rootPanel = CreatePanel(canvas.transform, "MindscapeOverlay", new Color(0.02f, 0.02f, 0.05f, 0.88f),
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        var centerPanel = CreatePanel(rootPanel.transform, "CenterPanel", new Color(0.08f, 0.08f, 0.12f, 1f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-460f, -280f), new Vector2(920f, 560f));

        var outline = centerPanel.AddComponent<Outline>();
        outline.effectColor = new Color(0.45f, 0.95f, 0.82f, 0.45f);
        outline.effectDistance = new Vector2(2f, 2f);

        var title = CreateText(centerPanel.transform, "Title", defaultFont, 28, TextAnchor.UpperCenter,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -32f), new Vector2(850f, 40f),
            new Color(0.45f, 0.95f, 0.82f, 1f));
        title.text = "HEAD KEY MINDSCAPE";

        var subtitle = CreateText(centerPanel.transform, "Subtitle", defaultFont, 18, TextAnchor.UpperCenter,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -76f), new Vector2(850f, 30f),
            new Color(0.7f, 0.72f, 0.76f, 1f));
        subtitle.text = "Reconstruct the timeline chronologically (1 to 3) to unlock Rendell's memory.";

        feedbackText = CreateText(centerPanel.transform, "Feedback", defaultFont, 20, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -120f), new Vector2(850f, 40f),
            Color.white);
        feedbackText.text = "Tap the shards in chronological order.";

        int[] displayIndices = { 1, 0, 2 };

        float startY = -180f;
        float spacingY = -90f;

        for (int i = 0; i < 3; i++)
        {
            int index = displayIndices[i];
            var shard = shards[index];
            int currentSlot = i;

            string cleanText = shard.text.Substring(3);

            var btnGo = CreateTapButton(centerPanel.transform, "Shard_" + shard.id, defaultFont, normalColor, Color.white,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-400f, startY + (i * spacingY)), new Vector2(800f, 76f),
                () => OnShardTapped(shard.id, currentSlot));

            shardButtons[currentSlot] = btnGo.GetComponent<Button>();
            shardTexts[currentSlot] = btnGo.transform.Find("Label").GetComponent<Text>();
            shardTexts[currentSlot].text = cleanText;
            shardTexts[currentSlot].alignment = TextAnchor.MiddleLeft;
            shardTexts[currentSlot].rectTransform.offsetMin = new Vector2(24f, 0f);
        }

        CreateTapButton(centerPanel.transform, "Exit Mindscape", defaultFont, new Color(0.22f, 0.24f, 0.28f, 1f), Color.white,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-120f, 24f), new Vector2(240f, 54f),
            Close);
    }

    private void OnShardTapped(int shardId, int slotIndex)
    {
        if (isSolved || userSequence.Contains(shardId)) return;

        int expectedNext = userSequence.Count + 1;
        if (shardId == expectedNext)
        {
            userSequence.Add(shardId);
            shardButtons[slotIndex].GetComponent<Image>().color = correctColor;
            feedbackText.text = "Sequence accepted... continue.";
            FindFirstObjectByType<GameAudioController>()?.PlayDoorUnlock();

            if (userSequence.Count == 3)
            {
                ResolveSuccess();
            }
        }
        else
        {
            StartCoroutine(ErrorFlashRoutine(slotIndex));
        }
    }

    private System.Collections.IEnumerator ErrorFlashRoutine(int failedSlot)
    {
        feedbackText.text = "Timeline error! Re-evaluating chronological bounds...";
        feedbackText.color = errorColor;
        FindFirstObjectByType<GameAudioController>()?.PlayDoorRattle();
        GameHaptics.TriggerHapticStall();

        shardButtons[failedSlot].GetComponent<Image>().color = errorColor;

        yield return new WaitForSeconds(0.8f);

        for (int i = 0; i < 3; i++)
        {
            shardButtons[i].GetComponent<Image>().color = normalColor;
        }

        userSequence.Clear();
        feedbackText.text = "Reconstruct the timeline chronologically.";
        feedbackText.color = Color.white;
    }

    private void ResolveSuccess()
    {
        isSolved = true;
        feedbackText.text = "Timeline restored! Loading Rendell's suppressed memory...";
        feedbackText.color = new Color(0.45f, 0.95f, 0.82f, 1f);

        FindFirstObjectByType<GameAudioController>()?.PlayMemoryTransition();

        var puzzle = FindFirstObjectByType<MemoryFragmentPuzzle>();
        if (puzzle != null)
        {
            puzzle.SolveFromUI();
        }

        Invoke(nameof(ShowMemoryText), 1.5f);
    }

    private void ShowMemoryText()
    {
        if (uiManager != null)
        {
            uiManager.OpenMemoryView();
        }
        Close();
    }

    private void Close()
    {
        eventBus?.MindscapeExited();
        if (rootPanel != null)
        {
            Destroy(rootPanel);
        }
        var controller = FindFirstObjectByType<TouchGameplayController>();
        controller?.SetInputLocked(false);
        Destroy(gameObject);
    }

    private static GameObject CreatePanel(Transform parent, string name, Color color,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.color = color;
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = sizeDelta;
        return go;
    }

    private static Text CreateText(Transform parent, string name, Font font, int fontSize, TextAnchor alignment,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var text = go.GetComponent<Text>();
        text.font = font;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = anchorMin;
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = sizeDelta;
        return text;
    }

    private static GameObject CreateTapButton(Transform parent, string label, Font font, Color bg, Color textColor,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta, UnityEngine.Events.UnityAction onTap)
    {
        var go = new GameObject(label + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.color = bg;

        var button = go.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onTap);

        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = sizeDelta;

        CreateText(go.transform, "Label", font, 20, TextAnchor.MiddleCenter,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, textColor).text = label;

        return go;
    }

    private Font GetDefaultFont()
    {
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return font;
    }
}
