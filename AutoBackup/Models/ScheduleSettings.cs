namespace AutoBackup.Models;

/// <summary>
/// Schedule settings for automatic backup
/// </summary>
public class ScheduleSettings
{
    /// <summary>
    /// Whether automatic backup is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Type of schedule
    /// </summary>
    public ScheduleType Type { get; set; } = ScheduleType.Interval;

    /// <summary>
    /// Interval in minutes (used when Type is Interval)
    /// </summary>
    public int IntervalMinutes { get; set; } = 30;

    /// <summary>
    /// Time of day for daily backup (used when Type is Daily)
    /// </summary>
    public TimeSpan? DailyTime { get; set; } = new TimeSpan(2, 0, 0); // 2:00 AM default

    /// <summary>
    /// Hour of day for hourly backup (minute of hour, used when Type is Hourly)
    /// </summary>
    public int HourlyMinute { get; set; } = 0;
}

/// <summary>
/// Schedule type enumeration
/// </summary>
public enum ScheduleType
{
    /// <summary>
    /// Run backup once per day at a specific time
    /// </summary>
    Daily,

    /// <summary>
    /// Run backup once per hour
    /// </summary>
    Hourly,

    /// <summary>
    /// Run backup at specified minute intervals
    /// </summary>
    Interval
}
