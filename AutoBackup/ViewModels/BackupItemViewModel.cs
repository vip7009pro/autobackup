using AutoBackup.Models;
using AutoBackup.ViewModels.Base;

namespace AutoBackup.ViewModels;

/// <summary>
/// ViewModel for a single backup item
/// </summary>
public class BackupItemViewModel : ViewModelBase
{
    private readonly BackupItem _item;
    private BackupStatus _status;

    public string Id => _item.Id;
    public string Name => _item.Name;
    public string SourcePath => _item.SourcePath;
    public string TargetPath => _item.TargetPath;
    public bool IncludeSubfolders => _item.IncludeSubfolders;
    public bool Enabled => _item.Enabled;
    public DateTime? LastBackupTime => _item.LastBackupTime;
    public bool VersionBackup => _item.VersionBackup;

    public BackupStatus Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public string LastBackupDisplay
    {
        get
        {
            if (!LastBackupTime.HasValue)
                return "Never";

            var ago = DateTime.Now - LastBackupTime.Value;
            if (ago.TotalMinutes < 1)
                return "Just now";
            if (ago.TotalMinutes < 60)
                return $"{(int)ago.TotalMinutes}m ago";
            if (ago.TotalHours < 24)
                return $"{(int)ago.TotalHours}h ago";
            return LastBackupTime.Value.ToString("MMM dd, HH:mm");
        }
    }

    public string StatusDisplay
    {
        get
        {
            return _item.LastBackupStatus switch
            {
                BackupStatus.Success => "✓ Success",
                BackupStatus.PartialSuccess => $"⚠ {_item.LastBackupError}",
                BackupStatus.Failed => $"✗ {_item.LastBackupError}",
                BackupStatus.Running => "⟳ Running...",
                _ => "—"
            };
        }
    }

    public string SourceDisplay => TruncatePath(SourcePath, 40);
    public string TargetDisplay => TruncatePath(TargetPath, 40);

    public BackupItemViewModel(BackupItem item)
    {
        _item = item;
        _status = item.LastBackupStatus;
    }

    private static string TruncatePath(string path, int maxLength)
    {
        if (string.IsNullOrEmpty(path) || path.Length <= maxLength)
            return path;

        return "..." + path.Substring(path.Length - maxLength + 3);
    }
}
