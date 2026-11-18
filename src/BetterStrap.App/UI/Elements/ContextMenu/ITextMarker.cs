using System.Windows.Media;

namespace BetterStrap.UI.Elements.ContextMenu;

public interface ITextMarker
{
    int StartOffset { get; }

    int Length { get; }

    Color? BackgroundColor { get; set; }

    Color? ForegroundColor { get; set; }

    string ToolTip { get; set; }
}
