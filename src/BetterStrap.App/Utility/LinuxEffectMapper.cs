using System;
using System.Globalization;
using System.Text;
using BetterStrap.Integrations.AntiAliasing;
using BetterStrap.Integrations.FrameGeneration;
using BetterStrap.Platform.Linux;

namespace BetterStrap.Utility;

internal static class LinuxEffectMapper
{
    public const int SupportedAntiAliasingMethods = 5;

    static LinuxEffectMapper()
    {
        if (Platform.UsesLinuxHost)
            LinuxSoberRuntimeProvider.EffectLayersDisabled += OnEffectLayersDisabled;
    }

    private static void OnEffectLayersDisabled()
    {
        BetterStrap.Integrations.Overlays.OverlayHub.SetLinuxHomepageNativeShaderActive(false);
    }

    public static LinuxEffectOptions CreateOptions()
    {
        int soberSharpness = Math.Clamp(App.Settings.Prop.SoberSharpness, 0, 100);
        bool soberSharpen = soberSharpness > 0;
        float sharpnessAmount = soberSharpen ? soberSharpness / 100f : 0.4f;

        LinuxHomepageShader.HomepageMedia? homepageMedia = LinuxHomepageShader.ResolveMedia();
        string? homepageShader = LinuxHomepageShader.Build(homepageMedia);

        return new LinuxEffectOptions(
            Enabled: soberSharpen || MapAntiAliasing() is not null || MapFrameGenMultiplier() > 1 || homepageShader is not null,
            AntiAliasing: MapAntiAliasing(),
            AntiAliasingUltra: AntiAliasingSettings.MethodIndex is 2 or 4,
            Sharpening: soberSharpen,
            SharpnessAmount: sharpnessAmount,
            GradingShader: null,
            HomepageShader: homepageShader,
            HomepageMediaPath: homepageMedia?.Path,
            FrameGenMultiplier: MapFrameGenMultiplier());
    }

    public static string? MapAntiAliasing()
    {
        return AntiAliasingSettings.MethodIndex switch
        {
            1 or 2 => "fxaa",
            3 or 4 => "smaa",
            _ => LinuxNvidiaSettings.Read().Fxaa ? "fxaa" : null
        };
    }

    private static int MapFrameGenMultiplier()
    {
        return FrameGenSettings.ModeIndex > 0 ? 2 : 0;
    }

    public static void RefreshConfiguration()
    {
        if (!BetterStrap.Utility.Platform.UsesLinuxHost)
            return;

        try
        {
            BetterStrap.Platform.Linux.LinuxEffectOptions options = CreateOptions();

            if (!options.Enabled)
                return;

            BetterStrap.Platform.Linux.LinuxEffectLayers.WriteConfiguration(options);
        }
        catch (Exception ex)
        {
            App.Logger?.WriteLine("LinuxEffectMapper::RefreshConfiguration", "The effect configuration could not be refreshed: " + ex.Message);
        }
    }
}
