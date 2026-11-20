using System.Drawing;

namespace ClipSaver.UI;

public class AppColorTable : ProfessionalColorTable
{
    private readonly AppTheme _theme;

    public AppColorTable(AppTheme theme) => _theme = theme;

    public override Color ToolStripDropDownBackground => _theme.Card;
    public override Color ImageMarginGradientBegin => _theme.Card;
    public override Color ImageMarginGradientMiddle => _theme.Card;
    public override Color ImageMarginGradientEnd => _theme.Card;
    public override Color MenuBorder => _theme.Border;
    public override Color MenuItemBorder => _theme.Accent;
    public override Color MenuItemSelected => _theme.AccentSoft;
    public override Color MenuItemSelectedGradientBegin => _theme.AccentSoft;
    public override Color MenuItemSelectedGradientEnd => _theme.AccentSoft;
    public override Color MenuItemPressedGradientBegin => _theme.CardHover;
    public override Color MenuItemPressedGradientEnd => _theme.CardHover;
    public override Color SeparatorDark => _theme.Border;
    public override Color SeparatorLight => _theme.Border;
    public override Color CheckBackground => _theme.AccentSoft;
    public override Color CheckSelectedBackground => _theme.AccentSoft;
    public override Color CheckPressedBackground => _theme.AccentSoft;
}
