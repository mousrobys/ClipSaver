using System.Drawing.Drawing2D;
using ClipSaver.Services;

namespace ClipSaver.UI;

public static class Draw
{
    public static readonly Font GlyphSmall = new("Segoe MDL2 Assets", 10F);
    public static readonly Font GlyphMedium = new("Segoe MDL2 Assets", 12F);
    public static readonly Font GlyphLarge = new("Segoe MDL2 Assets", 16F);
    public static readonly Font GlyphHuge = new("Segoe MDL2 Assets", 34F);

    public static GraphicsPath Rounded(Rectangle rect, int radius)
    {
        int d = Math.Max(1, Math.Min(radius * 2, Math.Min(rect.Width, rect.Height)));
        var path = new GraphicsPath();
        if (radius <= 0)
        {
            path.AddRectangle(rect);
            return path;
        }
        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    public static void FillRounded(Graphics g, Rectangle rect, int radius, Color color)
    {
        if (rect.Width <= 0 || rect.Height <= 0) return;
        using var path = Rounded(rect, radius);
        using var brush = new SolidBrush(color);
        g.FillPath(brush, path);
    }

    public static void StrokeRounded(Graphics g, Rectangle rect, int radius, Color color, float width = 1f)
    {
        if (rect.Width <= 0 || rect.Height <= 0) return;
        using var path = Rounded(rect, radius);
        using var pen = new Pen(color, width);
        g.DrawPath(pen, path);
    }

    public static Color Blend(Color a, Color b, float t)
    {
        t = Math.Clamp(t, 0f, 1f);
        return Color.FromArgb(
            (int)(a.A + (b.A - a.A) * t),
            (int)(a.R + (b.R - a.R) * t),
            (int)(a.G + (b.G - a.G) * t),
            (int)(a.B + (b.B - a.B) * t));
    }

    public static Color Alpha(Color color, int alpha) => Color.FromArgb(Math.Clamp(alpha, 0, 255), color);

    public static string FormatRelativeTime(DateTime time) => Loc.RelativeTime(time);

    public static string TypeName(Models.ClipboardItemType type) => Loc.TypeName(type);

    public static string TypeGlyph(Models.ClipboardItemType type) => type switch
    {
        Models.ClipboardItemType.Text => "\uE8A5",
        Models.ClipboardItemType.Link => "\uE71B",
        Models.ClipboardItemType.Image => "\uE91B",
        _ => "\uE8A5"
    };
}
