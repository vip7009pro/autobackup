using System.Collections.ObjectModel;
using System.IO;
using AutoBackup.Models;
using AutoBackup.ViewModels.Base;

namespace AutoBackup.ViewModels;

/// <summary>
/// ViewModel for the Edit Backup Item dialog
/// </summary>
public class EditBackupItemViewModel : ViewModelBase
{
    private string _name = "";
    private string _sourcePath = "";
    private string _targetPath = "";
    private bool _includeSubfolders = true;
    private bool _enabled = true;
    private bool _versionBackup = false;
    private string _excludePatterns = "";

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string SourcePath
    {
        get => _sourcePath;
        set
        {
            if (SetProperty(ref _sourcePath, value))
            {
                // Auto-generate name from source path if empty
                if (string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(value))
                {
                    Name = Path.GetFileName(value.TrimEnd(Path.DirectorySeparatorChar));
                }
            }
        }
    }

    public string TargetPath
    {
        get => _targetPath;
        set => SetProperty(ref _targetPath, value);
    }

    public bool IncludeSubfolders
    {
        get => _includeSubfolders;
        set => SetProperty(ref _includeSubfolders, value);
    }

    public bool Enabled
    {
        get => _enabled;
        set => SetProperty(ref _enabled, value);
    }

    public bool VersionBackup
    {
        get => _versionBackup;
        set => SetProperty(ref _versionBackup, value);
    }

    public string ExcludePatterns
    {
        get => _excludePatterns;
        set => SetProperty(ref _excludePatterns, value);
    }

    public bool IsValid =>
        !string.IsNullOrWhiteSpace(SourcePath) &&
        !string.IsNullOrWhiteSpace(TargetPath) &&
        Directory.Exists(SourcePath);

    public string ValidationMessage
    {
        get
        {
            if (string.IsNullOrWhiteSpace(SourcePath))
                return "Source path is required";
            if (!Directory.Exists(SourcePath))
                return "Source folder does not exist";
            if (string.IsNullOrWhiteSpace(TargetPath))
                return "Target path is required";
            return "";
        }
    }

    public RelayCommand BrowseSourceCommand { get; }
    public RelayCommand BrowseTargetCommand { get; }
    public RelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }

    public event EventHandler<BackupItem>? SaveRequested;
    public event EventHandler? CancelRequested;

    private BackupItem? _originalItem;

    public EditBackupItemViewModel()
    {
        BrowseSourceCommand = new RelayCommand(BrowseSource);
        BrowseTargetCommand = new RelayCommand(BrowseTarget);
        SaveCommand = new RelayCommand(Save, _ => IsValid);
        CancelCommand = new RelayCommand(_ => CancelRequested?.Invoke(this, EventArgs.Empty));
    }

    public void LoadItem(BackupItem? item)
    {
        _originalItem = item;

        if (item != null)
        {
            Name = item.Name;
            SourcePath = item.SourcePath;
            TargetPath = item.TargetPath;
            IncludeSubfolders = item.IncludeSubfolders;
            Enabled = item.Enabled;
            VersionBackup = item.VersionBackup;
            ExcludePatterns = string.Join(Environment.NewLine, item.ExcludePatterns);
        }
    }

    private void BrowseSource(object? parameter)
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = "Select Source Folder"
        };

        if (!string.IsNullOrEmpty(SourcePath) && Directory.Exists(SourcePath))
        {
            dialog.InitialDirectory = SourcePath;
        }

        if (dialog.ShowDialog() == true)
        {
            SourcePath = dialog.FolderName;
        }
    }

    private void BrowseTarget(object? parameter)
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = "Select Target Folder"
        };

        if (!string.IsNullOrEmpty(TargetPath) && Directory.Exists(TargetPath))
        {
            dialog.InitialDirectory = TargetPath;
        }

        if (dialog.ShowDialog() == true)
        {
            TargetPath = dialog.FolderName;
        }
    }

    private void Save(object? parameter)
    {
        var item = _originalItem ?? new BackupItem();

        item.Name = string.IsNullOrWhiteSpace(Name) 
            ? Path.GetFileName(SourcePath.TrimEnd(Path.DirectorySeparatorChar)) 
            : Name;
        item.SourcePath = SourcePath;
        item.TargetPath = TargetPath;
        item.IncludeSubfolders = IncludeSubfolders;
        item.Enabled = Enabled;
        item.VersionBackup = VersionBackup;
        item.ExcludePatterns = ExcludePatterns
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrEmpty(s))
            .ToList();

        SaveRequested?.Invoke(this, item);
    }
}
