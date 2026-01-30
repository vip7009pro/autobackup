using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AutoBackup.Converters;

/// <summary>
/// Converts boolean to Visibility
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var invert = parameter?.ToString()?.ToLower() == "invert";
        bool boolValue = false;

        if (value is bool b)
            boolValue = b;
        else if (value is int i)
            boolValue = i > 0;
        else if (value != null)
            boolValue = true;

        if (invert) boolValue = !boolValue;

        return boolValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var invert = parameter?.ToString()?.ToLower() == "invert";
        var isVisible = value is Visibility v && v == Visibility.Visible;

        return invert ? !isVisible : isVisible;
    }
}

/// <summary>
/// Converts boolean to inverse boolean
/// </summary>
public class InverseBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool b ? !b : value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool b ? !b : value;
    }
}
