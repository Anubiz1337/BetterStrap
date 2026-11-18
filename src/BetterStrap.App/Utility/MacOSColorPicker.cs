using System.Threading.Tasks;
using System.Windows.Media;
using RinColor;

namespace BetterStrap.Utility;

internal sealed class MacOSColorPicker : IRinScreenColorPicker
{
    public bool IsAvailable => BetterStrap.Platform.MacOS.MacOSColorSampler.IsAvailable;

    public async Task<Color?> PickAsync()
    {
        (byte R, byte G, byte B)? picked = await BetterStrap.Platform.MacOS.MacOSColorSampler.PickAsync();
        return picked is { } rgb ? Color.FromRgb(rgb.R, rgb.G, rgb.B) : null;
    }
}
