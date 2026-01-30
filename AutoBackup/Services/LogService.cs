using System.Collections.Concurrent;
using System.IO;
using AutoBackup.Models;
using AutoBackup.Services.Interfaces;

namespace AutoBackup.Services;

/// <summary>
/// Service for logging backup operations to file and memory
/// </summary>
public class LogService : ILogService
{
    private readonly string _logPath;
    private readonly int _maxFileSizeBytes;
    private readonly ConcurrentQueue<BackupLogEntry> _entries = new();
    private readonly object _fileLock = new();

    public event EventHandler<BackupLogEntry>? LogAdded;
    public IReadOnlyList<BackupLogEntry> Entries => _entries.ToList();

    public LogService(int maxFileSizeMB = 10)
    {
        var appDir = AppDomain.CurrentDomain.BaseDirectory;
        _logPath = Path.Combine(appDir, "backup.log");
        _maxFileSizeBytes = maxFileSizeMB * 1024 * 1024;
    }

    public void Info(string message, string? backupItemName = null, string? filePath = null)
    {
        Log(new BackupLogEntry
        {
            Level = Models.LogLevel.Info,
            Message = message,
            BackupItemName = backupItemName,
            FilePath = filePath
        });
    }

    public void Warning(string message, string? backupItemName = null, string? filePath = null)
    {
        Log(new BackupLogEntry
        {
            Level = Models.LogLevel.Warning,
            Message = message,
            BackupItemName = backupItemName,
            FilePath = filePath
        });
    }

    public void Error(string message, Exception? exception = null, string? backupItemName = null, string? filePath = null)
    {
        Log(new BackupLogEntry
        {
            Level = Models.LogLevel.Error,
            Message = message,
            BackupItemName = backupItemName,
            FilePath = filePath,
            ExceptionDetails = exception?.ToString()
        });
    }

    public void Success(string message, string? backupItemName = null)
    {
        Log(new BackupLogEntry
        {
            Level = Models.LogLevel.Success,
            Message = message,
            BackupItemName = backupItemName
        });
    }

    private void Log(BackupLogEntry entry)
    {
        // Add to in-memory queue
        _entries.Enqueue(entry);

        // Keep max 1000 entries in memory
        while (_entries.Count > 1000)
        {
            _entries.TryDequeue(out _);
        }

        // Write to file
        WriteToFile(entry);

        // Raise event
        LogAdded?.Invoke(this, entry);
    }

    private void WriteToFile(BackupLogEntry entry)
    {
        try
        {
            lock (_fileLock)
            {
                // Check file size for rotation
                if (File.Exists(_logPath))
                {
                    var fileInfo = new FileInfo(_logPath);
                    if (fileInfo.Length > _maxFileSizeBytes)
                    {
                        RotateLogFile();
                    }
                }

                File.AppendAllText(_logPath, entry.ToString() + Environment.NewLine);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error writing to log file: {ex.Message}");
        }
    }

    private void RotateLogFile()
    {
        try
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var rotatedPath = Path.Combine(
                Path.GetDirectoryName(_logPath) ?? "",
                $"backup_{timestamp}.log");

            File.Move(_logPath, rotatedPath);

            // Keep only last 5 rotated logs
            var logDir = Path.GetDirectoryName(_logPath) ?? AppDomain.CurrentDomain.BaseDirectory;
            var oldLogs = Directory.GetFiles(logDir, "backup_*.log")
                .OrderByDescending(f => f)
                .Skip(5);

            foreach (var oldLog in oldLogs)
            {
                try { File.Delete(oldLog); } catch { }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error rotating log file: {ex.Message}");
        }
    }

    public void Clear()
    {
        while (_entries.TryDequeue(out _)) { }
    }

    public async Task LoadAsync()
    {
        try
        {
            if (!File.Exists(_logPath))
                return;

            var lines = await File.ReadAllLinesAsync(_logPath);
            
            // Only load last 500 lines
            var recentLines = lines.TakeLast(500);

            foreach (var line in recentLines)
            {
                // Parse log line (simple parsing)
                if (TryParseLogLine(line, out var entry))
                {
                    _entries.Enqueue(entry);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading log file: {ex.Message}");
        }
    }

    private bool TryParseLogLine(string line, out BackupLogEntry entry)
    {
        entry = new BackupLogEntry();

        try
        {
            // Format: "yyyy-MM-dd HH:mm:ss [LEVEL] [ItemName?] Message"
            if (string.IsNullOrWhiteSpace(line) || line.Length < 20)
                return false;

            var timestampStr = line.Substring(0, 19);
            if (DateTime.TryParse(timestampStr, out var timestamp))
            {
                entry.Timestamp = timestamp;
            }

            if (line.Contains("[INFO]")) entry.Level = Models.LogLevel.Info;
            else if (line.Contains("[WARN]")) entry.Level = Models.LogLevel.Warning;
            else if (line.Contains("[ERROR]")) entry.Level = Models.LogLevel.Error;
            else if (line.Contains("[SUCCESS]")) entry.Level = Models.LogLevel.Success;

            // Extract message (simplified)
            var messageStart = line.LastIndexOf(']') + 1;
            if (messageStart > 0 && messageStart < line.Length)
            {
                entry.Message = line.Substring(messageStart).Trim();
            }
            else
            {
                entry.Message = line;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task ExportAsync(string filePath)
    {
        var lines = _entries.Select(e => e.ToString());
        await File.WriteAllLinesAsync(filePath, lines);
    }
}
