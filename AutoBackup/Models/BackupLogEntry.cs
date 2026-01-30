namespace AutoBackup.Models;

/// <summary>
/// Represents a log entry for backup operations
/// </summary>
public class BackupLogEntry
{
    /// <summary>
    /// Timestamp of the log entry
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.Now;

    /// <summary>
    /// Log level
    /// </summary>
    public LogLevel Level { get; set; } = LogLevel.Info;

    /// <summary>
    /// Log message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Related backup item name (if applicable)
    /// </summary>
    public string? BackupItemName { get; set; }

    /// <summary>
    /// File path involved (if applicable)
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// Exception details (if applicable)
    /// </summary>
    public string? ExceptionDetails { get; set; }

    public override string ToString()
    {
        var prefix = Level switch
        {
            LogLevel.Info => "[INFO]",
            LogLevel.Warning => "[WARN]",
            LogLevel.Error => "[ERROR]",
            LogLevel.Success => "[SUCCESS]",
            _ => "[INFO]"
        };
        
        var itemInfo = !string.IsNullOrEmpty(BackupItemName) ? $" [{BackupItemName}]" : "";
        return $"{Timestamp:yyyy-MM-dd HH:mm:ss} {prefix}{itemInfo} {Message}";
    }
}

/// <summary>
/// Log level enumeration
/// </summary>
public enum LogLevel
{
    Info,
    Warning,
    Error,
    Success
}
