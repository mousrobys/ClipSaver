using System.Drawing.Drawing2D;

namespace ClipSaver.UI;

public class SearchBox : Panel
{
    private readonly TextBox _box = new();
    private readonly Label _glyph = new();
    private bool _focused;

    public event EventHandler? QueryChanged;

    public AppTheme Theme { get; set; } = AppTheme.Dark();

    public string Query
    {
        get => _box.Text;
        set => _box.Text = value;
    }

    public string Placeholder
    {
        get => _box.PlaceholderText;
        set => _box.PlaceholderText = value;
    }

    public SearchBox()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.UserPaint | ControlStyles.ResizeRedraw |
                 ControlStyles.SupportsTransparentBackColor, true);
        Height = 42;
        BackColor = Color.Transparent;
        Padding = new Padding(0);

        _glyph.AutoSize = false;
        _glyph.Width = 24;
        _glyph.Text = "\uE721";
        _glyph.Font = Draw.GlyphSmall;
        _glyph.TextAlign = ContentAlignment.MiddleCenter;
        _glyph.BackColor = Color.Transparent;

        _box.BorderStyle = BorderStyle.None;
        _box.Font = new Font("Segoe UI", 10F);
        _box.PlaceholderText = "";
        _box.TextChanged += (_, _) => QueryChanged?.Invoke(this, EventArgs.Empty);
        _box.Enter += (_, _) => { _focused = true; Invalidate(); };
        _box.Leave += (_, _) => { _focused = false; Invalidate(); };

        Controls.Add(_box);
        Controls.Add(_glyph);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        int h = Math.Min(24, Height - 8);
        _glyph.SetBounds(12, (Height - h) / 2, 22, h);
        _box.SetBounds(38, (Height - 22) / 2, Math.Max(40, Width - 50), 22);
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        e.Graphics.Clear(Parent?.BackColor ?? Theme.Background);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var rect = new Rectangle(0, 0, Width - 1, Height - 2);
        Draw.FillRounded(g, rect, 11, Theme.Input);
        var border = _focused ? Draw.Alpha(Theme.Accent, 190) : Theme.Border;
        Draw.StrokeRounded(g, rect, 11, border, _focused ? 1.4f : 1f);
    }

    public void ApplyTheme(AppTheme theme)
    {
        Theme = theme;
        _glyph.ForeColor = theme.TextFaint;
        _box.BackColor = theme.Input;
        _box.ForeColor = theme.Text;
        Invalidate();
    }
}
