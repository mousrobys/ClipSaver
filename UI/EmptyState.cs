using System.Drawing.Drawing2D;

namespace ClipSaver.UI;

public class EmptyState : Control
{
    public AppTheme Theme { get; set; } = AppTheme.Dark();
    public string Glyph { get; set; } = "\uE81C";
    public string Title { get; set; } = "";
    public string Subtitle { get; set; } = "";

    private readonly Font _titleFont = new("Segoe UI Semibold", 14F);
    private readonly Font _subtitleFont = new("Segoe UI", 9.75F);

    public EmptyState()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.UserPaint | ControlStyles.ResizeRedraw |
                 ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        e.Graphics.Clear(Parent?.BackColor ?? Theme.Background);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        int centerY = Height / 2 - 40;

        var badgeRect = new Rectangle((Width - 84) / 2, centerY - 60, 84, 84);
        Draw.FillRounded(g, badgeRect, 24, Theme.Card);
        Draw.StrokeRounded(g, badgeRect, 24, Theme.Border);
        TextRenderer.DrawText(g, Glyph, Draw.GlyphHuge, badgeRect, Theme.TextFaint,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);

        var titleRect = new Rectangle(0, centerY + 40, Width, 30);
        TextRenderer.DrawText(g, Title, _titleFont, titleRect, Theme.Text,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);

        var subRect = new Rectangle(20, centerY + 70, Math.Max(20, Width - 40), 24);
        TextRenderer.DrawText(g, Subtitle, _subtitleFont, subRect, Theme.TextMuted,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _titleFont.Dispose();
            _subtitleFont.Dispose();
        }
        base.Dispose(disposing);
    }
}
