using System.Windows;
using AutoBackup.Services.Interfaces;
using AutoBackup.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AutoBackup.Views;

/// <summary>
/// Interaction logic for SettingsWindow.xaml
/// </summary>
public partial class SettingsWindow : Window
{
    private readonly SettingsViewModel _viewModel;

    public SettingsWindow()
    {
        InitializeComponent();

        var configService = App.Services.GetRequiredService<IConfigService>();
        var schedulerService = App.Services.GetRequiredService<ISchedulerService>();

        _viewModel = new SettingsViewModel(configService, schedulerService);
        DataContext = _viewModel;

        _viewModel.SaveRequested += OnSaveRequested;
        _viewModel.CancelRequested += OnCancelRequested;
    }

    private void OnSaveRequested(object? sender, EventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void OnCancelRequested(object? sender, EventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.SaveCommand.Execute(null);
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
