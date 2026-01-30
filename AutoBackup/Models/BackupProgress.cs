namespace AutoBackup.Models;

/// <summary>
/// Progress information for backup operations
/// </summary>
public class BackupProgress
{
    /// <summary>
    /// Total number of files to process
    /// </summary>
    public int TotalFiles { get; set; }

    /// <summary>
    /// Number of files processed
    /// </summary>
    public int ProcessedFiles { get; set; }

    /// <summary>
    /// Number of files copied
    /// </summary>
    public int CopiedFiles { get; set; }

    /// <summary>
    /// Number of files skipped (unchanged)
    /// </summary>
    public int SkippedFiles { get; set; }

    /// <summary>
    /// Number of files that failed
    /// </summary>
    public int FailedFiles { get; set; }

    /// <summary>
    /// Total bytes copied
    /// </summary>
    public long BytesCopied { get; set; }

    /// <summary>
    /// Current file being processed
    /// </summary>
    public string CurrentFile { get; set; } = string.Empty;

    /// <summary>
    /// Current backup item name
    /// </summary>
    public string CurrentItemName { get; set; } = string.Empty;

    /// <summary>
    /// Progress percentage (0-100)
    /// </summary>
    public double ProgressPercentage => TotalFiles > 0 ? (ProcessedFiles * 100.0 / TotalFiles) : 0;

    /// <summary>
    /// Whether the backup is currently running
    /// </summary>
    public bool IsRunning { get; set; }

    /// <summary>
    /// Status message
    /// </summary>
    public string StatusMessage { get; set; } = "Ready";
}
