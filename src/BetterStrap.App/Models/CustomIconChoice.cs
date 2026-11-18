using System.Windows.Media;

namespace BetterStrap.Models;

public class CustomIconChoice
{
    public string Path { get; init; } = "";

    public ImageSource? Image { get; init; }

    public bool IsSelected { get; init; }
}
