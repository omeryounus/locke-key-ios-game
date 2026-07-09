#if UNITY_EDITOR
/// <summary>
/// Hi-fi UX mockup paths under Assets/ArtSource/ux/ — editor/design reference only.
/// Do not Resources.Load these; build UI programmatically to match layout.
/// </summary>
public static class UxReferencePaths
{
    public const string Root = "Assets/ArtSource/ux";

    public const string S0Splash = Root + "/ux_s0_splash.jpg";
    public const string S1StoryReel = Root + "/ux_s1_story_reel.jpg";
    public const string S2ChapterMap = Root + "/ux_s2_chapter_map.jpg";
    public const string S3FoyerHud = Root + "/ux_s3_foyer_hud.jpg";
    public const string S4KeyDiscovery = Root + "/ux_s4_key_discovery.jpg";
    public const string S5LockPuzzle = Root + "/ux_s5_lock_puzzle.jpg";
    public const string S6KeyRing = Root + "/ux_s6_key_ring.jpg";
    public const string DesignSystem = Root + "/ux_design_system_board.jpg";
    public const string LandscapeFrame = Root + "/ux_landscape_device_frame.jpg";
}
#endif