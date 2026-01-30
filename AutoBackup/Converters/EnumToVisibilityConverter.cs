using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AutoBackup.Converters;

/// <summary>
/// Converts an Enum value to Visibility based on a parameter match
/// </summary>
public class EnumToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return Visibility.Collapsed;

        var checkValue = value.ToString();
        var targetValue = parameter.ToString();

        return string.Equals(checkValue, targetValue, StringComparison.InvariantCultureIgnoreCase) 
            ? Visibility.Visible 
            : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
