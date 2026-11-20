using System.Drawing.Drawing2D;

namespace ClipSaver.UI;

public class NumberStepper : Control
{
    private int _value;
    private bool _hoverLeft;
    private bool _hoverRight;

    public event EventHandler? ValueChanged;

    public AppTheme Theme { get; set; } = AppTheme.Dark();
    public int Minimum { get; set; }
    public int Maximum { get; set; } = 100;
    public int Step { get; set; } = 1;

    public int Value
    {
        get => _value;
        set
        {
            int clamped = Math.Clamp(value, Minimum, Maximum);
            if (clamped == _value) return;
            _value = clamped;
            ValueChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }
    }

    public NumberStepper()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
        Size = new Size(138, 38);
        Cursor = Cursors.Hand;
    }

    public void SetValueSilent(int value)
    {
        _value = Math.Clamp(value, Minimum, Maximum);
        Invalidate();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        bool left = e.X < Width / 3;
        bool right = e.X > Width * 2 / 3;
        if (left != _hoverLeft || right != _hoverRight)
        {
            _hoverLeft = left;
            _hoverRight = right;
            Invalidate();
        }
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _hoverLeft = false;
        _hoverRight = false;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button != MouseButtons.Left) return;
        if (e.X < Width / 3) Value -= Step;
        else if (e.X > Width * 2 / 3) Value += Step;
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        e.Graphics.Clear(Parent?.BackColor ?? Theme.Background);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        Draw.FillRounded(g, rect, 10, Theme.Input);
        Draw.StrokeRounded(g, rect, 10, Theme.Border);

        var muted = Theme.TextMuted;
        var active = Theme.Accent;
        Draw.FillRounded(g, new Rectangle(4, 4, Width / 3 - 6, Height - 9), 7,
            _hoverLeft ? Draw.Alpha(Theme.Text, Theme.IsDark ? 22 : 12) : Color.Transparent);
        Draw.FillRounded(g, new Rectangle(Width * 2 / 3 + 2, 4, Width / 3 - 6, Height - 9), 7,
            _hoverRight ? Draw.Alpha(Theme.Text, Theme.IsDark ? 22 : 12) : Color.Transparent);

        TextRenderer.DrawText(g, "\uE738", Draw.GlyphSmall,
            new Rectangle(4, 0, Width / 3 - 4, Height), _hoverLeft ? active : muted,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        TextRenderer.DrawText(g, "\uE710", Draw.GlyphSmall,
            new Rectangle(Width * 2 / 3, 0, Width / 3 - 4, Height), _hoverRight ? active : muted,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        TextRenderer.DrawText(g, _value.ToString(), Font,
            new Rectangle(Width / 3, 0, Width / 3 + 1, Height), Theme.Text,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
    }
}


