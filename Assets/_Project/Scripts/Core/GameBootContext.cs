/// <summary>
/// Static boot flags written before a scene transition so the new scene
/// knows how to initialize without a flash of wrong state.
/// Reset each field to default after it has been consumed.
/// </summary>
public static class GameBootContext
{
    /// <summary>
    /// When true, GrokUIFlowManager will open the S2 Chapter Map immediately
    /// on Start, before showing the Foyer gameplay.
    /// Set by TitleScreenController before loading Chapter1.
    /// </summary>
    public static bool OpenMapOnStart { get; set; }

    /// <summary>
    /// When true, TitleScreen opens the S1 story reel immediately (Replay Story).
    /// </summary>
    public static bool OpenStoryReelOnStart { get; set; }

    /// <summary>
    /// Reset all boot flags. Call at the end of the consuming scene's Start().
    /// </summary>
    public static void Reset()
    {
        OpenMapOnStart = false;
        OpenStoryReelOnStart = false;
    }
}
