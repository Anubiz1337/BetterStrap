using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using BetterStrap.Resources;

namespace BetterStrap.UI.ViewModels.Dialogs;

public class UninstallerViewModel
{
    public string Text => string.Format(Strings.Uninstaller_Text, App.ProjectIssuesLink, Paths.Base);

    public bool KeepData { get; set; } = true;

    public bool KeepVersions { get; set; }

    public bool KeepLogs { get; set; }

    public ICommand ConfirmUninstallCommand => new RelayCommand(ConfirmUninstall);

    public event EventHandler? ConfirmUninstallRequest;

    private void ConfirmUninstall()
    {
        this.ConfirmUninstallRequest?.Invoke(this, new EventArgs());
    }
}
