using AutoBackup.Models;

namespace AutoBackup.Services.Interfaces;

/// <summary>
/// Service for logging backup operations
/// </summary>
public interface ILogService
{
    /// <summary>
    /// Event raised when a new log entry is added
    /// </summary>
    event EventHandler<BackupLogEntry>? LogAdded;

    /// <summary>
    /// All log entries (in-memory)
    /// </summary>
    IReadOnlyList<BackupLogEntry> Entries { get; }

    /// <summary>
    /// Log an info message
    /// </summary>
    void Info(string message, string? backupItemName = null, string? filePath = null);

    /// <summary>
    /// Log a warning message
    /// </summary>
    void Warning(string message, string? backupItemName = null, string? filePath = null);

    /// <summary>
    /// Log an error message
    /// </summary>
    void Error(string message, Exception? exception = null, string? backupItemName = null, string? filePath = null);

    /// <summary>
    /// Log a success message
    /// </summary>
    void Success(string message, string? backupItemName = null);

    /// <summary>
    /// Clear all in-memory log entries
    /// </summary>
    void Clear();

    /// <summary>
    /// Load log entries from file
    /// </summary>
    Task LoadAsync();

    /// <summary>
    /// Export logs to a file
    /// </summary>
    Task ExportAsync(string filePath);
}
