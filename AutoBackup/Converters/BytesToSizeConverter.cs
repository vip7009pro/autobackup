using System.Globalization;
using System.Windows.Data;
using AutoBackup.Helpers;

namespace AutoBackup.Converters;

/// <summary>
/// Converts bytes to human-readable size string
/// </summary>
public class BytesToSizeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is long bytes)
        {
            return DiskSpaceHelper.FormatBytes(bytes);
        }
        if (value is int intBytes)
        {
            return DiskSpaceHelper.FormatBytes(intBytes);
        }
        return "0 B";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts nullable value to boolean (has value)
/// </summary>
public class NullToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value != null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
