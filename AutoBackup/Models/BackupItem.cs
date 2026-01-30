using System.Text.Json.Serialization;

namespace AutoBackup.Models;

/// <summary>
/// Represents a backup item configuration
/// </summary>
public class BackupItem
{
    /// <summary>
    /// Unique identifier for this backup item
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Display name for the backup item
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Source folder path to backup
    /// </summary>
    public string SourcePath { get; set; } = string.Empty;

    /// <summary>
    /// Target folder path where backup will be stored
    /// </summary>
    public string TargetPath { get; set; } = string.Empty;

    /// <summary>
    /// Whether to include subfolders in backup
    /// </summary>
    public bool IncludeSubfolders { get; set; } = true;

    /// <summary>
    /// Whether this backup item is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Last backup timestamp
    /// </summary>
    public DateTime? LastBackupTime { get; set; }

    /// <summary>
    /// Patterns to exclude from backup (e.g., *.tmp, node_modules)
    /// </summary>
    public List<string> ExcludePatterns { get; set; } = new();

    /// <summary>
    /// Whether to keep versioned backups with timestamps
    /// </summary>
    public bool VersionBackup { get; set; } = false;

    /// <summary>
    /// Number of files backed up in last run
    /// </summary>
    [JsonIgnore]
    public int LastBackupFileCount { get; set; }

    /// <summary>
    /// Status of last backup
    /// </summary>
    [JsonIgnore]
    public BackupStatus LastBackupStatus { get; set; } = BackupStatus.None;

    /// <summary>
    /// Error message if last backup failed
    /// </summary>
    [JsonIgnore]
    public string? LastBackupError { get; set; }
}

/// <summary>
/// Backup status enumeration
/// </summary>
public enum BackupStatus
{
    None,
    Running,
    Success,
    PartialSuccess,
    Failed
}
