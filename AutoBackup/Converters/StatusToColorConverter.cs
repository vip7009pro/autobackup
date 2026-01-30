using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using AutoBackup.Models;

namespace AutoBackup.Converters;

/// <summary>
/// Converts BackupStatus to a color brush
/// </summary>
public class StatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is BackupStatus status)
        {
            return status switch
            {
                BackupStatus.Success => new SolidColorBrush(Color.FromRgb(34, 197, 94)),      // Green
                BackupStatus.PartialSuccess => new SolidColorBrush(Color.FromRgb(234, 179, 8)), // Yellow
                BackupStatus.Failed => new SolidColorBrush(Color.FromRgb(239, 68, 68)),       // Red
                BackupStatus.Running => new SolidColorBrush(Color.FromRgb(59, 130, 246)),     // Blue
                _ => new SolidColorBrush(Color.FromRgb(156, 163, 175))                         // Gray
            };
        }
        return new SolidColorBrush(Color.FromRgb(156, 163, 175));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts LogLevel to a color brush
/// </summary>
public class LogLevelToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is LogLevel level)
        {
            return level switch
            {
                LogLevel.Success => new SolidColorBrush(Color.FromRgb(34, 197, 94)),   // Green
                LogLevel.Warning => new SolidColorBrush(Color.FromRgb(234, 179, 8)),   // Yellow
                LogLevel.Error => new SolidColorBrush(Color.FromRgb(239, 68, 68)),     // Red
                _ => new SolidColorBrush(Color.FromRgb(229, 231, 235))                  // Light gray
            };
        }
        return new SolidColorBrush(Color.FromRgb(229, 231, 235));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
