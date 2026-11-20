using System.Diagnostics;
using System.Drawing.Drawing2D;
using ClipSaver.Models;
using ClipSaver.Services;
using ClipSaver.UI;

namespace ClipSaver;

public class MainForm : Form, IMessageFilter
{
    private readonly DataStore _store = new();
    private AppTheme _theme = AppTheme.Dark();

    private NotifyIcon _trayIcon = null!;
    private ContextMenuStrip _trayMenu = null!;
    private ToolStripMenuItem _trayPauseItem = null!;
    private readonly ToolTip _toolTip = new();

    // Sidebar
    private readonly Panel _sidebar = new();
    private readonly Panel _logoArea = new();
    private readonly RoundedPanel _logoBadge = new();
    private readonly Label _logoGlyph = new();
    private readonly Label _appTitle = new();
    private readonly Label _appSubtitle = new();
    private readonly Panel _navPanel = new();
    private ModernButton _navAll = null!;
    private ModernButton _navFav = null!;
    private ModernButton _navSettings = null!;
    private readonly Panel _sidebarBottom = new();
    private ModernButton _pauseButton = null!;
    private readonly Label _themeLabel = new();
    private readonly ToggleSwitch _themeToggle = new();
    private readonly Label _statusLabel = new();
    private readonly Label _versionLabel = new();
    private readonly Label _hotkeyHint = new();

    // Content
    private readonly Panel _content = new();
    private readonly Panel _header = new();
    private readonly Label _headerTitle = new();
    private readonly Label _headerSubtitle = new();
    private readonly SearchBox _search = new();
    private readonly Panel _filterBar = new();
    private readonly FlowLayoutPanel _chips = new();
    private ModernButton _chipAll = null!;
    private ModernButton _chipText = null!;
    private ModernButton _chipLink = null!;
    private ModernButton _chipImage = null!;
    private readonly Label _countLabel = new();
    private readonly Panel _body = new();
    private readonly SmoothScrollPanel _scroll = new();
    private readonly EmptyState _empty = new();
    private readonly Panel _settingsPanel = new();
    private readonly SmoothScrollPanel _settingsScroll = new();

    private readonly RoundedPanel _toast = new();
    private readonly Label _toastLabel = new();

    // Settings controls
    private ToggleSwitch _tgAutoSave = null!;
    private ToggleSwitch _tgPause = null!;
    private ToggleSwitch _tgStartup = null!;
    private NumberStepper _numMax = null!;
    private NumberStepper _numRetention = null!;
    private ModernButton _btnLight = null!;
    private ModernButton _btnDark = null!;
    private ModernButton _btnLangAuto = null!;
    private ModernButton _btnLangRu = null!;
    private ModernButton _btnLangEn = null!;
    private Label _pathLabel = null!;

    private readonly Font _fontAppTitle = new("Segoe UI Semibold", 15F);
    private readonly Font _fontHeader = new("Segoe UI Semibold", 19F);
    private readonly Font _fontSection = new("Segoe UI Semibold", 11.5F);
    private readonly Font _fontRow = new("Segoe UI Semibold", 10F);
    private readonly Font _fontBody = new("Segoe UI", 9.75F);
    private readonly Font _fontSmall = new("Segoe UI", 8.75F);
    private readonly Font _fontTiny = new("Segoe UI", 8.25F);
    private readonly Font _fontPreview = new("Segoe UI", 10.25F);

    private enum ViewMode { All, Favorites, Settings }
    private ViewMode _view = ViewMode.All;
    private int _typeFilterIndex;

    private bool _isExitRequested;
    private bool _suppressClipboard;
    private bool _uiLoading;
    private bool _trayHintShown;

    private readonly System.Windows.Forms.Timer _showEventTimer = new() { Interval = 400 };
    private readonly System.Windows.Forms.Timer _refreshTimer = new() { Interval = 140 };
    private readonly System.Windows.Forms.Timer _toastTimer = new() { Interval = 1800 };
    private readonly System.Windows.Forms.Timer _clipboardSuppressTimer = new() { Interval = 550 };
    private readonly System.Windows.Forms.Timer _hotkeyGuardTimer = new() { Interval = 40 };

    private bool _hotkeyRegistered;
    private bool _hotkeyComboDown;
    private DateTime _lastHotkeyShow = DateTime.MinValue;

    public MainForm()
    {
        _store.Load();
        Loc.Initialize(_store.Settings.Language);
        _theme = _store.Settings.Theme == "Dark" ? AppTheme.Dark() : AppTheme.Light();

        Text = Loc.T("app.title");
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(980, 640);
        Size = new Size(1120, 760);
        BackColor = _theme.Background;
        Font = new Font("Segoe UI", 10F);
        KeyPreview = true;
        DoubleBuffered = true;

        try { Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

        BuildUi();
        BuildTray();

        _showEventTimer.Tick += (_, _) => CheckShowEvent();
        _showEventTimer.Start();
        _refreshTimer.Tick += (_, _) => { _refreshTimer.Stop(); if (_view != ViewMode.Settings) RefreshList(); };
        _toastTimer.Tick += (_, _) => { _toastTimer.Stop(); _toast.Visible = false; };
        _clipboardSuppressTimer.Tick += (_, _) => { _clipboardSuppressTimer.Stop(); _suppressClipboard = false; };

        Application.AddMessageFilter(this);

        Load += (_, _) => OnLoaded();
        FormClosing += OnFormClosing;
        Resize += (_, _) => LayoutUi();
    }

    // ===================== UI construction =====================

    private void BuildUi()
    {
        _sidebar.BackColor = _theme.Sidebar;
        _content.BackColor = _theme.Background;

        BuildSidebar();
        BuildContent();

        Controls.Add(_content);
        Controls.Add(_sidebar);
    }

    private void BuildSidebar()
    {
        _logoArea.BackColor = _theme.Sidebar;
        _navPanel.BackColor = _theme.Sidebar;
        _sidebarBottom.BackColor = _theme.Sidebar;

        _logoBadge.Size = new Size(46, 46);
        _logoBadge.Location = new Point(18, 27);
        _logoBadge.Radius = 14;
        _logoBadge.FillColor = _theme.Accent;
        _logoBadge.BorderColor = Color.Transparent;
        _logoBadge.BackColor = _theme.Sidebar;
        _logoGlyph.Dock = DockStyle.Fill;
        _logoGlyph.Text = "\uE8C8";
        _logoGlyph.Font = new Font("Segoe MDL2 Assets", 17F);
        _logoGlyph.ForeColor = _theme.AccentText;
        _logoGlyph.TextAlign = ContentAlignment.MiddleCenter;
        _logoGlyph.BackColor = Color.Transparent;
        _logoBadge.Controls.Add(_logoGlyph);

        _appTitle.Text = "ClipSaver";
        _appTitle.Font = _fontAppTitle;
        _appTitle.AutoSize = true;
        _appTitle.Location = new Point(78, 30);
        _appTitle.ForeColor = _theme.Text;
        _appTitle.BackColor = Color.Transparent;

        _appSubtitle.Text = Loc.T("app.subtitle");
        _appSubtitle.Font = _fontSmall;
        _appSubtitle.AutoSize = true;
        _appSubtitle.Location = new Point(79, 58);
        _appSubtitle.ForeColor = _theme.TextMuted;
        _appSubtitle.BackColor = Color.Transparent;

        _logoArea.Controls.Add(_logoBadge);
        _logoArea.Controls.Add(_appTitle);
        _logoArea.Controls.Add(_appSubtitle);

        _navAll = CreateNavButton(Loc.T("nav.all"), "\uE81C", () => SwitchView(ViewMode.All));
        _navAll.Top = 8;
        _navFav = CreateNavButton(Loc.T("nav.favorites"), "\uE734", () => SwitchView(ViewMode.Favorites));
        _navFav.Top = 62;
        _navSettings = CreateNavButton(Loc.T("nav.settings"), "\uE713", () => SwitchView(ViewMode.Settings));
        _navSettings.Top = 116;

        _navPanel.Controls.Add(_navAll);
        _navPanel.Controls.Add(_navFav);
        _navPanel.Controls.Add(_navSettings);

        _pauseButton = new ModernButton();
        _pauseButton.Theme = _theme;
        _pauseButton.Style = ButtonStyle.Subtle;
        _pauseButton.IconGlyph = "\uE769";
        _pauseButton.Text = Loc.T("recording.pause");
        _pauseButton.Radius = 11;
        _pauseButton.Size = new Size(216, 42);
        _pauseButton.Location = new Point(16, 12);
        _pauseButton.Font = new Font("Segoe UI", 9.75F);
        _pauseButton.Click += (_, _) => TogglePause();

        _themeLabel.Text = Loc.T("theme.dark");
        _themeLabel.Font = _fontBody;
        _themeLabel.AutoSize = true;
        _themeLabel.Location = new Point(18, 77);
        _themeLabel.ForeColor = _theme.TextMuted;
        _themeLabel.BackColor = Color.Transparent;

        _themeToggle.Theme = _theme;
        _themeToggle.Location = new Point(184, 70);
        _themeToggle.Size = new Size(46, 26);
        _themeToggle.CheckedChanged += (_, _) =>
        {
            if (_uiLoading) return;
            SetTheme(_themeToggle.Checked);
        };

        _statusLabel.Font = _fontSmall;
        _statusLabel.AutoSize = true;
        _statusLabel.Location = new Point(18, 114);
        _statusLabel.Text = Loc.T("status.recording");
        _statusLabel.ForeColor = _theme.Success;
        _statusLabel.BackColor = Color.Transparent;

        _versionLabel.Text = Loc.T("version");
        _versionLabel.Font = _fontTiny;
        _versionLabel.AutoSize = true;
        _versionLabel.Location = new Point(18, 148);
        _versionLabel.ForeColor = _theme.TextFaint;
        _versionLabel.BackColor = Color.Transparent;

        _hotkeyHint.Text = Loc.T("hotkey.hint");
        _hotkeyHint.Font = _fontTiny;
        _hotkeyHint.AutoSize = true;
        _hotkeyHint.Location = new Point(18, 168);
        _hotkeyHint.ForeColor = _theme.TextFaint;
        _hotkeyHint.BackColor = Color.Transparent;

        _sidebarBottom.Controls.Add(_pauseButton);
        _sidebarBottom.Controls.Add(_themeLabel);
        _sidebarBottom.Controls.Add(_themeToggle);
        _sidebarBottom.Controls.Add(_statusLabel);
        _sidebarBottom.Controls.Add(_versionLabel);
        _sidebarBottom.Controls.Add(_hotkeyHint);

        _sidebar.Controls.Add(_logoArea);
        _sidebar.Controls.Add(_navPanel);
        _sidebar.Controls.Add(_sidebarBottom);
    }

    private ModernButton CreateNavButton(string text, string glyph, Action onClick)
    {
        var button = new ModernButton
        {
            Text = text,
            IconGlyph = glyph,
            Style = ButtonStyle.Nav,
            Theme = _theme,
            Size = new Size(216, 46),
            Location = new Point(16, 0),
            Radius = 10,
            Font = new Font("Segoe UI", 10F)
        };
        button.Click += (_, _) => onClick();
        return button;
    }

    private void BuildContent()
    {
        _header.BackColor = _theme.Background;
        _headerTitle.Text = Loc.T("header.all");
        _headerTitle.Font = _fontHeader;
        _headerTitle.AutoSize = true;
        _headerTitle.Location = new Point(28, 16);
        _headerTitle.ForeColor = _theme.Text;
        _headerTitle.BackColor = Color.Transparent;

        _headerSubtitle.Text = Loc.AllSubtitle(_store.Items.Count);
        _headerSubtitle.Font = _fontBody;
        _headerSubtitle.AutoSize = true;
        _headerSubtitle.Location = new Point(30, 54);
        _headerSubtitle.ForeColor = _theme.TextMuted;
        _headerSubtitle.BackColor = Color.Transparent;

        _search.Placeholder = Loc.T("search.placeholder");
        _search.QueryChanged += (_, _) => ScheduleRefresh();
        _header.Controls.Add(_headerTitle);
        _header.Controls.Add(_headerSubtitle);
        _header.Controls.Add(_search);

        _filterBar.BackColor = _theme.Background;
        _chips.BackColor = _theme.Background;
        _chips.FlowDirection = FlowDirection.LeftToRight;
        _chips.WrapContents = false;
        _chips.AutoSize = false;

        _chipAll = CreateChip(Loc.T("chip.all"), 64, 0);
        _chipText = CreateChip(Loc.T("chip.text"), 76, 1);
        _chipLink = CreateChip(Loc.T("chip.link"), 86, 2);
        _chipImage = CreateChip(Loc.T("chip.image"), 118, 3);

        _chips.Controls.Add(_chipAll);
        _chips.Controls.Add(_chipText);
        _chips.Controls.Add(_chipLink);
        _chips.Controls.Add(_chipImage);

        _countLabel.Font = _fontSmall;
        _countLabel.TextAlign = ContentAlignment.MiddleRight;
        _countLabel.ForeColor = _theme.TextMuted;
        _countLabel.BackColor = Color.Transparent;

        _filterBar.Controls.Add(_chips);
        _filterBar.Controls.Add(_countLabel);

        _body.BackColor = _theme.Background;
        _body.Padding = new Padding(20, 0, 12, 14);

        _scroll.Theme = _theme;
        _scroll.Dock = DockStyle.Fill;

        _empty.Theme = _theme;
        _empty.Dock = DockStyle.Fill;

        _body.Controls.Add(_empty);
        _body.Controls.Add(_scroll);

        _settingsPanel.BackColor = _theme.Background;
        _settingsPanel.Padding = new Padding(20, 0, 12, 14);
        _settingsScroll.Theme = _theme;
        _settingsScroll.Dock = DockStyle.Fill;
        _settingsPanel.Controls.Add(_settingsScroll);
        BuildSettings();

        _toast.Radius = 12;
        _toast.FillColor = _theme.Card;
        _toast.BorderColor = _theme.Border;
        _toast.BackColor = _theme.Background;
        _toast.Size = new Size(320, 40);
        _toast.Visible = false;
        _toastLabel.Dock = DockStyle.Fill;
        _toastLabel.TextAlign = ContentAlignment.MiddleCenter;
        _toastLabel.Font = _fontBody;
        _toastLabel.ForeColor = _theme.Text;
        _toastLabel.BackColor = Color.Transparent;
        _toast.Controls.Add(_toastLabel);

        _content.Controls.Add(_body);
        _content.Controls.Add(_settingsPanel);
        _content.Controls.Add(_header);
        _content.Controls.Add(_filterBar);
        _content.Controls.Add(_toast);
        _toast.BringToFront();
    }

    private ModernButton CreateChip(string text, int width, int index)
    {
        var chip = new ModernButton
        {
            Text = text,
            Style = ButtonStyle.Chip,
            Theme = _theme,
            Size = new Size(width, 34),
            Margin = new Padding(0, 4, 8, 0),
            Radius = 17,
            Font = new Font("Segoe UI", 9.25F)
        };
        chip.Click += (_, _) =>
        {
            _typeFilterIndex = index;
            UpdateChipSelection();
            _scroll.ScrollToTop();
            RefreshList();
        };
        return chip;
    }

    private void UpdateChipSelection()
    {
        _chipAll.Selected = _typeFilterIndex == 0;
        _chipText.Selected = _typeFilterIndex == 1;
        _chipLink.Selected = _typeFilterIndex == 2;
        _chipImage.Selected = _typeFilterIndex == 3;
        _chipAll.Invalidate();
        _chipText.Invalidate();
        _chipLink.Invalidate();
        _chipImage.Invalidate();
    }

    // ===================== Settings =====================

    private void BuildSettings()
    {
        var content = _settingsScroll.Content;
        content.Controls.Clear();

        var cardRecord = CreateCard(Loc.T("settings.recording"), 170);
        AddRowText(cardRecord, 52, Loc.T("settings.autoSave"), Loc.T("settings.autoSaveSub"));
        _tgAutoSave = MakeToggle(cardRecord, 52, 8);
        _tgAutoSave.CheckedChanged += (_, _) =>
        {
            if (_uiLoading) return;
            _store.Settings.AutoSaveEnabled = _tgAutoSave.Checked;
            _store.SaveSettings();
            UpdateRecordingUi();
        };

        AddRowText(cardRecord, 112, Loc.T("settings.pause"), Loc.T("settings.pauseSub"));
        _tgPause = MakeToggle(cardRecord, 112, 8);
        _tgPause.CheckedChanged += (_, _) =>
        {
            if (_uiLoading) return;
            SetPause(_tgPause.Checked);
        };

        var cardStorage = CreateCard(Loc.T("settings.storage"), 226);
        AddRowText(cardStorage, 52, Loc.T("settings.max"), Loc.T("settings.maxSub"));
        _numMax = new NumberStepper
        {
            Theme = _theme,
            Minimum = 10,
            Maximum = 5000,
            Step = 10,
            Value = 200
        };
        PlaceRight(cardStorage, _numMax, 56);
        _numMax.ValueChanged += (_, _) =>
        {
            if (_uiLoading) return;
            _store.Settings.MaxItems = _numMax.Value;
            SaveAndCleanup();
        };

        AddRowText(cardStorage, 112, Loc.T("settings.retention"), Loc.T("settings.retentionSub"));
        _numRetention = new NumberStepper
        {
            Theme = _theme,
            Minimum = 0,
            Maximum = 3650,
            Step = 1,
            Value = 30
        };
        PlaceRight(cardStorage, _numRetention, 116);
        _numRetention.ValueChanged += (_, _) =>
        {
            if (_uiLoading) return;
            _store.Settings.RetentionDays = _numRetention.Value;
            SaveAndCleanup();
        };

        AddRowText(cardStorage, 172, Loc.T("settings.startup"), Loc.T("settings.startupSub"));
        _tgStartup = MakeToggle(cardStorage, 172, 8);
        _tgStartup.CheckedChanged += (_, _) =>
        {
            if (_uiLoading) return;
            _store.Settings.RunAtStartup = _tgStartup.Checked;
            AutostartService.SetEnabled(_tgStartup.Checked);
            _store.SaveSettings();
        };

        var cardAppearance = CreateCard(Loc.T("settings.appearance"), 142);
        var appearanceSub = new Label
        {
            Text = Loc.T("settings.themeChoose"),
            Font = _fontSmall,
            AutoSize = true,
            Location = new Point(20, 52),
            ForeColor = _theme.TextMuted,
            BackColor = Color.Transparent,
            Tag = "muted"
        };
        cardAppearance.Controls.Add(appearanceSub);

        _btnLight = new ModernButton
        {
            Text = Loc.T("theme.light"),
            IconGlyph = "\uE706",
            Style = ButtonStyle.Subtle,
            Theme = _theme,
            Size = new Size(150, 38),
            Location = new Point(20, 82),
            Radius = 10
        };
        _btnLight.Click += (_, _) => SetTheme(false);

        _btnDark = new ModernButton
        {
            Text = Loc.T("theme.darkChoice"),
            IconGlyph = "\uE708",
            Style = ButtonStyle.Subtle,
            Theme = _theme,
            Size = new Size(150, 38),
            Location = new Point(182, 82),
            Radius = 10
        };
        _btnDark.Click += (_, _) => SetTheme(true);

        cardAppearance.Controls.Add(_btnLight);
        cardAppearance.Controls.Add(_btnDark);

        var cardLanguage = CreateCard(Loc.T("settings.language"), 142);
        var languageSub = new Label
        {
            Text = Loc.T("settings.languageSub"),
            Font = _fontSmall,
            AutoSize = true,
            Location = new Point(20, 52),
            ForeColor = _theme.TextMuted,
            BackColor = Color.Transparent,
            Tag = "muted"
        };
        cardLanguage.Controls.Add(languageSub);

        _btnLangAuto = MakeLanguageButton(Loc.T("language.auto"), 20, () => ChangeLanguage("Auto"));
        _btnLangRu = MakeLanguageButton(Loc.T("language.russian"), 160, () => ChangeLanguage("Russian"));
        _btnLangEn = MakeLanguageButton(Loc.T("language.english"), 300, () => ChangeLanguage("English"));
        cardLanguage.Controls.Add(_btnLangAuto);
        cardLanguage.Controls.Add(_btnLangRu);
        cardLanguage.Controls.Add(_btnLangEn);

        var cardData = CreateCard(Loc.T("settings.data"), 172);
        var dataSub = new Label
        {
            Text = Loc.T("settings.dataSub"),
            Font = _fontSmall,
            AutoSize = true,
            Location = new Point(20, 52),
            ForeColor = _theme.TextMuted,
            BackColor = Color.Transparent,
            Tag = "muted"
        };
        _pathLabel = new Label
        {
            Text = _store.DataDirectory,
            Font = _fontTiny,
            AutoSize = false,
            Location = new Point(20, 76),
            Size = new Size(760, 20),
            ForeColor = _theme.TextFaint,
            BackColor = Color.Transparent,
            AutoEllipsis = true,
            Tag = "faint",
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        var btnOpen = new ModernButton
        {
            Text = Loc.T("settings.openFolder"),
            IconGlyph = "\uE8B7",
            Style = ButtonStyle.Subtle,
            Theme = _theme,
            Size = new Size(160, 38),
            Location = new Point(20, 108),
            Radius = 10
        };
        btnOpen.Click += (_, _) => OpenDataFolder();

        var btnClear = new ModernButton
        {
            Text = Loc.T("settings.clear"),
            IconGlyph = "\uE74D",
            Style = ButtonStyle.Danger,
            Theme = _theme,
            Size = new Size(184, 38),
            Location = new Point(190, 108),
            Radius = 10,
            DangerAction = true
        };
        btnClear.Click += (_, _) => ClearHistoryWithConfirm();

        cardData.Controls.Add(dataSub);
        cardData.Controls.Add(_pathLabel);
        cardData.Controls.Add(btnOpen);
        cardData.Controls.Add(btnClear);

        var cardAbout = CreateCard(Loc.T("settings.about"), 134);
        cardAbout.Controls.Add(MakeInfoLabel(Loc.T("about.line1"), 52, "body"));
        cardAbout.Controls.Add(MakeInfoLabel(Loc.T("about.line2"), 76, "muted"));
        cardAbout.Controls.Add(MakeInfoLabel(Loc.T("about.line3"), 100, "muted"));

        content.Controls.Add(cardRecord);
        content.Controls.Add(cardStorage);
        content.Controls.Add(cardAppearance);
        content.Controls.Add(cardLanguage);
        content.Controls.Add(cardData);
        content.Controls.Add(cardAbout);
    }

    private RoundedPanel CreateCard(string title, int height)
    {
        var card = new RoundedPanel
        {
            Width = 820,
            Height = height,
            Radius = 16,
            FillColor = _theme.Card,
            BorderColor = _theme.Border,
            BackColor = _theme.Background,
            Margin = new Padding(0, 0, 0, 12)
        };
        var label = new Label
        {
            Text = title,
            Font = _fontSection,
            AutoSize = true,
            Location = new Point(20, 15),
            ForeColor = _theme.Text,
            BackColor = Color.Transparent,
            Tag = "title"
        };
        card.Controls.Add(label);
        return card;
    }

    private void AddRowText(RoundedPanel card, int y, string title, string subtitle)
    {
        card.Controls.Add(new Label
        {
            Text = title,
            Font = _fontRow,
            AutoSize = true,
            Location = new Point(20, y),
            ForeColor = _theme.Text,
            BackColor = Color.Transparent
        });
        card.Controls.Add(new Label
        {
            Text = subtitle,
            Font = _fontSmall,
            AutoSize = true,
            Location = new Point(20, y + 22),
            ForeColor = _theme.TextMuted,
            BackColor = Color.Transparent,
            Tag = "muted"
        });
    }

    private ToggleSwitch MakeToggle(RoundedPanel card, int y, int offset)
    {
        var toggle = new ToggleSwitch
        {
            Theme = _theme,
            Location = new Point(card.Width - 46 - 22, y + offset)
        };
        toggle.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        card.Controls.Add(toggle);
        return toggle;
    }

    private void PlaceRight(RoundedPanel card, Control control, int y)
    {
        control.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        card.Controls.Add(control);
        control.Location = new Point(card.Width - control.Width - 22, y);
    }

    private ModernButton MakeLanguageButton(string text, int x, Action onClick)
    {
        var button = new ModernButton
        {
            Text = text,
            Style = ButtonStyle.Subtle,
            Theme = _theme,
            Size = new Size(130, 38),
            Location = new Point(x, 84),
            Radius = 10
        };
        button.Click += (_, _) => onClick();
        return button;
    }

    private Label MakeInfoLabel(string text, int y, string tag)
    {
        return new Label
        {
            Text = text,
            Font = tag == "body" ? _fontBody : _fontSmall,
            AutoSize = true,
            Location = new Point(20, y),
            ForeColor = tag == "body" ? _theme.Text : _theme.TextMuted,
            BackColor = Color.Transparent,
            Tag = tag == "body" ? "" : "muted"
        };
    }

    // ===================== Tray =====================

    private void BuildTray()
    {
        _trayPauseItem = new ToolStripMenuItem(Loc.T("tray.pause"), null, (_, _) => TogglePause());

        _trayMenu = new ContextMenuStrip
        {
            ShowImageMargin = false,
            Font = new Font("Segoe UI", 9.75F)
        };
        _trayMenu.Items.Add(Loc.T("tray.open"), null, (_, _) => ShowMainWindow());
        _trayMenu.Items.Add(_trayPauseItem);
        _trayMenu.Items.Add(Loc.T("tray.settings"), null, (_, _) => { ShowMainWindow(); SwitchView(ViewMode.Settings); });
        _trayMenu.Items.Add(new ToolStripSeparator());
        _trayMenu.Items.Add(Loc.T("tray.exit"), null, (_, _) => ExitApp());

        _trayIcon = new NotifyIcon
        {
            Text = "ClipSaver",
            Icon = Icon ?? SystemIcons.Application,
            Visible = true,
            ContextMenuStrip = _trayMenu
        };
        _trayIcon.DoubleClick += (_, _) => ShowMainWindow();

        StyleMenu(_trayMenu);
    }

    private void StyleMenu(ContextMenuStrip menu)
    {
        menu.BackColor = _theme.Card;
        menu.ForeColor = _theme.Text;
        menu.Renderer = new ToolStripProfessionalRenderer(new AppColorTable(_theme)) { RoundedEdges = true };
        foreach (ToolStripItem item in menu.Items)
        {
            item.BackColor = _theme.Card;
            item.ForeColor = _theme.Text;
        }
    }

    // ===================== Lifecycle =====================

    private void OnLoaded()
    {
        try { NativeMethods.AddClipboardFormatListener(Handle); } catch { }
        try
        {
            _hotkeyRegistered = NativeMethods.RegisterHotKey(Handle, NativeMethods.HOTKEY_ID_SHOW,
                NativeMethods.MOD_CONTROL | NativeMethods.MOD_SHIFT, (uint)Keys.V);
        }
        catch
        {
            _hotkeyRegistered = false;
        }

        // Watchdog fallback: some apps reserve Ctrl+Shift+V globally, and
        // RegisterHotKey can silently fail. Polling guarantees the window opens.
        _hotkeyGuardTimer.Tick += (_, _) => CheckHotkeyGuard();
        _hotkeyGuardTimer.Start();

        if (!_hotkeyRegistered)
        {
            try
            {
                _trayIcon.ShowBalloonTip(3500, "ClipSaver", Loc.T("hotkey.conflict"), ToolTipIcon.Warning);
            }
            catch { }
        }

        LoadSettingsUi();
        ApplyTheme();
        UpdateRecordingUi();
        SwitchView(ViewMode.All);
        LayoutUi();

        var args = Environment.GetCommandLineArgs();
        bool startMinimized = args.Any(a => a.Equals("--minimized", StringComparison.OrdinalIgnoreCase));
        bool openSettings = args.Any(a => a.Equals("--settings", StringComparison.OrdinalIgnoreCase));
        if (openSettings) SwitchView(ViewMode.Settings);
        if (startMinimized)
        {
            BeginInvoke(new Action(() => HideToTray(showHint: false)));
        }
    }

    private void OnFormClosing(object? sender, FormClosingEventArgs e)
    {
        if (!_isExitRequested &&
            e.CloseReason != CloseReason.WindowsShutDown &&
            e.CloseReason != CloseReason.TaskManagerClosing)
        {
            e.Cancel = true;
            HideToTray();
            return;
        }

        SaveAll();
        try { NativeMethods.RemoveClipboardFormatListener(Handle); } catch { }
        if (_hotkeyRegistered)
        {
            try { NativeMethods.UnregisterHotKey(Handle, NativeMethods.HOTKEY_ID_SHOW); } catch { }
        }
        Application.RemoveMessageFilter(this);
        _trayIcon.Visible = false;
        _trayIcon.Dispose();
        _trayMenu.Dispose();
        _showEventTimer.Stop();
        _refreshTimer.Stop();
        _toastTimer.Stop();
        _clipboardSuppressTimer.Stop();
        _hotkeyGuardTimer.Stop();
        _toolTip.Dispose();
    }

    private void SaveAll()
    {
        _store.SaveSettings();
        _store.SaveHistory();
    }

    private void ExitApp()
    {
        _isExitRequested = true;
        SaveAll();
        Close();
    }

    // ===================== Layout =====================

    private void LayoutUi()
    {
        if (!IsHandleCreated) return;

        int w = ClientSize.Width;
        int h = ClientSize.Height;

        const int sidebarWidth = 248;
        _sidebar.SetBounds(0, 0, sidebarWidth, h);
        _content.SetBounds(sidebarWidth, 0, Math.Max(0, w - sidebarWidth), h);

        int cw = _content.Width;

        _logoArea.SetBounds(0, 0, sidebarWidth, 104);
        _navPanel.SetBounds(0, 104, sidebarWidth, 176);
        _sidebarBottom.SetBounds(0, Math.Max(0, h - 190), sidebarWidth, 190);

        _header.SetBounds(0, 0, cw, 96);

        int searchWidth = Math.Clamp(cw - 56 - 300, 230, 360);
        _search.SetBounds(cw - 28 - searchWidth, 27, searchWidth, 42);

        _filterBar.SetBounds(0, 96, cw, 52);
        _chips.SetBounds(24, 8, Math.Max(100, cw - 250), 42);
        _countLabel.SetBounds(cw - 228, 12, 200, 28);

        _body.SetBounds(0, 148, cw, Math.Max(0, h - 148));
        _settingsPanel.SetBounds(0, 96, cw, Math.Max(0, h - 96));

        if (_view != ViewMode.Settings) _scroll.RefreshLayout();
        else _settingsScroll.RefreshLayout();

        if (_toast.Visible)
        {
            _toast.Left = Math.Max(12, (cw - _toast.Width) / 2);
            _toast.Top = Math.Max(12, h - _toast.Height - 26);
        }
    }

    // ===================== Views =====================

    private void SwitchView(ViewMode mode)
    {
        _view = mode;

        bool settings = mode == ViewMode.Settings;
        _settingsPanel.Visible = settings;
        _body.Visible = !settings;
        _filterBar.Visible = !settings;
        _search.Visible = !settings;
        _navAll.Selected = mode == ViewMode.All;
        _navFav.Selected = mode == ViewMode.Favorites;
        _navSettings.Selected = mode == ViewMode.Settings;
        _navAll.Invalidate();
        _navFav.Invalidate();
        _navSettings.Invalidate();

        if (settings)
        {
            _headerTitle.Text = Loc.T("header.settings");
            _headerSubtitle.Text = Loc.T("header.settings.sub");
            LoadSettingsUi();
            UpdateHeader();
        }
        else
        {
            _scroll.ScrollToTop();
            RefreshList();
        }

        LayoutUi();
    }

    private void RefreshList()
    {
        if (_view == ViewMode.Settings) return;

        var old = _scroll.Content.Controls.Cast<Control>().ToList();
        _scroll.Content.SuspendLayout();
        try
        {
            _scroll.Content.Controls.Clear();
            foreach (var c in old) c.Dispose();

            IEnumerable<ClipboardItem> source = _store.Items;
            if (_view == ViewMode.Favorites)
                source = source.Where(i => i.IsFavorite);

            var query = _search.Query.Trim();
            if (query.Length > 0)
                source = source.Where(i => MatchesSearch(i, query));

            source = _typeFilterIndex switch
            {
                1 => source.Where(i => i.Type == ClipboardItemType.Text),
                2 => source.Where(i => i.Type == ClipboardItemType.Link),
                3 => source.Where(i => i.Type == ClipboardItemType.Image),
                _ => source
            };

            var list = source.OrderByDescending(i => i.Timestamp).Take(400).ToList();
            foreach (var item in list)
            {
                _scroll.Content.Controls.Add(CreateItemCard(item));
            }

            bool isEmpty = list.Count == 0;
            _empty.Visible = isEmpty;
            _scroll.Visible = !isEmpty;

            if (isEmpty)
            {
                if (query.Length > 0)
                {
                    _empty.Glyph = "\uE721";
                    _empty.Title = Loc.T("empty.search.title");
                    _empty.Subtitle = Loc.T("empty.search.sub");
                }
                else if (_view == ViewMode.Favorites)
                {
                    _empty.Glyph = "\uE734";
                    _empty.Title = Loc.T("empty.favorites.title");
                    _empty.Subtitle = Loc.T("empty.favorites.sub");
                }
                else
                {
                    _empty.Glyph = "\uE81C";
                    _empty.Title = Loc.T("empty.history.title");
                    _empty.Subtitle = Loc.T("empty.history.sub");
                }
                _empty.Invalidate();
            }

            _countLabel.Text = Loc.ItemsCount(list.Count);
        }
        finally
        {
            _scroll.Content.ResumeLayout(false);
        }

        if (_scroll.Visible)
        {
            _scroll.PerformLayout();
            _scroll.RefreshLayout();
            _scroll.Content.PerformLayout();
        }

        UpdateHeader();
        UpdateChipSelection();
        UpdateRecordingUi();
    }

    private static bool MatchesSearch(ClipboardItem item, string query)
    {
        if (item.Text != null && item.Text.Contains(query, StringComparison.OrdinalIgnoreCase)) return true;
        if (item.Type == ClipboardItemType.Image)
        {
            return Loc.T("type.image").Contains(query, StringComparison.OrdinalIgnoreCase)
                || "image".Contains(query, StringComparison.OrdinalIgnoreCase)
                || "png".Contains(query, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }

    private void UpdateHeader()
    {
        if (_view == ViewMode.Settings)
        {
            _headerTitle.Text = Loc.T("header.settings");
            _headerSubtitle.Text = Loc.T("header.settings.sub");
            return;
        }

        if (_view == ViewMode.All)
        {
            int count = _store.Items.Count;
            _headerTitle.Text = Loc.T("header.all");
            _headerSubtitle.Text = Loc.AllSubtitle(count);
        }
        else
        {
            int count = _store.FavoritesCount;
            _headerTitle.Text = Loc.T("header.favorites");
            _headerSubtitle.Text = Loc.FavoritesSubtitle(count);
        }
    }

    private void ScheduleRefresh()
    {
        if (_view == ViewMode.Settings) return;
        _refreshTimer.Stop();
        _refreshTimer.Start();
    }

    // ===================== Item cards =====================

    private Control CreateItemCard(ClipboardItem item)
    {
        var theme = _theme;
        var card = new RoundedPanel
        {
            Height = 88,
            Width = 600,
            Radius = 14,
            FillColor = theme.Card,
            BorderColor = theme.Border,
            HoverFillColor = theme.CardHover,
            HoverBorderColor = Draw.Alpha(theme.Accent, 110),
            HoverEnabled = true,
            BackColor = theme.Background,
            Margin = new Padding(0, 0, 0, 10),
            Cursor = Cursors.Hand
        };

        Control iconControl;
        if (item.Type == ClipboardItemType.Image && !string.IsNullOrWhiteSpace(item.ImagePath))
        {
            var pb = new PictureBox
            {
                Size = new Size(52, 52),
                Location = new Point(16, 18),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = theme.CardHover,
                Cursor = Cursors.Hand
            };
            try
            {
                using var stream = new FileStream(item.ImagePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var source = Image.FromStream(stream);
                var thumb = new Bitmap(52, 52);
                using (var g = Graphics.FromImage(thumb))
                {
                    g.Clear(theme.CardHover);
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    double scale = Math.Min(52.0 / source.Width, 52.0 / source.Height);
                    int dw = Math.Max(1, (int)(source.Width * scale));
                    int dh = Math.Max(1, (int)(source.Height * scale));
                    g.DrawImage(source, (52 - dw) / 2, (52 - dh) / 2, dw, dh);
                }
                pb.Image = thumb;
                using var path = Draw.Rounded(new Rectangle(0, 0, 52, 52), 14);
                pb.Region = new Region(path);
                card.Disposed += (_, _) => { pb.Image?.Dispose(); pb.Image = null; };
            }
            catch
            {
                pb.BackColor = theme.CardHover;
            }
            iconControl = pb;
        }
        else
        {
            var tile = new RoundedPanel
            {
                Size = new Size(52, 52),
                Location = new Point(16, 18),
                Radius = 14,
                FillColor = theme.AccentSoft,
                BorderColor = Color.Transparent,
                BackColor = theme.Card
            };
            var glyph = new Label
            {
                Dock = DockStyle.Fill,
                Text = Draw.TypeGlyph(item.Type),
                Font = Draw.GlyphLarge,
                ForeColor = theme.Accent,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            tile.Controls.Add(glyph);
            iconControl = tile;
        }

        string previewText = item.Type == ClipboardItemType.Image ? Loc.T("empty.image") : item.DisplayPreview();
        var preview = new Label
        {
            Text = previewText,
            Font = _fontPreview,
            AutoSize = false,
            Location = new Point(82, 18),
            Size = new Size(300, 24),
            ForeColor = item.Type == ClipboardItemType.Link ? theme.Accent : theme.Text,
            BackColor = Color.Transparent,
            AutoEllipsis = true,
            Cursor = Cursors.Hand
        };

        var meta = new Label
        {
            Text = $"{Draw.TypeName(item.Type)} · {Draw.FormatRelativeTime(item.Timestamp)}",
            Font = _fontSmall,
            AutoSize = true,
            Location = new Point(82, 47),
            ForeColor = theme.TextMuted,
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand
        };

        var star = new Label
        {
            Text = "\uE735",
            Font = Draw.GlyphSmall,
            AutoSize = true,
            Location = new Point(meta.Right + 8, 47),
            ForeColor = theme.Accent,
            BackColor = Color.Transparent,
            Visible = item.IsFavorite
        };

        var btnCopy = MakeIconButton("\uE8C8", Loc.T("item.copy"), false, () => CopyItemToClipboard(item));
        var btnPin = MakeIconButton(item.IsFavorite ? "\uE735" : "\uE734",
            item.IsFavorite ? Loc.T("item.unpin") : Loc.T("item.pin"), false,
            () =>
            {
                _store.ToggleFavorite(item.Id);
                RefreshList();
            });
        var btnDelete = MakeIconButton("\uE74D", Loc.T("item.delete"), true,
            () =>
            {
                _store.DeleteItem(item.Id);
                RefreshList();
                ShowToast(Loc.T("toast.deleted"));
            });

        var menu = new ContextMenuStrip { ShowImageMargin = false, Font = new Font("Segoe UI", 9.5F) };
        menu.Items.Add(Loc.T("item.copy"), null, (_, _) => CopyItemToClipboard(item));
        menu.Items.Add(item.IsFavorite ? Loc.T("item.unpin") : Loc.T("item.pin"), null, (_, _) =>
        {
            _store.ToggleFavorite(item.Id);
            RefreshList();
        });
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(Loc.T("item.delete"), null, (_, _) =>
        {
            _store.DeleteItem(item.Id);
            RefreshList();
            ShowToast(Loc.T("toast.deleted"));
        });
        StyleMenu(menu);
        card.ContextMenuStrip = menu;
        card.Disposed += (_, _) => menu.Dispose();

        void LayoutCard()
        {
            int right = card.Width - 16;
            int by = (card.Height - 34) / 2;
            btnDelete.Location = new Point(right - 34, by);
            btnPin.Location = new Point(right - 34 * 2 - 6, by);
            btnCopy.Location = new Point(right - 34 * 3 - 12, by);

            int textWidth = Math.Max(80, btnCopy.Left - 82 - 14);
            preview.Width = textWidth;
            star.Left = Math.Min(btnCopy.Left - 14, meta.Right + 8);
        }

        card.Resize += (_, _) => LayoutCard();

        void CopyFromDoubleClick(object? s, EventArgs e) => CopyItemToClipboard(item);
        card.DoubleClick += CopyFromDoubleClick;
        preview.DoubleClick += CopyFromDoubleClick;
        meta.DoubleClick += CopyFromDoubleClick;
        iconControl.DoubleClick += CopyFromDoubleClick;
        star.DoubleClick += CopyFromDoubleClick;

        card.Controls.Add(iconControl);
        card.Controls.Add(preview);
        card.Controls.Add(meta);
        card.Controls.Add(star);
        card.Controls.Add(btnCopy);
        card.Controls.Add(btnPin);
        card.Controls.Add(btnDelete);

        LayoutCard();
        return card;
    }

    private ModernButton MakeIconButton(string glyph, string tip, bool danger, Action onClick)
    {
        var button = new ModernButton
        {
            IconOnly = true,
            IconGlyph = glyph,
            Style = ButtonStyle.Ghost,
            Theme = _theme,
            Size = new Size(34, 34),
            Radius = 10,
            DangerAction = danger
        };
        button.Click += (_, _) => onClick();
        _toolTip.SetToolTip(button, tip);
        return button;
    }

    // ===================== Settings state =====================

    private void LoadSettingsUi()
    {
        if (_tgAutoSave is null) return;
        _uiLoading = true;
        try
        {
            _tgAutoSave.Checked = _store.Settings.AutoSaveEnabled;
            _tgPause.Checked = _store.Settings.RecordingPaused;
            _tgStartup.Checked = AutostartService.IsEnabled();
            _numMax.SetValueSilent(_store.Settings.MaxItems);
            _numRetention.SetValueSilent(_store.Settings.RetentionDays);
            _themeToggle.Checked = _theme.IsDark;
            _btnLight.Selected = !_theme.IsDark;
            _btnDark.Selected = _theme.IsDark;
            _btnLight.Invalidate();
            _btnDark.Invalidate();
            _btnLangAuto.Selected = _store.Settings.Language == "Auto";
            _btnLangRu.Selected = _store.Settings.Language == "Russian";
            _btnLangEn.Selected = _store.Settings.Language == "English";
            _btnLangAuto.Invalidate();
            _btnLangRu.Invalidate();
            _btnLangEn.Invalidate();
            _pathLabel.Text = _store.DataDirectory;
        }
        finally
        {
            _uiLoading = false;
        }
    }

    private void SaveAndCleanup()
    {
        _store.SaveSettings();
        _store.ApplyRetentionAndLimit();
        _store.SaveHistory();
        if (_view != ViewMode.Settings) RefreshList();
        else UpdateHeader();
    }

    private void SetPause(bool paused)
    {
        _store.Settings.RecordingPaused = paused;
        _store.SaveSettings();
        _uiLoading = true;
        _tgPause.Checked = paused;
        _uiLoading = false;
        UpdateRecordingUi();
        ShowToast(paused ? Loc.T("toast.paused") : Loc.T("toast.resumed"));
    }

    private void TogglePause() => SetPause(!_store.Settings.RecordingPaused);

    private void UpdateRecordingUi()
    {
        bool paused = _store.Settings.RecordingPaused;
        _pauseButton.IconGlyph = paused ? "\uE768" : "\uE769";
        _pauseButton.Text = paused ? Loc.T("recording.resume") : Loc.T("recording.pause");
        _pauseButton.Invalidate();

        _statusLabel.Text = paused ? Loc.T("status.paused") : Loc.T("status.recording");
        _statusLabel.ForeColor = paused ? _theme.Danger : _theme.Success;

        if (_trayPauseItem is not null)
        {
            _trayPauseItem.Text = paused ? Loc.T("tray.resume") : Loc.T("tray.pause");
            _trayPauseItem.Checked = paused;
        }
        if (_trayIcon is not null)
            _trayIcon.Text = paused ? Loc.T("tray.paused") : Loc.T("tray.active");
    }

    private void SetTheme(bool dark)
    {
        _store.Settings.Theme = dark ? "Dark" : "Light";
        _theme = dark ? AppTheme.Dark() : AppTheme.Light();
        _store.SaveSettings();

        _uiLoading = true;
        _themeToggle.Checked = dark;
        _uiLoading = false;

        ApplyTheme();
        LoadSettingsUi();
        ApplyTrayTheme();

        if (_view != ViewMode.Settings) RefreshList();
        else UpdateHeader();
    }

    private void ChangeLanguage(string language)
    {
        if (_store.Settings.Language == language) return;

        var result = MessageBox.Show(this,
            Loc.T("language.confirm.text"),
            Loc.T("language.confirm.title"),
            MessageBoxButtons.OKCancel,
            MessageBoxIcon.Information);
        if (result != DialogResult.OK) return;

        _store.Settings.Language = language;
        _store.SaveSettings();
        RestartApp();
    }

    private void RestartApp()
    {
        try
        {
            string exe = Application.ExecutablePath;
            var psi = new ProcessStartInfo("cmd.exe",
                $"/c timeout /t 1 >nul & start \"\" \"{exe}\"")
            {
                CreateNoWindow = true,
                UseShellExecute = false
            };
            Process.Start(psi);
        }
        catch
        {
            // If restart fails, the new language is still applied on next manual start.
        }

        _isExitRequested = true;
        Close();
    }

    private void ApplyTrayTheme()
    {
        if (_trayMenu is null) return;
        StyleMenu(_trayMenu);
    }

    private void ApplyTheme()
    {
        var t = _theme;
        BackColor = t.Background;

        _sidebar.BackColor = t.Sidebar;
        _content.BackColor = t.Background;
        _header.BackColor = t.Background;
        _filterBar.BackColor = t.Background;
        _chips.BackColor = t.Background;
        _body.BackColor = t.Background;
        _settingsPanel.BackColor = t.Background;
        _logoArea.BackColor = t.Sidebar;
        _navPanel.BackColor = t.Sidebar;
        _sidebarBottom.BackColor = t.Sidebar;

        _logoBadge.FillColor = t.Accent;
        _logoBadge.BackColor = t.Sidebar;
        _logoBadge.Invalidate();
        _logoGlyph.ForeColor = t.AccentText;
        _appTitle.ForeColor = t.Text;
        _appSubtitle.ForeColor = t.TextMuted;
        _themeLabel.ForeColor = t.TextMuted;
        _versionLabel.ForeColor = t.TextFaint;
        _hotkeyHint.ForeColor = t.TextFaint;

        _headerTitle.ForeColor = t.Text;
        _headerSubtitle.ForeColor = t.TextMuted;
        _countLabel.ForeColor = t.TextMuted;

        _search.ApplyTheme(t);
        _empty.Theme = t;
        _empty.Invalidate();
        _scroll.Theme = t;
        _scroll.Invalidate();
        _settingsScroll.Theme = t;
        _settingsScroll.Invalidate();

        _toast.FillColor = t.Card;
        _toast.BorderColor = t.Border;
        _toast.BackColor = t.Background;
        _toastLabel.ForeColor = t.Text;
        _toast.Invalidate();

        ThemeRecursive(this, t);
        ThemeSettingsTree();
        UpdateRecordingUi();

        try { NativeMethods.ApplyThemeToTitleBar(Handle, t.IsDark); } catch { }
    }

    private void ThemeRecursive(Control root, AppTheme t)
    {
        foreach (Control c in root.Controls)
        {
            switch (c)
            {
                case ModernButton b:
                    b.Theme = t;
                    b.Invalidate();
                    break;
                case ToggleSwitch tg:
                    tg.Theme = t;
                    tg.Invalidate();
                    break;
                case NumberStepper ns:
                    ns.Theme = t;
                    ns.Invalidate();
                    break;
                case SearchBox sb:
                    sb.ApplyTheme(t);
                    break;
            }
            if (c.HasChildren) ThemeRecursive(c, t);
        }
    }

    private void ThemeSettingsTree()
    {
        var t = _theme;
        foreach (Control card in _settingsScroll.Content.Controls)
        {
            card.BackColor = t.Background;
            if (card is RoundedPanel rp)
            {
                rp.FillColor = t.Card;
                rp.BorderColor = t.Border;
                rp.Invalidate();
            }
            ThemeLabels(card, t);
        }
    }

    private void ThemeLabels(Control root, AppTheme t)
    {
        foreach (Control c in root.Controls)
        {
            if (c is Label label)
            {
                string tag = label.Tag as string ?? string.Empty;
                label.ForeColor = tag switch
                {
                    "muted" => t.TextMuted,
                    "faint" => t.TextFaint,
                    _ => t.Text
                };
                label.BackColor = Color.Transparent;
            }
            if (c.HasChildren) ThemeLabels(c, t);
        }
    }

    private void OpenDataFolder()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = _store.DataDirectory,
                UseShellExecute = true
            });
        }
        catch
        {
            ShowToast(Loc.T("toast.folderFailed"));
        }
    }

    private void ClearHistoryWithConfirm()
    {
        var result = MessageBox.Show(this,
            Loc.T("dialog.clear.text"),
            Loc.T("dialog.clear.title"),
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2);
        if (result != DialogResult.Yes) return;

        _store.ClearHistory(includeFavorites: false);
        if (_view != ViewMode.Settings) RefreshList();
        UpdateHeader();
        ShowToast(Loc.T("toast.cleared"));
    }

    // ===================== Toast =====================

    private void ShowToast(string text)
    {
        _toastLabel.Text = text;
        var size = TextRenderer.MeasureText(text, _fontBody);
        _toast.Size = new Size(Math.Max(180, size.Width + 40), 40);
        LayoutUi();
        _toast.Visible = true;
        _toast.BringToFront();
        _toastTimer.Stop();
        _toastTimer.Start();
    }

    // ===================== Clipboard =====================

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == NativeMethods.WM_CLIPBOARDUPDATE)
        {
            HandleClipboardUpdate();
        }
        else if (m.Msg == NativeMethods.WM_HOTKEY && m.WParam.ToInt32() == NativeMethods.HOTKEY_ID_SHOW)
        {
            ShowFromHotkey();
        }
        base.WndProc(ref m);
    }

    private void HandleClipboardUpdate()
    {
        if (_suppressClipboard) return;
        if (!_store.Settings.AutoSaveEnabled || _store.Settings.RecordingPaused) return;

        bool added = false;
        try
        {
            if (Clipboard.ContainsImage())
            {
                var image = Clipboard.GetImage();
                if (image is not null)
                {
                    using (image)
                    {
                        added = _store.AddImage(image) is not null;
                    }
                }
            }
            else if (Clipboard.ContainsText())
            {
                var text = Clipboard.GetText();
                added = _store.AddText(text) is not null;
            }
        }
        catch
        {
            // Буфер обмена может быть занят другим приложением — это нормально.
        }

        if (added && Visible)
        {
            ScheduleRefresh();
        }
    }

    private void CopyItemToClipboard(ClipboardItem item)
    {
        try
        {
            _suppressClipboard = true;
            _clipboardSuppressTimer.Stop();
            _clipboardSuppressTimer.Start();

            if (item.Type == ClipboardItemType.Image && !string.IsNullOrWhiteSpace(item.ImagePath) && File.Exists(item.ImagePath))
            {
                using var stream = new FileStream(item.ImagePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var image = Image.FromStream(stream);
                Clipboard.SetImage(new Bitmap(image));
            }
            else if (!string.IsNullOrEmpty(item.Text))
            {
                Clipboard.SetText(item.Text);
            }

            ShowToast(Loc.T("toast.copied"));
        }
        catch
        {
            ShowToast(Loc.T("toast.copyFailed"));
        }
    }

    public bool PreFilterMessage(ref Message m)
    {
        const int WM_MOUSEWHEEL = 0x020A;
        if (m.Msg != WM_MOUSEWHEEL || !Visible) return false;

        SmoothScrollPanel target = _view == ViewMode.Settings ? _settingsScroll : _scroll;
        if (!target.Visible) return false;

        var client = target.PointToClient(Cursor.Position);
        if (!target.ClientRectangle.Contains(client)) return false;

        short delta = unchecked((short)((long)m.WParam >> 16));
        target.ScrollBy(delta);
        return true;
    }

    // ===================== Window / tray =====================

    private void ShowMainWindow()
    {
        if (!Visible) Show();
        if (WindowState == FormWindowState.Minimized) WindowState = FormWindowState.Normal;
        Activate();
        BringToFront();
    }

    private void HideToTray(bool showHint = true)
    {
        Hide();

        if (showHint && !_trayHintShown)
        {
            _trayHintShown = true;
            try
            {
                _trayIcon.ShowBalloonTip(2500, "ClipSaver",
                    Loc.T("tray.balloon"),
                    ToolTipIcon.Info);
            }
            catch { }
        }
    }

    private void CheckShowEvent()
    {
        try
        {
            if (Program.ShowEvent != null && Program.ShowEvent.WaitOne(0))
                BeginInvoke(new Action(ShowMainWindow));
        }
        catch { }
    }

    private void CheckHotkeyGuard()
    {
        if (_isExitRequested) return;

        bool ctrl = (NativeMethods.GetAsyncKeyState(NativeMethods.VK_CONTROL) & 0x8000) != 0;
        bool shift = (NativeMethods.GetAsyncKeyState(NativeMethods.VK_SHIFT) & 0x8000) != 0;
        bool alt = (NativeMethods.GetAsyncKeyState(NativeMethods.VK_MENU) & 0x8000) != 0;
        bool v = (NativeMethods.GetAsyncKeyState(NativeMethods.VK_V) & 0x8000) != 0;
        bool down = ctrl && shift && v && !alt;

        if (down && !_hotkeyComboDown)
        {
            _hotkeyComboDown = true;
            ShowFromHotkey();
        }
        else if (!down)
        {
            _hotkeyComboDown = false;
        }
    }

    private void ShowFromHotkey()
    {
        if (_isExitRequested) return;

        var now = DateTime.UtcNow;
        if ((now - _lastHotkeyShow).TotalMilliseconds < 350) return;
        _lastHotkeyShow = now;

        try
        {
            BeginInvoke(new Action(ShowMainWindow));
        }
        catch
        {
            ShowMainWindow();
        }
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        try { NativeMethods.ApplyThemeToTitleBar(Handle, _theme.IsDark); } catch { }
        LayoutUi();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Application.RemoveMessageFilter(this);
            _trayIcon?.Dispose();
            _trayMenu?.Dispose();
            _showEventTimer?.Dispose();
            _refreshTimer?.Dispose();
            _toastTimer?.Dispose();
            _clipboardSuppressTimer?.Dispose();
            _hotkeyGuardTimer?.Dispose();
            _toolTip?.Dispose();
        }
        base.Dispose(disposing);
    }
}
