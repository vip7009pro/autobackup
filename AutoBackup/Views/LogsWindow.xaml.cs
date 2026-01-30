using System.Windows;
using AutoBackup.Services.Interfaces;
using AutoBackup.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AutoBackup.Views;

/// <summary>
/// Interaction logic for LogsWindow.xaml
/// </summary>
public partial class LogsWindow : Window
{
    private readonly LogViewModel _viewModel;

    public LogsWindow()
    {
        InitializeComponent();

        var logService = App.Services.GetRequiredService<ILogService>();
        _viewModel = new LogViewModel(logService);
        DataContext = _viewModel;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
