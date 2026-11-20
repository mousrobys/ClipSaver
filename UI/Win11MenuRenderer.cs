using System.Drawing.Drawing2D;

namespace ClipSaver.UI;

public class Win11MenuRenderer : ToolStripProfessionalRenderer
{
    private readonly AppTheme _theme;

    public Win11MenuRenderer(AppTheme theme) : base(new AppColorTable(theme))
    {
        _theme = theme;
        RoundedEdges = true;
    }

    protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
    {
        using var brush = new SolidBrush(_theme.Card);
        e.Graphics.FillRectangle(brush, e.AffectedBounds);
    }

    protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
    {
        using var brush = new SolidBrush(_theme.Card);
        e.Graphics.FillRectangle(brush, e.AffectedBounds);
    }

    protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var rect = new Rectangle(0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1);
        using var path = Draw.Rounded(rect, 10);
        using var pen = new Pen(_theme.Border);
        g.DrawPath(pen, path);
    }

    protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
    {
        if (!e.Item.Selected || !e.Item.Enabled) return;

        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var rect = new Rectangle(3, 1, e.Item.Width - 6, e.Item.Height - 2);
        using var path = Draw.Rounded(rect, 7);
        using var brush = new SolidBrush(_theme.AccentSoft);
        g.FillPath(brush, path);
    }

    protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
    {
        e.TextColor = !e.Item.Enabled
            ? _theme.TextFaint
            : e.Item.Selected ? _theme.Accent : _theme.Text;
        base.OnRenderItemText(e);
    }

    protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
    {
        var y = e.Item.Height / 2;
        using var pen = new Pen(Draw.Alpha(_theme.Border, 180));
        e.Graphics.DrawLine(pen, 14, y, e.Item.Width - 14, y);
    }

    protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        TextRenderer.DrawText(g, "\uE73E", Draw.GlyphSmall,
            e.ImageRectangle, _theme.Accent,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
    }

    protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
    {
        e.ArrowColor = e.Item is not null && e.Item.Selected ? _theme.Accent : _theme.TextMuted;
        base.OnRenderArrow(e);
    }
}
