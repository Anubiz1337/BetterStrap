using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace BetterStrap.UI.ViewModels.Installer;

public class InstallViewModel : NotifyPropertyChangedViewModel
{
    private readonly BetterStrap.Installer installer = new BetterStrap.Installer();

    private readonly string _originalInstallLocation;
    private string _soberInstallStatus = string.Empty;

    private System.Threading.CancellationTokenSource? _soberCancellation;

    public EventHandler<bool>? SetCanContinueEvent;

    public BetterStrap.Models.SettingTasks.ShortcutTask PlayerIconTask { get; }
    public BetterStrap.Models.SettingTasks.ShortcutTask StudioIconTask { get; }
    public BetterStrap.Models.SettingTasks.ShortcutTask SettingsIconTask { get; }

    internal void SetImport(BetterStrap.Utility.BootstrapperImportPlan? plan)
    {
        installer.PendingImport = plan;
    }

    public string InstallLocation
    {
        get { return installer.InstallLocation; }
        set
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                SetCanContinueEvent?.Invoke(this, e: true);
                installer.InstallLocationError = "";
                OnPropertyChanged(nameof(ErrorMessage));
            }
            installer.InstallLocation = value;
            OnPropertyChanged(nameof(DataFoundMessageVisibility));
        }
    }

    public Visibility DataFoundMessageVisibility
    {
        get
        {
            if (!installer.ExistingDataPresent)
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }
    }

    public string ErrorMessage => installer.InstallLocationError;

    public string InstallLocationTitle =>
        BetterStrap.Utility.Platform.IsMacOS
            ? BetterStrap.Resources.Strings.Installer_MacOS_DataLocation_Title
            : BetterStrap.Resources.Strings.Installer_Install_Location_Title;

    public string InstallLocationDescription =>
        BetterStrap.Utility.Platform.IsMacOS
            ? BetterStrap.Resources.Strings.Installer_MacOS_DataLocation_Text
            : BetterStrap.Resources.Strings.Installer_Install_Location_Text;

    public string SoberInstallStatus
    {
        get => _soberInstallStatus;
        private set
        {
            _soberInstallStatus = value;
            OnPropertyChanged(nameof(SoberInstallStatus));
        }
    }

    public void SetSoberInstallStatus(string message)
    {
        SoberInstallStatus = message;
    }

    public bool RosemaryAvailable => BetterStrap.Utility.Platform.UsesLinuxHost && BetterStrap.Platform.Linux.LinuxRosemary.IsSupportedHost;

    public Visibility RosemaryVisibility =>
        RosemaryAvailable && !BetterStrap.Utility.Platform.IsFreeBSD ? Visibility.Visible : Visibility.Collapsed;

    private bool _useRosemary = BetterStrap.Utility.Platform.IsFreeBSD;

    public bool UseSober
    {
        get => !_useRosemary;
        set
        {
            if (value)
                UseRosemary = false;
        }
    }

    public bool UseRosemary
    {
        get => _useRosemary;
        set
        {
            bool chosen = BetterStrap.Utility.Platform.IsFreeBSD || value && RosemaryAvailable;
            if (_useRosemary == chosen)
                return;
            _useRosemary = chosen;
            OnPropertyChanged(nameof(UseRosemary));
            OnPropertyChanged(nameof(UseSober));
        }
    }

    public bool CreateDesktopShortcuts
    {
        get { return installer.CreateDesktopShortcuts; }
        set { installer.CreateDesktopShortcuts = value; }
    }

    public bool CreateStartMenuShortcuts
    {
        get { return installer.CreateStartMenuShortcuts; }
        set { installer.CreateStartMenuShortcuts = value; }
    }

    public bool ExtractRobloxIcons
    {
        get { return installer.ExtractRobloxIcons; }
        set { installer.ExtractRobloxIcons = value; }
    }

    public bool CreatePlayerShortcut
    {
        get { return installer.CreatePlayerShortcut; }
        set { installer.CreatePlayerShortcut = value; }
    }

    public bool CreateStudioShortcut
    {
        get { return installer.CreateStudioShortcut; }
        set { installer.CreateStudioShortcut = value; }
    }

    public bool CreateSettingsShortcut
    {
        get { return installer.CreateSettingsShortcut; }
        set { installer.CreateSettingsShortcut = value; }
    }

    public ICommand BrowseInstallLocationCommand => new RelayCommand(BrowseInstallLocation);

    public ICommand ResetInstallLocationCommand => new RelayCommand(ResetInstallLocation);

    public ICommand OpenFolderCommand => new RelayCommand(OpenFolder);

    public InstallViewModel()
    {
        _originalInstallLocation = installer.InstallLocation;
        string suffix = BetterStrap.Utility.Shortcut.Suffix;
        PlayerIconTask = new(
            "RobloxPlayer",
            Paths.Desktop,
            BetterStrap.Resources.Strings.LaunchMenu_LaunchRoblox + suffix,
            "-player",
            true,
            true
        );
        StudioIconTask = new(
            "RobloxStudio",
            Paths.Desktop,
            BetterStrap.Resources.Strings.LaunchMenu_LaunchRobloxStudio + suffix,
            "-studio",
            true,
            true
        );
        SettingsIconTask = new("Settings", Paths.Desktop, BetterStrap.Resources.Strings.Menu_Title + suffix, "-settings", true, true);
        installer.FunctionShortcutTasks = [PlayerIconTask, StudioIconTask, SettingsIconTask];
    }

    public async System.Threading.Tasks.Task<bool> DoInstallAsync(Action<string>? report = null)
    {
        if (!installer.CheckInstallLocation())
        {
            SetCanContinueEvent?.Invoke(this, e: false);
            OnPropertyChanged(nameof(ErrorMessage));
            return false;
        }
        try
        {
            PlayerIconTask.ValidateAppearance(CreatePlayerShortcut);
            StudioIconTask.ValidateAppearance(CreateStudioShortcut);
            SettingsIconTask.ValidateAppearance(CreateSettingsShortcut);
            installer.DoInstall();
        }
        catch (Exception ex)
        {
            App.Logger.WriteException("InstallViewModel::DoInstall", ex);
            Frontend.ShowMessageBox(
                "BetterStrap could not finish installing to this location.\n\n" + ex.Message + "\n\nPick a different folder and try again.",
                MessageBoxImage.Hand
            );
            SetCanContinueEvent?.Invoke(this, e: true);
            return false;
        }
        if (BetterStrap.Utility.Platform.UsesLinuxHost)
        {
            BetterStrap.Utility.LinuxRuntimeChoice.SelectDefaults(UseRosemary);
            App.Logger.WriteLine(
                "InstallViewModel::DoInstall",
                UseRosemary ? "Roblox runs through Rosemary" : "Roblox runs through Sober and Vinegar"
            );
        }
        if (BetterStrap.Utility.Platform.UsesLinuxHost && UseRosemary)
        {
            using System.Threading.CancellationTokenSource cancellation = new();
            _soberCancellation = cancellation;
            Action<string> status = message =>
            {
                System.Windows.Application.Current?.Dispatcher.BeginInvoke(new Action(() => SoberInstallStatus = message));
                report?.Invoke(message);
            };
            try
            {
                BetterStrap.Platform.Linux.LinuxRosemary.DownloadSegments = BetterStrap.Utility.DownloadConfiguration.NormalizeSegments(
                    App.Settings.Prop.MaxDownloadSegments
                );
                System.Threading.Tasks.Task<BetterStrap.Platform.OperationResult<string>> runtimeTask =
                    BetterStrap.Platform.Linux.LinuxRosemary.EnsureRuntimeAsync(status, cancellation.Token);
                System.Threading.Tasks.Task<BetterStrap.Platform.OperationResult<string>> robloxTask =
                    BetterStrap.Platform.Linux.LinuxRosemary.EnsureRobloxAsync(
                        BetterStrap.Platform.Linux.RosemaryApplication.Player,
                        new BetterStrap.Platform.Linux.RosemaryRobloxOptions(),
                        status,
                        cancellation.Token
                    );
                await System.Threading.Tasks.Task.WhenAll(runtimeTask, robloxTask);
                BetterStrap.Platform.OperationResult<string> runtime = runtimeTask.Result;
                if (!robloxTask.Result.Succeeded)
                    App.Logger.WriteLine(
                        "InstallViewModel::DoInstall",
                        robloxTask.Result.Failure?.Message ?? "macOS Roblox could not be downloaded yet, it downloads on the first launch"
                    );
                App.Logger.WriteLine(
                    "InstallViewModel::DoInstall",
                    runtime.Succeeded ? "Rosemary installed" : (runtime.Failure?.Message ?? "Rosemary could not be installed")
                );
                if (!runtime.Succeeded)
                    Frontend.ShowMessageBox(
                        runtime.Failure?.Code == "RosemaryMissing"
                            ? Strings.Menu_Rosemary_NotInstalledFreeBSD
                            : runtime.Failure?.Message ?? "Rosemary could not be installed",
                        MessageBoxImage.Error,
                        MessageBoxButton.OK
                    );
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
            {
                App.Logger.WriteLine("InstallViewModel::DoInstall", "The Rosemary install was cancelled");
                return false;
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("InstallViewModel::DoInstall", ex);
                Frontend.ShowMessageBox("Rosemary could not be installed: " + ex.Message, MessageBoxImage.Error, MessageBoxButton.OK);
            }
            finally
            {
                _soberCancellation = null;
            }
        }
        else if (BetterStrap.Utility.Platform.UsesLinuxHost)
        {
            SoberInstallStatus = "Installing Sober";
            using System.Threading.CancellationTokenSource cancellation = new();
            _soberCancellation = cancellation;
            try
            {
                BetterStrap.Platform.OperationResult result = await new BetterStrap.Platform.Linux.LinuxSoberInstaller(
                    new BetterStrap.Core.SystemProcessService()
                ).InstallAsync(cancellation.Token, report);
                App.Logger.WriteLine(
                    "InstallViewModel::DoInstall",
                    result.Succeeded ? "Sober installed" : (result.Failure?.Message ?? "Sober could not be installed")
                );
                if (!result.Succeeded)
                    Frontend.ShowMessageBox(
                        result.Failure?.Message ?? "Sober could not be installed",
                        MessageBoxImage.Error,
                        MessageBoxButton.OK
                    );
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
            {
                App.Logger.WriteLine("InstallViewModel::DoInstall", "The Sober install was cancelled");
                return false;
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("InstallViewModel::DoInstall", ex);
                Frontend.ShowMessageBox("Sober could not be installed: " + ex.Message, MessageBoxImage.Error, MessageBoxButton.OK);
            }
            finally
            {
                _soberCancellation = null;
            }
        }
        return true;
    }

    public void CancelSoberInstall()
    {
        try
        {
            _soberCancellation?.Cancel();
        }
        catch (ObjectDisposedException) { }
    }

    private void BrowseInstallLocation()
    {
        if (BetterStrap.Utility.Platform.UsesLinuxHost)
        {
            Microsoft.Win32.OpenFolderDialog dialog = new()
            {
                Title = "Choose the BetterStrap installation folder",
                InitialDirectory = InstallLocation,
            };
            if (dialog.ShowDialog() == true)
            {
                InstallLocation = dialog.FolderName;
                OnPropertyChanged(nameof(InstallLocation));
            }
            return;
        }

        using FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
        if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
        {
            InstallLocation = folderBrowserDialog.SelectedPath;
            OnPropertyChanged(nameof(InstallLocation));
        }
    }

    private void ResetInstallLocation()
    {
        InstallLocation = _originalInstallLocation;
        OnPropertyChanged(nameof(InstallLocation));
    }

    private void OpenFolder()
    {
        if (!BetterStrap.Utility.Platform.IsWindows)
        {
            BetterStrap.Utility.PlatformShell.TryOpenFolder(Paths.Base);
            return;
        }

        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = BetterStrap.Utility.PlatformShell.WindowsTool("explorer.exe"),
                UseShellExecute = true,
            };
            startInfo.ArgumentList.Add(Paths.Base);
            Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            App.Logger.WriteLine("InstallViewModel::OpenFolder", "Could not open the install folder: " + ex.Message);
        }
    }
}
