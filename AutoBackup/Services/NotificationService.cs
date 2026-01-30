using AutoBackup.Services.Interfaces;
using Microsoft.Toolkit.Uwp.Notifications;

namespace AutoBackup.Services;

/// <summary>
/// Service for showing Windows toast notifications
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IConfigService _configService;

    public NotificationService(IConfigService configService)
    {
        _configService = configService;
    }

    private bool IsEnabled => _configService.Config.General.ShowNotifications;

    public void ShowSuccess(string title, string message)
    {
        if (!IsEnabled) return;
        ShowToast(title, message);
    }

    public void ShowError(string title, string message)
    {
        if (!IsEnabled) return;
        ShowToast("❌ " + title, message);
    }

    public void ShowWarning(string title, string message)
    {
        if (!IsEnabled) return;
        ShowToast("⚠️ " + title, message);
    }

    public void ShowInfo(string title, string message)
    {
        if (!IsEnabled) return;
        ShowToast(title, message);
    }

    public void ShowBackupCompleted(int copiedFiles, int skippedFiles, int failedFiles, TimeSpan duration)
    {
        if (!IsEnabled) return;

        var status = failedFiles > 0 ? "⚠️ Backup Completed with Errors" : "✅ Backup Completed";
        var details = $"{copiedFiles} files backed up, {skippedFiles} unchanged";
        
        if (failedFiles > 0)
        {
            details += $", {failedFiles} failed";
        }

        details += $" ({duration.TotalSeconds:F1}s)";

        ShowToast(status, details);
    }

    public void ShowDiskSpaceWarning(string driveLetter, double freeSpaceGB)
    {
        if (!IsEnabled) return;
        ShowToast("⚠️ Low Disk Space Warning", $"Drive {driveLetter} has only {freeSpaceGB:F1} GB free space remaining");
    }

    private static void ShowToast(string title, string message)
    {
        try
        {
            var builder = new ToastContentBuilder()
                .AddText(title)
                .AddText(message);
            
            builder.Show();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to show toast: {ex.Message}");
        }
    }
}
