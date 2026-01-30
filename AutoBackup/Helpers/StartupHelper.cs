using Microsoft.Win32;

namespace AutoBackup.Helpers;

/// <summary>
/// Helper class for Windows startup management
/// </summary>
public static class StartupHelper
{
    private const string AppName = "AutoBackup";
    private const string RegistryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

    /// <summary>
    /// Check if the app is set to start with Windows
    /// </summary>
    public static bool IsStartupEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKey, false);
            return key?.GetValue(AppName) != null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Enable or disable startup with Windows
    /// </summary>
    public static bool SetStartup(bool enable)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKey, true);
            if (key == null)
                return false;

            if (enable)
            {
                var exePath = Environment.ProcessPath;
                if (string.IsNullOrEmpty(exePath))
                    return false;

                // Add minimized flag to start minimized to tray
                key.SetValue(AppName, $"\"{exePath}\" --minimized");
            }
            else
            {
                key.DeleteValue(AppName, false);
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Check if application was started with --minimized flag
    /// </summary>
    public static bool IsStartedMinimized()
    {
        var args = Environment.GetCommandLineArgs();
        return args.Any(a => a.Equals("--minimized", StringComparison.OrdinalIgnoreCase));
    }
}
