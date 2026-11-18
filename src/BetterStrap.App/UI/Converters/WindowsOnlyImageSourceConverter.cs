using System;
using System.Globalization;
using System.Windows.Data;

namespace BetterStrap.UI.Converters;

public class WindowsOnlyImageSourceConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return BetterStrap.Utility.Platform.UsesPortableUi ? null : value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
