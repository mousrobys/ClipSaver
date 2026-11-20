using System.Drawing.Drawing2D;

namespace ClipSaver.UI;

public class SmoothScrollPanel : Control
{
    private int _offset;
    private bool _dragging;
    private int _dragStartY;
    private int _dragStartOffset;
    private bool _thumbHover;

    public AppTheme Theme { get; set; } = AppTheme.Dark();
    public FlowLayoutPanel Content { get; }

    public SmoothScrollPanel()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor | ControlStyles.Selectable, true);
        BackColor = Color.Transparent;
        TabStop = false;

        Content = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = false,
            AutoScroll = false,
            BackColor = Color.Transparent,
            TabStop = false
        };
        Content.Padding = new Padding(0);
        Controls.Add(Content);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        RefreshLayout();
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        e.Graphics.Clear(Parent?.BackColor ?? Theme.Background);
        Content.BackColor = Parent?.BackColor ?? Theme.Background;
    }

    public void RefreshLayout()
    {
        if (Width <= 0) return;
        int contentWidth = Math.Max(120, ClientSize.Width - 12);
        Content.SuspendLayout();
        Content.SetBounds(0, 0, contentWidth, Math.Max(0, Content.Height));
        foreach (Control child in Content.Controls)
        {
            child.Width = Math.Max(80, contentWidth - 8 - child.Margin.Horizontal);
        }

        int total = Content.Padding.Vertical + 4;
        foreach (Control child in Content.Controls)
        {
            total += child.Height + child.Margin.Vertical;
        }

        Content.Height = Math.Max(1, total);
        Content.PerformLayout();
        Content.ResumeLayout(true);

        _offset = Math.Clamp(_offset, 0, MaxOffset);
        Content.Top = -_offset;
        Invalidate();
    }

    private int MaxOffset => Math.Max(0, Content.Height - ClientSize.Height + 8);

    public void ScrollToTop()
    {
        ScrollTo(0);
    }

    public void ScrollBy(int wheelDelta)
    {
        ScrollTo(_offset - (wheelDelta / 120) * 72);
    }

    private void ScrollTo(int value)
    {
        int clamped = Math.Clamp(value, 0, MaxOffset);
        if (clamped == _offset) return;
        _offset = clamped;
        Content.Top = -_offset;
        Invalidate();
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        ScrollBy(e.Delta);
    }

    private (Rectangle track, Rectangle thumb) GetScrollGeometry()
    {
        int trackWidth = 6;
        int trackX = Width - trackWidth - 4;
        var track = new Rectangle(trackX, 6, trackWidth, Math.Max(10, Height - 12));
        int max = MaxOffset;
        if (max <= 0) return (track, Rectangle.Empty);

        int viewport = Math.Max(1, Height);
        int contentHeight = Math.Max(viewport, Content.Height);
        int thumbHeight = Math.Max(34, track.Height * viewport / contentHeight);
        int travel = track.Height - thumbHeight;
        int thumbY = track.Y + (int)(travel * ((double)_offset / max));
        var thumb = new Rectangle(track.X, thumbY, track.Width, thumbHeight);
        return (track, thumb);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var (track, thumb) = GetScrollGeometry();
        if (thumb.Height > 0)
        {
            var trackColor = Draw.Alpha(Theme.Border, Theme.IsDark ? 90 : 60);
            Draw.FillRounded(g, track, 3, trackColor);
            var thumbColor = _dragging || _thumbHover ? Theme.Accent : Theme.ScrollThumb;
            Draw.FillRounded(g, thumb, 3, thumbColor);
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        var (_, thumb) = GetScrollGeometry();
        bool overThumb = thumb.Height > 0 && thumb.Contains(e.Location);
        bool overTrack = thumb.Height > 0 && e.X >= Width - 16;
        if (overThumb != _thumbHover)
        {
            _thumbHover = overThumb;
            Invalidate();
        }
        Cursor = overTrack ? Cursors.Hand : Cursors.Default;

        if (_dragging && thumb.Height > 0)
        {
            int max = MaxOffset;
            var (track, _) = GetScrollGeometry();
            int travel = Math.Max(1, track.Height - thumb.Height);
            int deltaY = e.Y - _dragStartY;
            ScrollTo(_dragStartOffset + (int)((double)deltaY / travel * max));
        }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button != MouseButtons.Left) return;
        Focus();
        var (_, thumb) = GetScrollGeometry();
        if (thumb.Height == 0) return;
        if (thumb.Contains(e.Location))
        {
            _dragging = true;
            _dragStartY = e.Y;
            _dragStartOffset = _offset;
        }
        else if (e.X >= Width - 16)
        {
            ScrollTo(_offset + (e.Y < thumb.Y ? -ClientSize.Height * 9 / 10 : ClientSize.Height * 9 / 10));
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        _dragging = false;
        Invalidate();
    }
}


