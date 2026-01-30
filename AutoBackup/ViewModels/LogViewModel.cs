using System.Collections.ObjectModel;
using System.Windows.Threading;
using AutoBackup.Models;
using AutoBackup.Services.Interfaces;
using AutoBackup.ViewModels.Base;

namespace AutoBackup.ViewModels;

/// <summary>
/// ViewModel for the Log view
/// </summary>
public class LogViewModel : ViewModelBase
{
    private readonly ILogService _logService;
    private readonly Dispatcher _dispatcher;

    private LogLevel? _filterLevel;
    private string _filterText = "";
    private BackupLogEntry? _selectedEntry;

    public ObservableCollection<BackupLogEntry> LogEntries { get; } = new();
    public ObservableCollection<BackupLogEntry> FilteredEntries { get; } = new();

    public LogLevel? FilterLevel
    {
        get => _filterLevel;
        set
        {
            if (SetProperty(ref _filterLevel, value))
            {
                ApplyFilter();
            }
        }
    }

    public string FilterText
    {
        get => _filterText;
        set
        {
            if (SetProperty(ref _filterText, value))
            {
                ApplyFilter();
            }
        }
    }

    public BackupLogEntry? SelectedEntry
    {
        get => _selectedEntry;
        set => SetProperty(ref _selectedEntry, value);
    }

    public LogLevel[] LogLevels => Enum.GetValues<LogLevel>();

    public RelayCommand ClearCommand { get; }
    public AsyncRelayCommand ExportCommand { get; }
    public RelayCommand RefreshCommand { get; }

    public LogViewModel(ILogService logService)
    {
        _logService = logService;
        _dispatcher = System.Windows.Application.Current.Dispatcher;

        ClearCommand = new RelayCommand(Clear);
        ExportCommand = new AsyncRelayCommand(ExportAsync);
        RefreshCommand = new RelayCommand(Refresh);

        _logService.LogAdded += OnLogAdded;

        LoadEntries();
    }

    private void LoadEntries()
    {
        LogEntries.Clear();
        foreach (var entry in _logService.Entries.OrderByDescending(e => e.Timestamp))
        {
            LogEntries.Add(entry);
        }
        ApplyFilter();
    }

    private void OnLogAdded(object? sender, BackupLogEntry entry)
    {
        _dispatcher.Invoke(() =>
        {
            LogEntries.Insert(0, entry);
            if (MatchesFilter(entry))
            {
                FilteredEntries.Insert(0, entry);
            }

            // Keep max 1000 entries in UI
            while (LogEntries.Count > 1000)
            {
                LogEntries.RemoveAt(LogEntries.Count - 1);
            }
            while (FilteredEntries.Count > 1000)
            {
                FilteredEntries.RemoveAt(FilteredEntries.Count - 1);
            }
        });
    }

    private void ApplyFilter()
    {
        FilteredEntries.Clear();
        foreach (var entry in LogEntries.Where(MatchesFilter))
        {
            FilteredEntries.Add(entry);
        }
    }

    private bool MatchesFilter(BackupLogEntry entry)
    {
        if (FilterLevel.HasValue && entry.Level != FilterLevel.Value)
            return false;

        if (!string.IsNullOrWhiteSpace(FilterText))
        {
            var searchText = FilterText.ToLowerInvariant();
            return entry.Message.ToLowerInvariant().Contains(searchText) ||
                   (entry.BackupItemName?.ToLowerInvariant().Contains(searchText) ?? false) ||
                   (entry.FilePath?.ToLowerInvariant().Contains(searchText) ?? false);
        }

        return true;
    }

    private void Clear(object? parameter)
    {
        _logService.Clear();
        LogEntries.Clear();
        FilteredEntries.Clear();
    }

    private async Task ExportAsync(object? parameter)
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "Log Files|*.log|Text Files|*.txt|All Files|*.*",
            DefaultExt = ".log",
            FileName = $"backup_log_{DateTime.Now:yyyyMMdd_HHmmss}"
        };

        if (dialog.ShowDialog() == true)
        {
            await _logService.ExportAsync(dialog.FileName);
        }
    }

    private void Refresh(object? parameter)
    {
        LoadEntries();
    }
}
