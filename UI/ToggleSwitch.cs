using System.Drawing.Drawing2D;

namespace ClipSaver.UI;

public class ToggleSwitch : Control
{
    private bool _checked;
    private float _position;
    private readonly System.Windows.Forms.Timer _timer;

    public event EventHandler? CheckedChanged;

    public AppTheme Theme { get; set; } = AppTheme.Dark();

    public bool Checked
    {
        get => _checked;
        set
        {
            if (_checked == value) return;
            _checked = value;
            _timer.Start();
            CheckedChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }
    }

    public ToggleSwitch()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
        Size = new Size(46, 26);
        Cursor = Cursors.Hand;
        _timer = new System.Windows.Forms.Timer { Interval = 15 };
        _timer.Tick += (_, _) =>
        {
            float target = _checked ? 1f : 0f;
            if (Math.Abs(_position - target) < 0.06f)
            {
                _position = target;
                _timer.Stop();
            }
            else
            {
                _position += (target - _position) * 0.35f;
            }
            Invalidate();
        };
    }

    protected override void OnClick(EventArgs e)
    {
        base.OnClick(e);
        Checked = !Checked;
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        e.Graphics.Clear(Parent?.BackColor ?? Theme.Background);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var track = new Rectangle(1, (Height - 24) / 2, Width - 2, 24);
        var trackColor = _checked
            ? Theme.Accent
            : (Theme.IsDark ? Color.FromArgb(66, 68, 80) : Color.FromArgb(205, 207, 215));
        Draw.FillRounded(g, track, 12, trackColor);

        int knobSize = 18;
        int travel = track.Width - knobSize - 6;
        int knobX = track.X + 3 + (int)(travel * _position);
        int knobY = track.Y + 3;
        using var knob = new SolidBrush(_checked ? Theme.AccentText : Color.White);
        g.FillEllipse(knob, knobX, knobY, knobSize, knobSize);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _timer.Dispose();
        base.Dispose(disposing);
    }
}


