using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using BetterStrap.Enums;
using BetterStrap.UI.Elements.Base;

namespace BetterStrap.UI.Elements.Dialogs;

public partial class BloxshadeDialog : WpfUiWindow
{
    public NextAction CloseAction;

    public BloxshadeDialog()
    {
        InitializeComponent();
        if (BetterStrap.Utility.Platform.UsesLinuxHost)
        {
            Grid.SetRow(SettingText, 1);
            Grid.SetRowSpan(SettingText, 1);
            SettingText.Margin = new Thickness(73, 0, 0, 12);
        }
    }

    public void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
