using UnityEngine;

/// <summary>
/// Persistent accessibility / presentation settings for mobile UX.
/// </summary>
public static class GameSettings
{
    private const string PrefBrightness = "lk_brightness";
    private const string PrefUiScale = "lk_ui_scale";
    private const string PrefLeftHanded = "lk_left_handed";
    private const string PrefSubtitleScale = "lk_subtitle_scale";
    private const string PrefColorblind = "lk_colorblind";
    private const string PrefTutorialDone = "lk_tutorial_done_v2";

    public static float Brightness
    {
        get => PlayerPrefs.GetFloat(PrefBrightness, 1.22f); // default +22%
        set { PlayerPrefs.SetFloat(PrefBrightness, Mathf.Clamp(value, 0.7f, 1.6f)); PlayerPrefs.Save(); }
    }

    public static float UiScale
    {
        get => PlayerPrefs.GetFloat(PrefUiScale, 1f);
        set { PlayerPrefs.SetFloat(PrefUiScale, Mathf.Clamp(value, 0.85f, 1.35f)); PlayerPrefs.Save(); }
    }

    public static bool LeftHanded
    {
        get => PlayerPrefs.GetInt(PrefLeftHanded, 0) == 1;
        set { PlayerPrefs.SetInt(PrefLeftHanded, value ? 1 : 0); PlayerPrefs.Save(); }
    }

    public static float SubtitleScale
    {
        get => PlayerPrefs.GetFloat(PrefSubtitleScale, 1f);
        set { PlayerPrefs.SetFloat(PrefSubtitleScale, Mathf.Clamp(value, 0.85f, 1.4f)); PlayerPrefs.Save(); }
    }

    /// <summary>0 = off, 1 = deuteranopia-friendly gold/cyan remap.</summary>
    public static int ColorblindMode
    {
        get => PlayerPrefs.GetInt(PrefColorblind, 0);
        set { PlayerPrefs.SetInt(PrefColorblind, Mathf.Clamp(value, 0, 1)); PlayerPrefs.Save(); }
    }

    public static bool TutorialCompleted
    {
        get => PlayerPrefs.GetInt(PrefTutorialDone, 0) == 1;
        set { PlayerPrefs.SetInt(PrefTutorialDone, value ? 1 : 0); PlayerPrefs.Save(); }
    }

    public static Color AccentColor =>
        ColorblindMode == 1
            ? new Color(0.35f, 0.85f, 0.95f, 1f)
            : LockeKeyUITheme.LKGold;
}
