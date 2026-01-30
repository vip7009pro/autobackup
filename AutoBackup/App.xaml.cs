using System.Windows;
using AutoBackup.Helpers;
using AutoBackup.Services;
using AutoBackup.Services.Interfaces;
using AutoBackup.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AutoBackup;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private IServiceProvider? _serviceProvider;

    public static IServiceProvider Services => ((App)Current)._serviceProvider!;

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        // Configure services
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        // Create and show main window
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();

        // Check if started minimized
        if (StartupHelper.IsStartedMinimized())
        {
            mainWindow.WindowState = WindowState.Minimized;
            mainWindow.ShowInTaskbar = false;
        }
        else
        {
            mainWindow.Show();
        }
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Register services
        services.AddSingleton<IConfigService, ConfigService>();
        services.AddSingleton<IFileCompareService, FileCompareService>();
        services.AddSingleton<ILogService>(sp => new LogService(
            sp.GetRequiredService<IConfigService>().Config.General.MaxLogFileSizeMB));
        services.AddSingleton<IBackupService, BackupService>();
        services.AddSingleton<ISchedulerService, SchedulerService>();
        services.AddSingleton<INotificationService, NotificationService>();

        // Register ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<LogViewModel>();
        services.AddTransient<EditBackupItemViewModel>();

        // Register MainWindow
        services.AddSingleton<MainWindow>();
    }

    private void Application_Exit(object sender, ExitEventArgs e)
    {
        // Cleanup notifications
        Microsoft.Toolkit.Uwp.Notifications.ToastNotificationManagerCompat.Uninstall();
    }
}
