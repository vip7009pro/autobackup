using System.IO;
using System.Text.Json;
using AutoBackup.Models;
using AutoBackup.Services.Interfaces;

namespace AutoBackup.Services;

/// <summary>
/// Service for managing application configuration stored in JSON
/// </summary>
public class ConfigService : IConfigService
{
    private readonly string _configPath;
    private readonly JsonSerializerOptions _jsonOptions;
    private AppConfig _config = new();

    public AppConfig Config => _config;
    public event EventHandler? ConfigChanged;

    public ConfigService()
    {
        // Store config in app directory
        var appDir = AppDomain.CurrentDomain.BaseDirectory;
        _configPath = Path.Combine(appDir, "config.json");

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task LoadAsync()
    {
        try
        {
            if (File.Exists(_configPath))
            {
                var json = await File.ReadAllTextAsync(_configPath);
                _config = JsonSerializer.Deserialize<AppConfig>(json, _jsonOptions) ?? new AppConfig();
            }
            else
            {
                // Create default config
                _config = new AppConfig();
                await SaveAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading config: {ex.Message}");
            _config = new AppConfig();
        }
    }

    public async Task SaveAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_config, _jsonOptions);
            await File.WriteAllTextAsync(_configPath, json);
            ConfigChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving config: {ex.Message}");
            throw;
        }
    }

    public void AddBackupItem(BackupItem item)
    {
        if (string.IsNullOrEmpty(item.Id))
        {
            item.Id = Guid.NewGuid().ToString();
        }

        // Generate name from source path if not set
        if (string.IsNullOrEmpty(item.Name))
        {
            item.Name = Path.GetFileName(item.SourcePath.TrimEnd(Path.DirectorySeparatorChar));
        }

        _config.BackupItems.Add(item);
    }

    public void UpdateBackupItem(BackupItem item)
    {
        var index = _config.BackupItems.FindIndex(x => x.Id == item.Id);
        if (index >= 0)
        {
            _config.BackupItems[index] = item;
        }
    }

    public void RemoveBackupItem(string id)
    {
        _config.BackupItems.RemoveAll(x => x.Id == id);
    }

    public BackupItem? GetBackupItem(string id)
    {
        return _config.BackupItems.FirstOrDefault(x => x.Id == id);
    }
}
