namespace AutoBackup.Models;

/// <summary>
/// Root application configuration
/// </summary>
public class AppConfig
{
    /// <summary>
    /// List of backup items
    /// </summary>
    public List<BackupItem> BackupItems { get; set; } = new();

    /// <summary>
    /// Schedule settings for automatic backup
    /// </summary>
    public ScheduleSettings Schedule { get; set; } = new();

    /// <summary>
    /// Last successful global backup time
    /// </summary>
    public DateTime? LastGlobalBackupTime { get; set; }

    /// <summary>
    /// General application settings
    /// </summary>
    public GeneralSettings General { get; set; } = new();

    /// <summary>
    /// Global exclude patterns applied to all backups
    /// </summary>
    public List<string> GlobalExcludePatterns { get; set; } = new()
    {
        "*.tmp",
        "*.temp",
        "*.log",
        "Thumbs.db",
        "desktop.ini",
        ".DS_Store",
        "node_modules",
        ".git",
        "bin",
        "obj"
    };
}

/// <summary>
/// General application settings
/// </summary>
public class GeneralSettings
{
    /// <summary>
    /// Start application with Windows
    /// </summary>
    public bool StartWithWindows { get; set; } = false;

    /// <summary>
    /// Minimize to system tray on close
    /// </summary>
    public bool MinimizeToTray { get; set; } = true;

    /// <summary>
    /// Show toast notifications
    /// </summary>
    public bool ShowNotifications { get; set; } = true;

    /// <summary>
    /// Minimum free disk space in GB before warning
    /// </summary>
    public double MinFreeDiskSpaceGB { get; set; } = 1.0;

    /// <summary>
    /// Use hash comparison for file changes (slower but more accurate)
    /// </summary>
    public bool UseHashComparison { get; set; } = true;

    /// <summary>
    /// Maximum log file size in MB
    /// </summary>
    public int MaxLogFileSizeMB { get; set; } = 10;
}
