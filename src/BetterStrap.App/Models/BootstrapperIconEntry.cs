using System.ComponentModel;
using System.Windows.Media;
using BetterStrap.Enums;
using BetterStrap.Extensions;

namespace BetterStrap.Models;

public class BootstrapperIconEntry : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public BootstrapperIcon IconType { get; set; }

    public string? CustomLocation { get; set; }

    public ImageSource ImageSource => IconEx.GetIconSource(IconType, CustomLocation);

    public void RefreshImage() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageSource)));
}
