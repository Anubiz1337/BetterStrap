using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using BetterStrap.Integrations.Plugins;
using BetterStrap.Resources;
using BetterStrap.UI.Elements.Base;

namespace BetterStrap.UI.Elements.Dialogs;

public partial class CreateBetterStrapModDialog : WpfUiWindow
{
    private string _suggestedName = "";

    private string? _iconPath;

    public string? CreatedMod { get; private set; }

    public CreateBetterStrapModDialog()
    {
        InitializeComponent();
        foreach (ModFiles.Template template in ModFiles.Templates)
            TemplateList.Items.Add(BuildTemplateItem(template));
        TemplateList.SelectedIndex = 0;
        Loaded += OnDialogLoaded;
        Closed += OnDialogClosed;
    }

    public static string TemplateTitle(string key) =>
        key switch
        {
            "dashboard" => Strings.Ui_BetterStrapMods_TemplateDashboard,
            "sidebar-page" => Strings.Ui_BetterStrapMods_TemplateSidebarPage,
            "theme-customizer" => Strings.Ui_BetterStrapMods_TemplateThemeCustomizer,
            "sidebar-manager" => Strings.Ui_BetterStrapMods_TemplateSidebarManager,
            "fastflags" => Strings.Ui_BetterStrapMods_TemplateFastFlags,
            "game-hooks" => Strings.Ui_BetterStrapMods_TemplateGameHooks,
            _ => Strings.Ui_BetterStrapMods_TemplateBlank,
        };

    public static string TemplateDescription(string key) =>
        key switch
        {
            "dashboard" => Strings.Ui_BetterStrapMods_TemplateDashboardDescription,
            "sidebar-page" => Strings.Ui_BetterStrapMods_TemplateSidebarPageDescription,
            "theme-customizer" => Strings.Ui_BetterStrapMods_TemplateThemeCustomizerDescription,
            "sidebar-manager" => Strings.Ui_BetterStrapMods_TemplateSidebarManagerDescription,
            "fastflags" => Strings.Ui_BetterStrapMods_TemplateFastFlagsDescription,
            "game-hooks" => Strings.Ui_BetterStrapMods_TemplateGameHooksDescription,
            _ => Strings.Ui_BetterStrapMods_TemplateBlankDescription,
        };

    private static ListBoxItem BuildTemplateItem(ModFiles.Template template)
    {
        StackPanel panel = new() { Margin = new Thickness(2, 4, 2, 4) };
        panel.Children.Add(new TextBlock { Text = TemplateTitle(template.Key), FontWeight = FontWeights.SemiBold });
        TextBlock description = new()
        {
            Text = TemplateDescription(template.Key),
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 2, 0, 0),
        };
        description.SetResourceReference(TextBlock.ForegroundProperty, "TextFillColorTertiaryBrush");
        panel.Children.Add(description);
        return new ListBoxItem { Content = panel, Tag = template };
    }

    private ModFiles.Template SelectedTemplate =>
        (TemplateList.SelectedItem as ListBoxItem)?.Tag as ModFiles.Template ?? ModFiles.Templates[0];

    private void OnDialogLoaded(object sender, RoutedEventArgs e)
    {
        NameBox.Focus();
        NameBox.SelectAll();
    }

    private void OnDialogClosed(object? sender, EventArgs e)
    {
        Loaded -= OnDialogLoaded;
        Closed -= OnDialogClosed;
    }

    private void TemplateList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (NameBox == null)
            return;
        if (string.IsNullOrWhiteSpace(NameBox.Text) || NameBox.Text == _suggestedName)
        {
            _suggestedName = ModFiles.NextAvailableName(SelectedTemplate.DefaultName);
            NameBox.Text = _suggestedName;
        }
    }

    private void TemplateList_MouseDoubleClick(object sender, MouseButtonEventArgs e) => Create();

    private void NameBox_TextChanged(object sender, TextChangedEventArgs e) => ErrorText.Visibility = Visibility.Collapsed;

    private void NameBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;
        e.Handled = true;
        Create();
    }

    private async void ChooseIcon_Click(object sender, RoutedEventArgs e)
    {
        Microsoft.Win32.OpenFileDialog picker = new() { Filter = Strings.Ui_BetterStrapMods_IconFilter, CheckFileExists = true };
        if (picker.ShowDialog(this) != true)
            return;
        IconPreview.Source = null;
        IconPlaceholder.Visibility = Visibility.Visible;
        BitmapSource? preview = await BetterStrap.Utility.MediaContentFilter.LoadCheckedIconAsync(picker.FileName);
        if (preview == null)
        {
            ShowError(Strings.Ui_BetterStrapMods_IconFailed);
            return;
        }
        _iconPath = picker.FileName;
        IconPreview.Source = preview;
        IconPlaceholder.Visibility = Visibility.Collapsed;
    }

    public static BitmapSource? LoadIcon(string path)
    {
        try
        {
            BitmapDecoder decoder = BitmapDecoder.Create(new Uri(path), BitmapCreateOptions.IgnoreImageCache, BitmapCacheOption.OnLoad);
            BitmapFrame frame = decoder.Frames.OrderByDescending(f => f.PixelWidth).First();
            frame.Freeze();
            return frame;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            return null;
        }
    }

    private void Create_Click(object sender, RoutedEventArgs e) => Create();

    private void Create()
    {
        string name = ModFiles.NormalizeName(NameBox.Text);
        if (!ModFiles.IsValidName(name))
        {
            ShowError(Strings.Ui_BetterStrapMods_InvalidNameMessage);
            return;
        }
        if (System.IO.Directory.Exists(System.IO.Path.Combine(Paths.Plugins, name)))
        {
            ShowError(string.Format(Strings.Ui_BetterStrapMods_AlreadyExistsMessage, name));
            return;
        }
        try
        {
            ModFiles.Create(name, SelectedTemplate.Key, _iconPath);
            CreatedMod = name;
            DialogResult = true;
        }
        catch (Exception ex)
        {
            ModFiles.TryDelete(System.IO.Path.Combine(Paths.Plugins, name));
            ShowError(ex.Message);
        }
    }

    private void ShowError(string text)
    {
        ErrorText.Text = text;
        ErrorText.Visibility = Visibility.Visible;
    }
}
