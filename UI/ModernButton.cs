using System.Drawing.Drawing2D;

namespace ClipSaver.UI;

public enum ButtonStyle
{
    Ghost,
    Subtle,
    Accent,
    Danger,
    Nav,
    Chip
}

public class ModernButton : Control
{
    private bool _hover;
    private bool _down;

    public AppTheme Theme { get; set; } = AppTheme.Dark();
    public ButtonStyle Style { get; set; } = ButtonStyle.Subtle;
    public string? IconGlyph { get; set; }
    public bool Selected { get; set; }
    public bool IconOnly { get; set; }
    public int Radius { get; set; } = 10;
    public bool DangerAction { get; set; }

    public ModernButton()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
        Cursor = Cursors.Hand;
        Font = new Font("Segoe UI", 9.75F);
        Height = 38;
        BackColor = Color.Transparent;
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _hover = true;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _hover = false;
        _down = false;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left) { _down = true; Invalidate(); }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        _down = false;
        Invalidate();
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        var color = Parent?.BackColor ?? Theme.Background;
        e.Graphics.Clear(color);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        ResolveColors(out var fill, out var border, out var fore);

        if (DangerAction && (_hover || _down))
        {
            fore = Theme.Danger;
            fill = Theme.DangerSoft;
        }

        if (fill.A > 0) Draw.FillRounded(g, rect, Radius, fill);
        if (border.A > 0) Draw.StrokeRounded(g, rect, Radius, border);

        var text = Text ?? string.Empty;
        bool hasIcon = !string.IsNullOrEmpty(IconGlyph);
        var textFlags = TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix |
                        TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine;

        if (IconOnly)
        {
            if (hasIcon)
            {
                TextRenderer.DrawText(g, IconGlyph, Draw.GlyphMedium,
                    new Rectangle(0, 0, Width, Height), fore,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            }
        }
        else if (Style == ButtonStyle.Nav)
        {
            const int iconLeft = 14;
            if (hasIcon)
            {
                TextRenderer.DrawText(g, IconGlyph, Draw.GlyphMedium,
                    new Rectangle(iconLeft, 0, 22, Height), fore,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            }
            var textRect = new Rectangle(iconLeft + 30, 0, Width - iconLeft - 34, Height);
            TextRenderer.DrawText(g, text, Font, textRect, fore,
                textFlags | TextFormatFlags.Left);
        }
        else
        {
            int iconWidth = hasIcon ? 22 : 0;
            var textSize = TextRenderer.MeasureText(g, text, Font);
            int total = iconWidth + textSize.Width;
            int startX = Math.Max(10, (Width - total) / 2);
            if (hasIcon)
            {
                TextRenderer.DrawText(g, IconGlyph, Draw.GlyphMedium,
                    new Rectangle(startX, 0, iconWidth, Height), fore,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            }
            var textRect = new Rectangle(startX + iconWidth, 0, Width - startX - iconWidth - 8, Height);
            TextRenderer.DrawText(g, text, Font, textRect, fore,
                textFlags | TextFormatFlags.Left);
        }
    }

    private void ResolveColors(out Color fill, out Color border, out Color fore)
    {
        fill = Color.Transparent;
        border = Color.Transparent;
        fore = Theme.Text;

        switch (Style)
        {
            case ButtonStyle.Ghost:
                if (_hover) fill = Draw.Alpha(Theme.Text, Theme.IsDark ? 22 : 16);
                fore = Theme.TextMuted;
                break;

            case ButtonStyle.Subtle:
                fill = Theme.Card;
                border = Theme.Border;
                fore = Theme.Text;
                if (Selected)
                {
                    fill = Theme.AccentSoft;
                    border = Draw.Alpha(Theme.Accent, 130);
                    fore = Theme.Accent;
                }
                if (_hover) fill = Selected ? Draw.Blend(Theme.AccentSoft, Theme.Accent, 0.08f) : Theme.CardHover;
                if (_down) fill = Draw.Blend(fill, Theme.Border, 0.25f);
                break;

            case ButtonStyle.Accent:
                fill = Theme.Accent;
                fore = Theme.AccentText;
                if (_hover) fill = Draw.Blend(Theme.Accent, Theme.IsDark ? Colors.White : Colors.Black, 0.08f);
                if (_down) fill = Draw.Blend(Theme.Accent, Colors.Black, 0.12f);
                break;

            case ButtonStyle.Danger:
                fill = Color.Transparent;
                border = Draw.Alpha(Theme.Danger, 120);
                fore = Theme.Danger;
                if (_hover) fill = Theme.DangerSoft;
                break;

            case ButtonStyle.Nav:
                if (Selected)
                {
                    fill = Theme.AccentSoft;
                    fore = Theme.Accent;
                }
                else
                {
                    fore = Theme.TextMuted;
                    if (_hover) fill = Draw.Alpha(Theme.Text, Theme.IsDark ? 16 : 10);
                }
                break;

            case ButtonStyle.Chip:
                if (Selected)
                {
                    fill = Theme.ChipSelected;
                    border = Draw.Alpha(Theme.Accent, 90);
                    fore = Theme.Accent;
                }
                else
                {
                    fill = Theme.Chip;
                    fore = Theme.TextMuted;
                    if (_hover)
                    {
                        fill = Theme.CardHover;
                        fore = Theme.Text;
                    }
                }
                break;
        }
    }

    private static class Colors
    {
        public static readonly Color White = Color.White;
        public static readonly Color Black = Color.Black;
    }
}


