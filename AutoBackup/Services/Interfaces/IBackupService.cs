using AutoBackup.Models;

namespace AutoBackup.Services.Interfaces;

/// <summary>
/// Event args for backup progress updates
/// </summary>
public class BackupProgressEventArgs : EventArgs
{
    public BackupProgress Progress { get; }
    public BackupProgressEventArgs(BackupProgress progress) => Progress = progress;
}

/// <summary>
/// Event args for backup completion
/// </summary>
public class BackupCompletedEventArgs : EventArgs
{
    public bool Success { get; }
    public int TotalFiles { get; }
    public int CopiedFiles { get; }
    public int FailedFiles { get; }
    public TimeSpan Duration { get; }
    public string? ErrorMessage { get; }

    public BackupCompletedEventArgs(bool success, int totalFiles, int copiedFiles, int failedFiles, TimeSpan duration, string? errorMessage = null)
    {
        Success = success;
        TotalFiles = totalFiles;
        CopiedFiles = copiedFiles;
        FailedFiles = failedFiles;
        Duration = duration;
        ErrorMessage = errorMessage;
    }
}

/// <summary>
/// Service for performing backup operations
/// </summary>
public interface IBackupService
{
    /// <summary>
    /// Event raised when backup progress changes
    /// </summary>
    event EventHandler<BackupProgressEventArgs>? ProgressChanged;

    /// <summary>
    /// Event raised when backup completes
    /// </summary>
    event EventHandler<BackupCompletedEventArgs>? BackupCompleted;

    /// <summary>
    /// Whether a backup is currently running
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Current backup progress
    /// </summary>
    BackupProgress CurrentProgress { get; }

    /// <summary>
    /// Backup all enabled items
    /// </summary>
    Task BackupAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Backup a specific item
    /// </summary>
    Task BackupItemAsync(BackupItem item, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancel the current backup operation
    /// </summary>
    void Cancel();
}
