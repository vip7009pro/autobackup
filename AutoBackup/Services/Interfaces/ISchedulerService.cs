namespace AutoBackup.Services.Interfaces;

/// <summary>
/// Event args for scheduled backup trigger
/// </summary>
public class ScheduledBackupEventArgs : EventArgs
{
    public DateTime ScheduledTime { get; }
    public DateTime NextRunTime { get; }

    public ScheduledBackupEventArgs(DateTime scheduledTime, DateTime nextRunTime)
    {
        ScheduledTime = scheduledTime;
        NextRunTime = nextRunTime;
    }
}

/// <summary>
/// Service for scheduling automatic backups
/// </summary>
public interface ISchedulerService
{
    /// <summary>
    /// Event raised when a scheduled backup should run
    /// </summary>
    event EventHandler<ScheduledBackupEventArgs>? BackupTriggered;

    /// <summary>
    /// Whether the scheduler is running
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Next scheduled backup time
    /// </summary>
    DateTime? NextRunTime { get; }

    /// <summary>
    /// Start the scheduler
    /// </summary>
    void Start();

    /// <summary>
    /// Stop the scheduler
    /// </summary>
    void Stop();

    /// <summary>
    /// Restart the scheduler (e.g., after settings change)
    /// </summary>
    void Restart();

    /// <summary>
    /// Calculate next run time based on current schedule settings
    /// </summary>
    DateTime CalculateNextRunTime();
}
