using System.ComponentModel;
using System.Windows;
using AutoBackup.Models;
using AutoBackup.Services.Interfaces;
using AutoBackup.ViewModels;
using AutoBackup.Views;
using AutoBackup.Views.Dialogs;
using Microsoft.Extensions.DependencyInjection;

namespace AutoBackup;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private readonly IConfigService _configService;
    private bool _isExiting = false;

    public MainWindow(MainViewModel viewModel, IConfigService configService)
    {
        InitializeComponent();

        _viewModel = viewModel;
        _configService = configService;
        DataContext = _viewModel;

        // Subscribe to ViewModel events
        _viewModel.ShowWindowRequested += OnShowWindowRequested;
        _viewModel.ExitRequested += OnExitRequested;
        _viewModel.AddItemRequested += OnAddItemRequested;
        _viewModel.EditItemRequested += OnEditItemRequested;

        // Initialize async
        Loaded += async (s, e) => await _viewModel.InitializeAsync();
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        if (_isExiting)
        {
            // Actually exit
            TrayIcon.Dispose();
            return;
        }

        // Check if we should minimize to tray
        if (_configService.Config.General.MinimizeToTray)
        {
            e.Cancel = true;
            Hide();
            ShowInTaskbar = false;
        }
    }

    private void OnShowWindowRequested(object? sender, EventArgs e)
    {
        Show();
        ShowInTaskbar = true;
        WindowState = WindowState.Normal;
        Activate();
    }

    private void OnExitRequested(object? sender, EventArgs e)
    {
        _isExiting = true;
        Application.Current.Shutdown();
    }

    private void OnAddItemRequested(object? sender, BackupItem item)
    {
        var dialog = new EditBackupItemDialog();
        dialog.Owner = this;
        dialog.LoadItem(null);

        if (dialog.ShowDialog() == true && dialog.Result != null)
        {
            _viewModel.OnItemAdded(dialog.Result);
        }
    }

    private void OnEditItemRequested(object? sender, BackupItem item)
    {
        var dialog = new EditBackupItemDialog();
        dialog.Owner = this;
        dialog.LoadItem(item);

        if (dialog.ShowDialog() == true && dialog.Result != null)
        {
            _viewModel.OnItemEdited(dialog.Result);
        }
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsWindow();
        settingsWindow.Owner = this;
        settingsWindow.ShowDialog();
        _viewModel.RefreshAfterSettingsChange();
    }

    private void LogsButton_Click(object sender, RoutedEventArgs e)
    {
        var logsWindow = new LogsWindow();
        logsWindow.Owner = this;
        logsWindow.ShowDialog();
    }
}