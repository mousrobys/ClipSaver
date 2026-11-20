using System.Drawing.Drawing2D;

namespace ClipSaver.UI;

public static class AppIcons
{
    public static Bitmap Glyph(string glyph, Color color, int size = 18)
    {
        var bmp = new Bitmap(size, size);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        TextRenderer.DrawText(g, glyph, Draw.GlyphSmall,
            new Rectangle(0, 0, size, size), color,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
        return bmp;
    }

    public static Bitmap FromIcon(Icon? icon, int size = 28)
    {
        var bmp = new Bitmap(size, size);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.Clear(Color.Transparent);
        if (icon is not null)
        {
            using var source = icon.ToBitmap();
            g.DrawImage(source, 0, 0, size, size);
        }
        return bmp;
    }
}
