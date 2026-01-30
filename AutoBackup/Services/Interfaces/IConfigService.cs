using AutoBackup.Models;

namespace AutoBackup.Services.Interfaces;

/// <summary>
/// Service for managing application configuration
/// </summary>
public interface IConfigService
{
    /// <summary>
    /// Current application configuration
    /// </summary>
    AppConfig Config { get; }

    /// <summary>
    /// Event raised when configuration changes
    /// </summary>
    event EventHandler? ConfigChanged;

    /// <summary>
    /// Load configuration from file
    /// </summary>
    Task LoadAsync();

    /// <summary>
    /// Save configuration to file
    /// </summary>
    Task SaveAsync();

    /// <summary>
    /// Add a new backup item
    /// </summary>
    void AddBackupItem(BackupItem item);

    /// <summary>
    /// Update an existing backup item
    /// </summary>
    void UpdateBackupItem(BackupItem item);

    /// <summary>
    /// Remove a backup item
    /// </summary>
    void RemoveBackupItem(string id);

    /// <summary>
    /// Get a backup item by id
    /// </summary>
    BackupItem? GetBackupItem(string id);
}
