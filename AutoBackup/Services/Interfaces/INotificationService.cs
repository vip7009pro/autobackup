namespace AutoBackup.Services.Interfaces;

/// <summary>
/// Service for showing notifications
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Show a success notification
    /// </summary>
    void ShowSuccess(string title, string message);

    /// <summary>
    /// Show an error notification
    /// </summary>
    void ShowError(string title, string message);

    /// <summary>
    /// Show a warning notification
    /// </summary>
    void ShowWarning(string title, string message);

    /// <summary>
    /// Show an info notification
    /// </summary>
    void ShowInfo(string title, string message);

    /// <summary>
    /// Show backup completed notification
    /// </summary>
    void ShowBackupCompleted(int copiedFiles, int skippedFiles, int failedFiles, TimeSpan duration);

    /// <summary>
    /// Show disk space warning
    /// </summary>
    void ShowDiskSpaceWarning(string driveLetter, double freeSpaceGB);
}
