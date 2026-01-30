using System.Windows;
using AutoBackup.Helpers;
using AutoBackup.Models;
using AutoBackup.Services.Interfaces;
using AutoBackup.ViewModels.Base;

namespace AutoBackup.ViewModels;

/// <summary>
/// ViewModel for the Settings view
/// </summary>
public class SettingsViewModel : ViewModelBase
{
    private readonly IConfigService _configService;
    private readonly ISchedulerService _schedulerService;

    // Schedule settings
    private bool _scheduleEnabled;
    private ScheduleType _scheduleType;
    private int _intervalMinutes;
    private TimeSpan _dailyTime;
    private int _hourlyMinute;

    // General settings
    private bool _startWithWindows;
    private bool _minimizeToTray;
    private bool _showNotifications;
    private double _minFreeDiskSpaceGB;
    private bool _useHashComparison;

    // Exclude patterns
    private string _globalExcludePatterns = "";

    public bool ScheduleEnabled
    {
        get => _scheduleEnabled;
        set => SetProperty(ref _scheduleEnabled, value);
    }

    public ScheduleType ScheduleType
    {
        get => _scheduleType;
        set => SetProperty(ref _scheduleType, value);
    }

    public int IntervalMinutes
    {
        get => _intervalMinutes;
        set => SetProperty(ref _intervalMinutes, Math.Max(1, value));
    }

    public TimeSpan DailyTime
    {
        get => _dailyTime;
        set => SetProperty(ref _dailyTime, value);
    }

    public int DailyHour
    {
        get => (int)_dailyTime.TotalHours;
        set => DailyTime = new TimeSpan(value, DailyMinute, 0);
    }

    public int DailyMinute
    {
        get => _dailyTime.Minutes;
        set => DailyTime = new TimeSpan(DailyHour, value, 0);
    }

    public int HourlyMinute
    {
        get => _hourlyMinute;
        set => SetProperty(ref _hourlyMinute, Math.Clamp(value, 0, 59));
    }

    public bool StartWithWindows
    {
        get => _startWithWindows;
        set => SetProperty(ref _startWithWindows, value);
    }

    public bool MinimizeToTray
    {
        get => _minimizeToTray;
        set => SetProperty(ref _minimizeToTray, value);
    }

    public bool ShowNotifications
    {
        get => _showNotifications;
        set => SetProperty(ref _showNotifications, value);
    }

    public double MinFreeDiskSpaceGB
    {
        get => _minFreeDiskSpaceGB;
        set => SetProperty(ref _minFreeDiskSpaceGB, Math.Max(0.1, value));
    }

    public bool UseHashComparison
    {
        get => _useHashComparison;
        set => SetProperty(ref _useHashComparison, value);
    }

    public string GlobalExcludePatterns
    {
        get => _globalExcludePatterns;
        set => SetProperty(ref _globalExcludePatterns, value);
    }

    public ScheduleType[] ScheduleTypes => Enum.GetValues<ScheduleType>();

    public RelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }

    public event EventHandler? SaveRequested;
    public event EventHandler? CancelRequested;

    public SettingsViewModel(IConfigService configService, ISchedulerService schedulerService)
    {
        _configService = configService;
        _schedulerService = schedulerService;

        SaveCommand = new RelayCommand(Save);
        CancelCommand = new RelayCommand(_ => CancelRequested?.Invoke(this, EventArgs.Empty));

        LoadSettings();
    }

    private void LoadSettings()
    {
        var schedule = _configService.Config.Schedule;
        var general = _configService.Config.General;

        _scheduleEnabled = schedule.Enabled;
        _scheduleType = schedule.Type;
        _intervalMinutes = schedule.IntervalMinutes;
        _dailyTime = schedule.DailyTime ?? new TimeSpan(2, 0, 0);
        _hourlyMinute = schedule.HourlyMinute;

        _startWithWindows = general.StartWithWindows;
        _minimizeToTray = general.MinimizeToTray;
        _showNotifications = general.ShowNotifications;
        _minFreeDiskSpaceGB = general.MinFreeDiskSpaceGB;
        _useHashComparison = general.UseHashComparison;

        _globalExcludePatterns = string.Join(Environment.NewLine, _configService.Config.GlobalExcludePatterns);
    }

    private void Save(object? parameter)
    {
        // Update schedule settings
        _configService.Config.Schedule.Enabled = ScheduleEnabled;
        _configService.Config.Schedule.Type = ScheduleType;
        _configService.Config.Schedule.IntervalMinutes = IntervalMinutes;
        _configService.Config.Schedule.DailyTime = DailyTime;
        _configService.Config.Schedule.HourlyMinute = HourlyMinute;

        // Update general settings
        _configService.Config.General.StartWithWindows = StartWithWindows;
        _configService.Config.General.MinimizeToTray = MinimizeToTray;
        _configService.Config.General.ShowNotifications = ShowNotifications;
        _configService.Config.General.MinFreeDiskSpaceGB = MinFreeDiskSpaceGB;
        _configService.Config.General.UseHashComparison = UseHashComparison;

        // Update exclude patterns
        _configService.Config.GlobalExcludePatterns = GlobalExcludePatterns
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrEmpty(s))
            .ToList();

        // Apply Windows startup setting
        if (StartWithWindows != StartupHelper.IsStartupEnabled())
        {
            StartupHelper.SetStartup(StartWithWindows);
        }

        // Save config
        _configService.SaveAsync();

        // Restart scheduler
        _schedulerService.Restart();

        SaveRequested?.Invoke(this, EventArgs.Empty);
    }
}
