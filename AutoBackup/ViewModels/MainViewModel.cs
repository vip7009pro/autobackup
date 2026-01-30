using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using AutoBackup.Helpers;
using AutoBackup.Models;
using AutoBackup.Services.Interfaces;
using AutoBackup.ViewModels.Base;

namespace AutoBackup.ViewModels;

/// <summary>
/// Main ViewModel for the application
/// </summary>
public class MainViewModel : ViewModelBase
{
    private readonly IConfigService _configService;
    private readonly IBackupService _backupService;
    private readonly ISchedulerService _schedulerService;
    private readonly ILogService _logService;
    private readonly INotificationService _notificationService;
    private readonly Dispatcher _dispatcher;

    private BackupItemViewModel? _selectedItem;
    private bool _isBackingUp;
    private double _progressPercentage;
    private string _statusMessage = "Ready";
    private string _currentFile = "";
    private string _nextBackupTime = "";
    private string _lastBackupTimeDisplay = "Never";
    private string _globalStatus = "Idle";

    public ObservableCollection<BackupItemViewModel> BackupItems { get; } = new();

    public BackupItemViewModel? SelectedItem
    {
        get => _selectedItem;
        set => SetProperty(ref _selectedItem, value);
    }

    public bool IsBackingUp
    {
        get => _isBackingUp;
        set => SetProperty(ref _isBackingUp, value, () =>
        {
            ((AsyncRelayCommand)BackupAllCommand).RaiseCanExecuteChanged();
            ((AsyncRelayCommand)BackupSelectedCommand).RaiseCanExecuteChanged();
        });
    }

    public double ProgressPercentage
    {
        get => _progressPercentage;
        set => SetProperty(ref _progressPercentage, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public string CurrentFile
    {
        get => _currentFile;
        set => SetProperty(ref _currentFile, value);
    }

    public string NextBackupTime
    {
        get => _nextBackupTime;
        set => SetProperty(ref _nextBackupTime, value);
    }

    public string LastBackupTimeDisplay
    {
        get => _lastBackupTimeDisplay;
        set => SetProperty(ref _lastBackupTimeDisplay, value);
    }

    public string GlobalStatus
    {
        get => _globalStatus;
        set => SetProperty(ref _globalStatus, value);
    }

    // Commands
    public AsyncRelayCommand BackupAllCommand { get; }
    public AsyncRelayCommand BackupSelectedCommand { get; }
    public RelayCommand AddItemCommand { get; }
    public RelayCommand EditItemCommand { get; }
    public RelayCommand DeleteItemCommand { get; }
    public RelayCommand CancelBackupCommand { get; }
    public RelayCommand ShowWindowCommand { get; }
    public RelayCommand ExitCommand { get; }

    // Events for UI
    public event EventHandler? ShowWindowRequested;
    public event EventHandler? ExitRequested;
    public event EventHandler<BackupItem>? AddItemRequested;
    public event EventHandler<BackupItem>? EditItemRequested;

    public MainViewModel(
        IConfigService configService,
        IBackupService backupService,
        ISchedulerService schedulerService,
        ILogService logService,
        INotificationService notificationService)
    {
        _configService = configService;
        _backupService = backupService;
        _schedulerService = schedulerService;
        _logService = logService;
        _notificationService = notificationService;
        _dispatcher = Application.Current.Dispatcher;

        // Initialize commands
        BackupAllCommand = new AsyncRelayCommand(BackupAllAsync, () => !IsBackingUp);
        BackupSelectedCommand = new AsyncRelayCommand(BackupSelectedAsync, () => !IsBackingUp && SelectedItem != null);
        AddItemCommand = new RelayCommand(AddItem);
        EditItemCommand = new RelayCommand(EditItem, _ => SelectedItem != null);
        DeleteItemCommand = new RelayCommand(DeleteItem, _ => SelectedItem != null);
        CancelBackupCommand = new RelayCommand(CancelBackup, _ => IsBackingUp);
        ShowWindowCommand = new RelayCommand(_ => ShowWindowRequested?.Invoke(this, EventArgs.Empty));
        ExitCommand = new RelayCommand(_ => ExitRequested?.Invoke(this, EventArgs.Empty));

        // Subscribe to events
        _backupService.ProgressChanged += OnProgressChanged;
        _backupService.BackupCompleted += OnBackupCompleted;
        _schedulerService.BackupTriggered += OnScheduledBackup;
    }

    public async Task InitializeAsync()
    {
        await _configService.LoadAsync();
        await _logService.LoadAsync();

        // Load backup items
        RefreshBackupItems();

        // Start scheduler
        _schedulerService.Start();
        UpdateNextBackupTime();
        UpdateLastBackupTime();

        // Check disk space for all targets
        CheckDiskSpace();
    }

    private void RefreshBackupItems()
    {
        _dispatcher.Invoke(() =>
        {
            BackupItems.Clear();
            foreach (var item in _configService.Config.BackupItems)
            {
                BackupItems.Add(new BackupItemViewModel(item));
            }
        });
    }

    private async Task BackupAllAsync()
    {
        IsBackingUp = true;
        await _backupService.BackupAllAsync();
    }

    private async Task BackupSelectedAsync()
    {
        if (SelectedItem == null) return;

        IsBackingUp = true;
        var item = _configService.GetBackupItem(SelectedItem.Id);
        if (item != null)
        {
            await _backupService.BackupItemAsync(item);
        }
    }

    private void AddItem(object? parameter)
    {
        var newItem = new BackupItem();
        AddItemRequested?.Invoke(this, newItem);
    }

    public void OnItemAdded(BackupItem item)
    {
        _configService.AddBackupItem(item);
        _configService.SaveAsync();
        BackupItems.Add(new BackupItemViewModel(item));
    }

    private void EditItem(object? parameter)
    {
        if (SelectedItem == null) return;

        var item = _configService.GetBackupItem(SelectedItem.Id);
        if (item != null)
        {
            EditItemRequested?.Invoke(this, item);
        }
    }

    public void OnItemEdited(BackupItem item)
    {
        _configService.UpdateBackupItem(item);
        _configService.SaveAsync();

        var vm = BackupItems.FirstOrDefault(x => x.Id == item.Id);
        if (vm != null)
        {
            var index = BackupItems.IndexOf(vm);
            BackupItems[index] = new BackupItemViewModel(item);
        }
    }

    private void DeleteItem(object? parameter)
    {
        if (SelectedItem == null) return;

        var result = MessageBox.Show(
            $"Are you sure you want to delete '{SelectedItem.Name}'?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            _configService.RemoveBackupItem(SelectedItem.Id);
            _configService.SaveAsync();
            BackupItems.Remove(SelectedItem);
            SelectedItem = null;
        }
    }

    private void CancelBackup(object? parameter)
    {
        _backupService.Cancel();
        StatusMessage = "Cancelling...";
    }

    private void OnProgressChanged(object? sender, BackupProgressEventArgs e)
    {
        _dispatcher.Invoke(() =>
        {
            ProgressPercentage = e.Progress.ProgressPercentage;
            StatusMessage = e.Progress.StatusMessage;
            CurrentFile = e.Progress.CurrentFile;
            IsBackingUp = e.Progress.IsRunning;
            GlobalStatus = e.Progress.IsRunning ? "Backing up..." : "Idle";

            // Update item status
            var itemVm = BackupItems.FirstOrDefault(x => x.Name == e.Progress.CurrentItemName);
            if (itemVm != null)
            {
                itemVm.Status = BackupStatus.Running;
            }
        });
    }

    private void OnBackupCompleted(object? sender, BackupCompletedEventArgs e)
    {
        _dispatcher.Invoke(() =>
        {
            IsBackingUp = false;
            ProgressPercentage = 0;
            CurrentFile = "";
            StatusMessage = e.Success ? "Backup completed" : $"Backup failed: {e.ErrorMessage}";
            GlobalStatus = "Idle";

            if (e.Success)
            {
                _configService.Config.LastGlobalBackupTime = DateTime.Now;
                _configService.SaveAsync();
            }

            // Refresh items to show updated status
            RefreshBackupItems();

            // Show notification
            _notificationService.ShowBackupCompleted(
                e.CopiedFiles,
                e.TotalFiles - e.CopiedFiles - e.FailedFiles,
                e.FailedFiles,
                e.Duration);

            // Update next backup time
            UpdateNextBackupTime();
            UpdateLastBackupTime();
        });
    }

    private async void OnScheduledBackup(object? sender, ScheduledBackupEventArgs e)
    {
        await _dispatcher.InvokeAsync(async () =>
        {
            _logService.Info($"Scheduled backup triggered at {e.ScheduledTime}");
            await BackupAllAsync();
            UpdateNextBackupTime();
        });
    }

    private void UpdateNextBackupTime()
    {
        var nextRun = _schedulerService.NextRunTime;
        if (nextRun.HasValue && _configService.Config.Schedule.Enabled)
        {
            NextBackupTime = $"Next backup: {nextRun.Value:HH:mm}";
        }
        else
        {
            NextBackupTime = "Auto backup disabled";
        }
    }

    private void UpdateLastBackupTime()
    {
        var lastBackup = _configService.Config.LastGlobalBackupTime;
        LastBackupTimeDisplay = lastBackup.HasValue 
            ? lastBackup.Value.ToString("g") 
            : "Never";
    }

    private void CheckDiskSpace()
    {
        var minSpace = _configService.Config.General.MinFreeDiskSpaceGB;

        foreach (var item in _configService.Config.BackupItems)
        {
            var freeSpace = DiskSpaceHelper.GetFreeSpaceGB(item.TargetPath);
            if (freeSpace >= 0 && freeSpace < minSpace)
            {
                var driveLetter = DiskSpaceHelper.GetDriveLetter(item.TargetPath);
                _notificationService.ShowDiskSpaceWarning(driveLetter, freeSpace);
                _logService.Warning($"Low disk space on {driveLetter}: {freeSpace:F1} GB free", item.Name);
            }
        }
    }

    public void RefreshAfterSettingsChange()
    {
        RefreshBackupItems();
        UpdateNextBackupTime();
        UpdateLastBackupTime();
        CheckDiskSpace();
    }
}
