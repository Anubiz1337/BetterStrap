using System;
using System.Diagnostics;
using System.Windows.Input;
using BetterStrap.UI.ViewModels.ContextMenu;

namespace BetterStrap.UI.Elements.Settings.Pages
{
    public class DonoPageViewModel
    {
        public ICommand OpenUrlCommand { get; }

        public DonoPageViewModel()
        {
            OpenUrlCommand = new RelayCommand(OpenUrl);
        }

        private void OpenUrl(object? parameter)
        {
            if (parameter is string url)
            {
                Utilities.OpenWebLink(url);
            }
        }
    }
}
