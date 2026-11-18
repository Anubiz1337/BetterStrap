using System;

namespace BetterStrap.UI.Elements.Bootstrapper;

public static class BackgroundEvents
{
    public static event Action<string?>? BackgroundChanged;

    public static void RaiseBackgroundChanged(string? path)
    {
        BackgroundEvents.BackgroundChanged?.Invoke(path);
    }
}
