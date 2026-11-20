namespace ClipSaver.UI;

public sealed class AppTheme
{
    public bool IsDark { get; init; }
    public Color Background { get; init; }
    public Color Sidebar { get; init; }
    public Color Surface { get; init; }
    public Color Card { get; init; }
    public Color CardHover { get; init; }
    public Color Border { get; init; }
    public Color Text { get; init; }
    public Color TextMuted { get; init; }
    public Color TextFaint { get; init; }
    public Color Accent { get; init; }
    public Color AccentText { get; init; }
    public Color AccentSoft { get; init; }
    public Color Danger { get; init; }
    public Color DangerSoft { get; init; }
    public Color Success { get; init; }
    public Color Chip { get; init; }
    public Color ChipSelected { get; init; }
    public Color Input { get; init; }
    public Color ScrollThumb { get; init; }

    public static AppTheme Dark() => new()
    {
        IsDark = true,
        Background = Color.FromArgb(17, 18, 22),
        Sidebar = Color.FromArgb(23, 24, 29),
        Surface = Color.FromArgb(27, 28, 34),
        Card = Color.FromArgb(34, 35, 42),
        CardHover = Color.FromArgb(41, 42, 51),
        Border = Color.FromArgb(52, 54, 64),
        Text = Color.FromArgb(241, 242, 247),
        TextMuted = Color.FromArgb(154, 156, 170),
        TextFaint = Color.FromArgb(108, 110, 124),
        Accent = Color.FromArgb(96, 205, 255),
        AccentText = Color.FromArgb(7, 30, 43),
        AccentSoft = Color.FromArgb(26, 51, 66),
        Danger = Color.FromArgb(255, 122, 122),
        DangerSoft = Color.FromArgb(58, 30, 34),
        Success = Color.FromArgb(92, 220, 150),
        Chip = Color.FromArgb(30, 31, 38),
        ChipSelected = Color.FromArgb(26, 51, 66),
        Input = Color.FromArgb(30, 31, 38),
        ScrollThumb = Color.FromArgb(72, 74, 86)
    };

    public static AppTheme Light() => new()
    {
        IsDark = false,
        Background = Color.FromArgb(243, 244, 247),
        Sidebar = Color.FromArgb(250, 250, 252),
        Surface = Color.White,
        Card = Color.White,
        CardHover = Color.FromArgb(246, 248, 251),
        Border = Color.FromArgb(224, 225, 231),
        Text = Color.FromArgb(24, 25, 30),
        TextMuted = Color.FromArgb(103, 105, 118),
        TextFaint = Color.FromArgb(142, 144, 156),
        Accent = Color.FromArgb(0, 103, 192),
        AccentText = Color.White,
        AccentSoft = Color.FromArgb(226, 240, 253),
        Danger = Color.FromArgb(196, 43, 43),
        DangerSoft = Color.FromArgb(252, 235, 235),
        Success = Color.FromArgb(16, 137, 80),
        Chip = Color.FromArgb(236, 237, 241),
        ChipSelected = Color.FromArgb(226, 240, 253),
        Input = Color.White,
        ScrollThumb = Color.FromArgb(188, 190, 200)
    };
}
