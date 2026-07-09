using UnityEngine;

/// <summary>
/// Design tokens from ux_design_system_board — programmatic uGUI targets (not screenshot textures).
/// Reference canvas: 393 × 852 portrait (see LockeUILayout).
/// </summary>
public static class LockeKeyUITheme
{
    public const float RefWidth = 393f;
    public const float RefHeight = 852f;

    // ── Colors ──────────────────────────────────────────────────────────
    public static readonly Color LKInk = Hex("#121218");
    public static readonly Color LKWood = Hex("#4A3728");
    public static readonly Color LKMoon = Hex("#2B3D5C");
    public static readonly Color LKGold = Hex("#D4AF37");
    public static readonly Color LKIron = Hex("#5C6B7A");
    public static readonly Color Success = Hex("#3D9A5F");
    public static readonly Color BodyText = Hex("#C8C8D0");
    public static readonly Color CaptionText = Hex("#888899");
    public static readonly Color ButtonOnGold = Hex("#1A1A1A");
    public static readonly Color White = Color.white;

    public static Color OverlayScrim => new(0f, 0f, 0f, 0.40f);

    // Sheet gradient endpoints (wood / ink)
    public static readonly Color SheetTop = LKWood;
    public static readonly Color SheetBottom = new(0.07f, 0.08f, 0.12f, 1f);

    // ── Typography (ref 393w) ───────────────────────────────────────────
    public const int DisplaySize = 30;
    public const int TitleSize = 21;
    public const int BodySize = 16;
    public const int CaptionSize = 12;
    public const int ButtonSize = 17;

    // ── Layout ──────────────────────────────────────────────────────────
    public const float PrimaryButtonHeight = 48f;
    public const float PrimaryButtonRadius = 12f;
    public const float HudBarHeight = 44f;
    public const float SheetTopRadius = 16f;
    public const float KeySlotSize = 72f;
    public const float KeySlotRingWidth = 3f;
    public const float ToastBottomInset = 24f;
    public const float ChapterCardThumbAspect = 16f / 9f;

    public static Color Hex(string hex)
    {
        if (hex.StartsWith("#")) hex = hex[1..];
        if (hex.Length == 6)
        {
            var r = int.Parse(hex[..2], System.Globalization.NumberStyles.HexNumber);
            var g = int.Parse(hex[2..4], System.Globalization.NumberStyles.HexNumber);
            var b = int.Parse(hex[4..6], System.Globalization.NumberStyles.HexNumber);
            return new Color(r / 255f, g / 255f, b / 255f, 1f);
        }

        return Color.magenta;
    }
}