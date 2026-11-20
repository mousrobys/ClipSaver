using System.Drawing.Drawing2D;

namespace ClipSaver.UI;

public class RoundedPanel : Panel
{
    private bool _hovering;
    private readonly System.Windows.Forms.Timer _hoverTimer;

    public int Radius { get; set; } = 12;
    public Color FillColor { get; set; } = Color.Transparent;
    public Color BorderColor { get; set; } = Color.Transparent;
    public Color HoverFillColor { get; set; } = Color.Transparent;
    public Color HoverBorderColor { get; set; } = Color.Transparent;
    public bool HoverEnabled { get; set; }

    public RoundedPanel()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        _hoverTimer = new System.Windows.Forms.Timer { Interval = 40 };
        _hoverTimer.Tick += (_, _) => UpdateHoverState();
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        if (HoverEnabled) _hoverTimer.Start();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        if (HoverEnabled) _hoverTimer.Start();
    }

    protected override void OnControlAdded(ControlEventArgs e)
    {
        base.OnControlAdded(e);
        if (e.Control is null) return;
        e.Control.MouseEnter += (_, _) =>
        {
            if (HoverEnabled) _hoverTimer.Start();
        };
    }

    private void UpdateHoverState()
    {
        if (!HoverEnabled)
        {
            _hoverTimer.Stop();
            return;
        }

        bool inside;
        try
        {
            var p = PointToClient(Cursor.Position);
            inside = ClientRectangle.Contains(p);
        }
        catch
        {
            inside = false;
        }

        if (inside != _hovering)
        {
            _hovering = inside;
            Invalidate();
        }

        if (!inside) _hoverTimer.Stop();
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        if (BackColor.A == 0)
        {
            e.Graphics.Clear(Parent?.BackColor ?? SystemColors.Control);
            return;
        }
        base.OnPaintBackground(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        var fill = _hovering && HoverFillColor.A > 0 ? HoverFillColor : FillColor;
        var border = _hovering && HoverBorderColor.A > 0 ? HoverBorderColor : BorderColor;

        if (fill.A > 0) Draw.FillRounded(g, rect, Radius, fill);
        if (border.A > 0) Draw.StrokeRounded(g, rect, Radius, border);

        base.OnPaint(e);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _hoverTimer.Dispose();
        base.Dispose(disposing);
    }
}

