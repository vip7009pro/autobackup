using AutoBackup.Models;
using AutoBackup.Services.Interfaces;

namespace AutoBackup.Services;

/// <summary>
/// Service for scheduling automatic backups
/// </summary>
public class SchedulerService : ISchedulerService
{
    private readonly IConfigService _configService;
    private Timer? _timer;
    private DateTime? _nextRunTime;

    public event EventHandler<ScheduledBackupEventArgs>? BackupTriggered;

    public bool IsRunning { get; private set; }
    public DateTime? NextRunTime => _nextRunTime;

    public SchedulerService(IConfigService configService)
    {
        _configService = configService;
        _configService.ConfigChanged += OnConfigChanged;
    }

    private void OnConfigChanged(object? sender, EventArgs e)
    {
        // Restart scheduler when config changes
        if (IsRunning)
        {
            Restart();
        }
    }

    public void Start()
    {
        if (IsRunning)
            return;

        var schedule = _configService.Config.Schedule;
        if (!schedule.Enabled)
        {
            System.Diagnostics.Debug.WriteLine("Scheduler not enabled in config");
            return;
        }

        _nextRunTime = CalculateNextRunTime();
        var delay = _nextRunTime.Value - DateTime.Now;

        if (delay < TimeSpan.Zero)
        {
            delay = TimeSpan.Zero;
        }

        _timer = new Timer(OnTimerCallback, null, delay, Timeout.InfiniteTimeSpan);
        IsRunning = true;

        System.Diagnostics.Debug.WriteLine($"Scheduler started. Next run: {_nextRunTime}");
    }

    public void Stop()
    {
        _timer?.Dispose();
        _timer = null;
        IsRunning = false;
        _nextRunTime = null;

        System.Diagnostics.Debug.WriteLine("Scheduler stopped");
    }

    public void Restart()
    {
        Stop();
        Start();
    }

    public DateTime CalculateNextRunTime()
    {
        var schedule = _configService.Config.Schedule;
        var now = DateTime.Now;

        return schedule.Type switch
        {
            ScheduleType.Daily => CalculateDailyNextRun(now, schedule),
            ScheduleType.Hourly => CalculateHourlyNextRun(now, schedule),
            ScheduleType.Interval => CalculateIntervalNextRun(now, schedule),
            _ => now.AddMinutes(30)
        };
    }

    private DateTime CalculateIntervalNextRun(DateTime now, ScheduleSettings schedule)
    {
        var interval = schedule.IntervalMinutes;
        var lastBackup = _configService.Config.LastGlobalBackupTime;

        if (lastBackup.HasValue)
        {
            var nextFromLast = lastBackup.Value.AddMinutes(interval);
            // If the calculated next time is in the future, use it to maintain cadence
            if (nextFromLast > now)
            {
                return nextFromLast;
            }
        }

        // If no last backup or it was too long ago, schedule from now
        return now.AddMinutes(interval);
    }

    private DateTime CalculateDailyNextRun(DateTime now, ScheduleSettings schedule)
    {
        var dailyTime = schedule.DailyTime ?? new TimeSpan(2, 0, 0);
        var todayRun = now.Date.Add(dailyTime);

        // If today's run time has passed, schedule for tomorrow
        return todayRun > now ? todayRun : todayRun.AddDays(1);
    }

    private DateTime CalculateHourlyNextRun(DateTime now, ScheduleSettings schedule)
    {
        var minute = schedule.HourlyMinute;
        var thisHourRun = new DateTime(now.Year, now.Month, now.Day, now.Hour, minute, 0);

        // If this hour's run has passed, schedule for next hour
        return thisHourRun > now ? thisHourRun : thisHourRun.AddHours(1);
    }

    private void OnTimerCallback(object? state)
    {
        var scheduledTime = _nextRunTime ?? DateTime.Now;

        // Calculate next run time
        _nextRunTime = CalculateNextRunTime();
        var delay = _nextRunTime.Value - DateTime.Now;

        if (delay < TimeSpan.Zero)
        {
            delay = TimeSpan.FromSeconds(1);
        }

        // Reschedule timer for next run
        _timer?.Change(delay, Timeout.InfiniteTimeSpan);

        // Trigger backup
        BackupTriggered?.Invoke(this, new ScheduledBackupEventArgs(scheduledTime, _nextRunTime.Value));

        System.Diagnostics.Debug.WriteLine($"Backup triggered at {scheduledTime}. Next run: {_nextRunTime}");
    }
}
