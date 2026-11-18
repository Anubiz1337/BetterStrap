using System;
using System.Collections.Generic;
using BetterStrap.Enums;

namespace BetterStrap.Utility;

public static class LinuxRuntimeChoice
{
    public const string Sober = "sober";
    public const string Vinegar = "vinegar";
    public const string Rosemary = "rosemary";

    public static bool UsesRosemary(LaunchMode mode)
    {
        if (!Platform.UsesLinuxHost || App.Settings?.Prop == null)
            return false;
        if (Platform.IsFreeBSD)
            return BetterStrap.Platform.Linux.LinuxRosemary.IsSupportedHost;
        string choice = mode == LaunchMode.Player ? App.Settings.Prop.LinuxPlayerRuntime : App.Settings.Prop.LinuxStudioRuntime;
        return string.Equals(choice, Rosemary, StringComparison.OrdinalIgnoreCase)
            && BetterStrap.Platform.Linux.LinuxRosemary.IsSupportedHost;
    }

    public static bool PlayerUsesRosemary => UsesRosemary(LaunchMode.Player);

    private static bool? _pageUsesRosemary;

    public static bool PageUsesRosemary => _pageUsesRosemary ?? PlayerUsesRosemary;

    public static void Select(LaunchMode mode, string runtime)
    {
        if (App.Settings?.Prop == null)
            return;
        if (mode == LaunchMode.Player)
            App.Settings.Prop.LinuxPlayerRuntime = string.Equals(runtime, Rosemary, StringComparison.OrdinalIgnoreCase) ? Rosemary : Sober;
        else
            App.Settings.Prop.LinuxStudioRuntime = string.Equals(runtime, Rosemary, StringComparison.OrdinalIgnoreCase)
                ? Rosemary
                : Vinegar;
        App.Settings.Save();
        _pageUsesRosemary = UsesRosemary(mode);
        Changed?.Invoke(null, EventArgs.Empty);
        PrefetchRosemary();
    }

    public static event EventHandler? Changed;

    private static int _prefetching;

    public static void PrefetchRosemary()
    {
        if (!Platform.UsesLinuxHost || !BetterStrap.Platform.Linux.LinuxRosemary.IsSupportedHost || App.Settings?.Prop == null)
            return;
        bool player = UsesRosemary(LaunchMode.Player);
        bool studio = UsesRosemary(LaunchMode.Studio);
        if (!player && !studio || System.Threading.Interlocked.Exchange(ref _prefetching, 1) != 0)
            return;
        BetterStrap.Platform.Linux.LinuxRosemary.DownloadSegments = DownloadConfiguration.NormalizeSegments(
            App.Settings.Prop.MaxDownloadSegments
        );
        BetterStrap.Platform.Linux.RosemaryRobloxOptions options = new(
            App.Settings.Prop.IsChannelEnabled ? App.Settings.Prop.Channel : null,
            App.Settings.Prop.UpdateRoblox
        );
        _ = System.Threading.Tasks.Task.Run(async () =>
        {
            System.Diagnostics.Stopwatch clock = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                List<System.Threading.Tasks.Task> work = [BetterStrap.Platform.Linux.LinuxRosemary.EnsureRuntimeAsync(null)];
                if (
                    player
                    && BetterStrap.Platform.Linux.LinuxRosemary.RunningProcessIds(BetterStrap.Platform.Linux.RosemaryApplication.Player).Count
                        == 0
                )
                    work.Add(
                        BetterStrap.Platform.Linux.LinuxRosemary.EnsureRobloxAsync(
                            BetterStrap.Platform.Linux.RosemaryApplication.Player,
                            options,
                            null
                        )
                    );
                if (
                    studio
                    && BetterStrap.Platform.Linux.LinuxRosemary.RunningProcessIds(BetterStrap.Platform.Linux.RosemaryApplication.Studio).Count
                        == 0
                )
                    work.Add(
                        BetterStrap.Platform.Linux.LinuxRosemary.EnsureRobloxAsync(
                            BetterStrap.Platform.Linux.RosemaryApplication.Studio,
                            options,
                            null
                        )
                    );
                await System.Threading.Tasks.Task.WhenAll(work);
                App.Logger?.WriteLine(
                    "LinuxRuntimeChoice::PrefetchRosemary",
                    "Rosemary is ready in the background after " + clock.ElapsedMilliseconds + " ms"
                );
            }
            catch (Exception ex)
            {
                App.Logger?.WriteLine(
                    "LinuxRuntimeChoice::PrefetchRosemary",
                    "The background Rosemary update did not finish: " + ex.Message
                );
            }
            finally
            {
                System.Threading.Interlocked.Exchange(ref _prefetching, 0);
            }
        });
    }

    public static IReadOnlyDictionary<string, string> RosemaryEnvironment()
    {
        Dictionary<string, string> environment = new(StringComparer.Ordinal);
        BetterStrap.Models.Persistable.AppSettings? settings = App.Settings?.Prop;
        if (settings == null)
            return environment;
        if (settings.RosemarySwapInterval is "0" or "1" or "-1")
            environment["ROSEMARY_SWAP_INTERVAL"] = settings.RosemarySwapInterval;
        environment["ROSEMARY_DECORATIONS"] = string.Equals(settings.RosemaryDecorations, "server", StringComparison.Ordinal)
            ? "server"
            : "client";
        environment["ROSEMARY_COMMAND_KEY"] = settings.RosemaryCommandKey is "ctrl" or "alt" ? settings.RosemaryCommandKey : "super";
        if (settings.RosemaryConfinePointer)
            environment["ROSEMARY_CONFINE_POINTER"] = "1";
        environment["ROSEMARY_PLAIN_CTRL_CLICK"] = settings.RosemaryPlainControlClick ? "1" : "0";
        if (settings.RosemaryForceOpenGl)
            environment["VK_DRIVER_FILES"] = "/dev/null";
        if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WAYLAND_DISPLAY")))
            environment["SDL_VIDEO_DRIVER"] = "wayland,x11";
        return environment;
    }

    public static void SelectDefaults(bool rosemary)
    {
        if (App.Settings?.Prop == null)
            return;
        App.Settings.Prop.LinuxPlayerRuntime = rosemary ? Rosemary : Sober;
        App.Settings.Prop.LinuxStudioRuntime = rosemary ? Rosemary : Vinegar;
        App.Settings.Save();
        _pageUsesRosemary = null;
        Changed?.Invoke(null, EventArgs.Empty);
    }
}
