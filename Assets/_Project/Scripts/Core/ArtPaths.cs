/// <summary>
/// Compile-time constants for all runtime-loaded art paths under Resources/Art/.
/// Pass these to Resources.Load<Sprite>() — no file extension, no leading slash.
/// </summary>
public static class ArtPaths
{
    // ── Keys ──────────────────────────────────────────────────────────────
    public const string KeyAnywhere  = "Art/Keys/key_anywhere";
    public const string KeyHead      = "Art/Keys/key_head";
    public const string KeyMending   = "Art/Keys/key_mending";
    public const string KeyOmega     = "Art/Keys/key_omega";
    public const string KeyGhost     = "Art/Keys/key_ghost";
    public const string KeyShadow    = "Art/Keys/key_shadow";
    public const string KeyEcho      = "Art/Keys/key_echo";
    public const string KeyMatchstick = "Art/Keys/key_matchstick";
    public const string KeyMirror    = "Art/Keys/key_mirror";
    public const string KeyMusicBox  = "Art/Keys/key_music_box";
    public const string KeyAnimal    = "Art/Keys/key_animal";
    public const string KeyIdentity  = "Art/Keys/key_identity";
    public const string KeyAlpha     = "Art/Keys/key_alpha";

    // ── Backgrounds ───────────────────────────────────────────────────────
    public const string BgFoyerPortrait  = "Art/Backgrounds/bg_keyhouse_foyer_9x16";
    public const string BgFoyerLandscape = "Art/Backgrounds/bg_keyhouse_foyer_16x9";
    public const string BgWellhouse      = "Art/Backgrounds/bg_wellhouse_exterior";
    public const string BgBlackDoor      = "Art/Backgrounds/bg_black_door_chamber";

    // ── Storyboard ────────────────────────────────────────────────────────
    public const string Story01 = "Art/Storyboard/story_01_arrival";
    public const string Story02 = "Art/Storyboard/story_02_first_discovery";
    public const string Story03 = "Art/Storyboard/story_03_wellhouse_echo";
    public const string Story04 = "Art/Storyboard/story_04_black_door";

    // ── UI ────────────────────────────────────────────────────────────────
    public const string UiKeySlotEmpty = "Art/UI/ui_key_slot_empty";
    public const string UiBtnPrimary   = "Art/UI/ui_btn_primary";
    public const string UiCodexPanel   = "Art/UI/ui_codex_panel";

    // ── Key catalog: keyId → sprite path ─────────────────────────────────
    public static string KeySpriteForId(string keyId) => keyId switch
    {
        "house"     => "Art/Sprites/Keys/house_key",
        "anywhere"  => KeyAnywhere,
        "head"      => KeyHead,
        "mending"   => KeyMending,
        "omega"     => KeyOmega,
        "ghost"     => KeyGhost,
        "shadow"    => KeyShadow,
        "echo"      => KeyEcho,
        "matchstick"=> KeyMatchstick,
        "mirror"    => KeyMirror,
        "music_box" => KeyMusicBox,
        "animal"    => KeyAnimal,
        "identity"  => KeyIdentity,
        "alpha"     => KeyAlpha,
        _           => UiKeySlotEmpty
    };

    public static readonly string[] AllKeyIds =
    {
        "anywhere", "head", "mending", "omega", "ghost",
        "shadow", "echo", "matchstick", "mirror", "music_box",
        "animal", "identity", "alpha"
    };

    public static string KeyDisplayName(string keyId) => keyId switch
    {
        "house"      => "House Key",
        "anywhere"   => "Anywhere Key",
        "head"       => "Head Key",
        "mending"    => "Mending Key",
        "omega"      => "Omega Key",
        "ghost"      => "Ghost Key",
        "shadow"     => "Shadow Key",
        "echo"       => "Echo Key",
        "matchstick" => "Matchstick Key",
        "mirror"     => "Mirror Key",
        "music_box"  => "Music Box Key",
        "animal"     => "Animal Key",
        "identity"   => "Identity Key",
        "alpha"      => "Alpha Key",
        _            => "Unknown Key"
    };
}
