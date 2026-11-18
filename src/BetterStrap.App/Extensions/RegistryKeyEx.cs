using System;
using System.Windows;
using Microsoft.Win32;
using BetterStrap.Enums;
using BetterStrap.Resources;
using BetterStrap.UI;

namespace BetterStrap.Extensions;

public static class RegistryKeyEx
{
    public static void SetValueSafe(this RegistryKey registryKey, string? name, object value)
    {
        if (!BetterStrap.Utility.Platform.SupportsRegistry)
        {
            App.Logger.WriteLine("RegistryKeyEx::SetValueSafe", $"Skipped registry write on this platform: {registryKey}\\{name}");
            return;
        }
        try
        {
            App.Logger.WriteLine("RegistryKeyEx::SetValueSafe", $"Writing '{value}' to {registryKey}\\{name}");
            registryKey.SetValue(name, value);
        }
        catch (UnauthorizedAccessException)
        {
            Frontend.ShowMessageBox(Strings.Dialog_RegistryWriteError, MessageBoxImage.Hand);
            App.Terminate(ErrorCode.ERROR_INSTALL_FAILURE);
        }
    }

    public static void DeleteValueSafe(this RegistryKey registryKey, string name)
    {
        if (!BetterStrap.Utility.Platform.SupportsRegistry)
        {
            App.Logger.WriteLine("RegistryKeyEx::DeleteValueSafe", $"Skipped registry delete on this platform: {registryKey}\\{name}");
            return;
        }
        try
        {
            App.Logger.WriteLine("RegistryKeyEx::DeleteValueSafe", $"Deleting {registryKey}\\{name}");
            registryKey.DeleteValue(name);
        }
        catch (UnauthorizedAccessException)
        {
            Frontend.ShowMessageBox(Strings.Dialog_RegistryWriteError, MessageBoxImage.Hand);
            App.Terminate(ErrorCode.ERROR_INSTALL_FAILURE);
        }
    }
}
