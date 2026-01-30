using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using AutoBackup.Models;
using AutoBackup.Services.Interfaces;

namespace AutoBackup.Services;

/// <summary>
/// Service for performing backup operations
/// </summary>
public class BackupService : IBackupService
{
    private readonly IConfigService _configService;
    private readonly IFileCompareService _fileCompareService;
    private readonly ILogService _logService;
    private CancellationTokenSource? _cts;
    private readonly ManualResetEventSlim _pauseEvent = new(true);
    private DateTime _lastProgressUpdate = DateTime.MinValue;

    public event EventHandler<BackupProgressEventArgs>? ProgressChanged;
    public event EventHandler<BackupCompletedEventArgs>? BackupCompleted;

    public bool IsRunning { get; private set; }
    public bool IsPaused { get; private set; }
    public BackupProgress CurrentProgress { get; private set; } = new();

    public BackupService(IConfigService configService, IFileCompareService fileCompareService, ILogService logService)
    {
        _configService = configService;
        _fileCompareService = fileCompareService;
        _logService = logService;
    }

    public async Task BackupAllAsync(CancellationToken cancellationToken = default)
    {
        if (IsRunning)
        {
            _logService.Warning("Backup already in progress");
            return;
        }

        var enabledItems = _configService.Config.BackupItems.Where(x => x.Enabled).ToList();
        if (enabledItems.Count == 0)
        {
            _logService.Warning("No backup items configured");
            return;
        }

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        IsRunning = true;
        var stopwatch = Stopwatch.StartNew();

        CurrentProgress = new BackupProgress
        {
            IsRunning = true,
            StatusMessage = "Starting backup..."
        };
        RaiseProgressChanged();

        _logService.Info("Starting backup for all items");

        int totalCopied = 0, totalFailed = 0;

        try
        {
            foreach (var item in enabledItems)
            {
                if (_cts.Token.IsCancellationRequested)
                    break;

                if (_cts.Token.IsCancellationRequested)
                    break;

                _pauseEvent.Wait(_cts.Token);

                await BackupItemInternalAsync(item, _cts.Token);
                totalCopied += item.LastBackupFileCount;
                totalFailed += item.LastBackupStatus == BackupStatus.Failed ? 1 : 0;
            }

            stopwatch.Stop();
            var success = totalFailed == 0;

            _logService.Success($"Backup completed: {totalCopied} files copied in {stopwatch.Elapsed.TotalSeconds:F1}s");

            BackupCompleted?.Invoke(this, new BackupCompletedEventArgs(
                success,
                CurrentProgress.TotalFiles,
                CurrentProgress.CopiedFiles,
                CurrentProgress.FailedFiles,
                stopwatch.Elapsed));
        }
        catch (OperationCanceledException)
        {
            _logService.Warning("Backup cancelled by user");
            BackupCompleted?.Invoke(this, new BackupCompletedEventArgs(
                false, 0, 0, 0, stopwatch.Elapsed, "Cancelled by user"));
        }
        catch (Exception ex)
        {
            _logService.Error("Backup failed", ex);
            BackupCompleted?.Invoke(this, new BackupCompletedEventArgs(
                false, 0, 0, 0, stopwatch.Elapsed, ex.Message));
        }
        finally
        {
            IsRunning = false;
            IsPaused = false;
            _pauseEvent.Set();
            CurrentProgress.IsRunning = false;
            CurrentProgress.StatusMessage = "Ready";
            RaiseProgressChanged(true);
            await _configService.SaveAsync();
        }
    }

    public async Task BackupItemAsync(BackupItem item, CancellationToken cancellationToken = default)
    {
        if (IsRunning)
        {
            _logService.Warning("Backup already in progress");
            return;
        }

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        IsRunning = true;
        var stopwatch = Stopwatch.StartNew();

        CurrentProgress = new BackupProgress
        {
            IsRunning = true,
            StatusMessage = $"Starting backup for {item.Name}..."
        };
        RaiseProgressChanged();

        try
        {
            await BackupItemInternalAsync(item, _cts.Token);
            stopwatch.Stop();

            var success = item.LastBackupStatus != BackupStatus.Failed;
            BackupCompleted?.Invoke(this, new BackupCompletedEventArgs(
                success,
                CurrentProgress.TotalFiles,
                CurrentProgress.CopiedFiles,
                CurrentProgress.FailedFiles,
                stopwatch.Elapsed));
        }
        catch (OperationCanceledException)
        {
            _logService.Warning($"Backup cancelled for {item.Name}");
            BackupCompleted?.Invoke(this, new BackupCompletedEventArgs(
                false, 0, 0, 0, stopwatch.Elapsed, "Cancelled"));
        }
        catch (Exception ex)
        {
            _logService.Error($"Backup failed for {item.Name}", ex);
            BackupCompleted?.Invoke(this, new BackupCompletedEventArgs(
                false, 0, 0, 0, stopwatch.Elapsed, ex.Message));
        }
        finally
        {
            IsRunning = false;
            IsPaused = false;
            _pauseEvent.Set();
            CurrentProgress.IsRunning = false;
            CurrentProgress.StatusMessage = "Ready";
            RaiseProgressChanged(true);
            await _configService.SaveAsync();
        }
    }

    private async Task BackupItemInternalAsync(BackupItem item, CancellationToken ct)
    {
        if (!Directory.Exists(item.SourcePath))
        {
            item.LastBackupStatus = BackupStatus.Failed;
            item.LastBackupError = "Source folder does not exist";
            _logService.Error($"Source folder does not exist: {item.SourcePath}", backupItemName: item.Name);
            return;
        }

        // Ensure target directory exists
        Directory.CreateDirectory(item.TargetPath);

        CurrentProgress.CurrentItemName = item.Name;
        CurrentProgress.StatusMessage = $"Scanning {item.Name}...";
        RaiseProgressChanged();

        _logService.Info($"Starting backup: {item.SourcePath} → {item.TargetPath}", item.Name);

        // Get all files to backup
        var searchOption = item.IncludeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        var sourceFiles = Directory.GetFiles(item.SourcePath, "*", searchOption);

        // Filter out excluded files
        var excludePatterns = item.ExcludePatterns.Concat(_configService.Config.GlobalExcludePatterns).ToList();
        var filesToBackup = sourceFiles.Where(f => !ShouldExclude(f, item.SourcePath, excludePatterns)).ToList();

        CurrentProgress.TotalFiles += filesToBackup.Count;
        int copied = 0, skipped = 0, failed = 0;

        foreach (var sourceFile in filesToBackup)
        {
            if (ct.IsCancellationRequested)
                break;

            _pauseEvent.Wait(ct);

            try
            {
                // Calculate relative path and target path
                var relativePath = Path.GetRelativePath(item.SourcePath, sourceFile);
                var targetFile = Path.Combine(item.TargetPath, relativePath);

                CurrentProgress.CurrentFile = relativePath;
                CurrentProgress.StatusMessage = $"Processing: {relativePath}";
                RaiseProgressChanged();

                // Check if file has changed
                var useHash = _configService.Config.General.UseHashComparison;
                var hasChanged = await _fileCompareService.HasFileChangedAsync(sourceFile, targetFile, useHash);

                if (hasChanged)
                {
                    // Ensure target directory exists
                    var targetDir = Path.GetDirectoryName(targetFile);
                    if (!string.IsNullOrEmpty(targetDir))
                    {
                        Directory.CreateDirectory(targetDir);
                    }

                    // Handle version backup
                    if (item.VersionBackup && File.Exists(targetFile))
                    {
                        var versionPath = GetVersionedPath(targetFile);
                        File.Move(targetFile, versionPath);
                        _logService.Info($"Versioned: {Path.GetFileName(targetFile)} → {Path.GetFileName(versionPath)}", item.Name);
                    }

                    // Copy file preserving metadata
                    File.Copy(sourceFile, targetFile, overwrite: true);
                    
                    // Preserve timestamps
                    var sourceInfo = new FileInfo(sourceFile);
                    File.SetLastWriteTimeUtc(targetFile, sourceInfo.LastWriteTimeUtc);

                    CurrentProgress.CopiedFiles++;
                    CurrentProgress.BytesCopied += sourceInfo.Length;
                    copied++;
                }
                else
                {
                    CurrentProgress.SkippedFiles++;
                    skipped++;
                }

                CurrentProgress.ProcessedFiles++;
                RaiseProgressChanged();
            }
            catch (Exception ex)
            {
                _logService.Error($"Failed to backup: {sourceFile}", ex, item.Name, sourceFile);
                CurrentProgress.FailedFiles++;
                CurrentProgress.ProcessedFiles++;
                failed++;
                RaiseProgressChanged();
            }
        }

        // Update item status
        item.LastBackupTime = DateTime.Now;
        item.LastBackupFileCount = copied;

        if (failed > 0 && copied > 0)
        {
            item.LastBackupStatus = BackupStatus.PartialSuccess;
            item.LastBackupError = $"{failed} files failed";
        }
        else if (failed > 0)
        {
            item.LastBackupStatus = BackupStatus.Failed;
            item.LastBackupError = $"All {failed} files failed";
        }
        else
        {
            item.LastBackupStatus = BackupStatus.Success;
            item.LastBackupError = null;
        }

        _logService.Success($"Completed {item.Name}: {copied} copied, {skipped} skipped, {failed} failed", item.Name);
    }

    private bool ShouldExclude(string filePath, string basePath, List<string> patterns)
    {
        var relativePath = Path.GetRelativePath(basePath, filePath);
        var fileName = Path.GetFileName(filePath);

        foreach (var pattern in patterns)
        {
            if (string.IsNullOrWhiteSpace(pattern))
                continue;

            // Check if pattern is a folder name
            if (!pattern.Contains('*') && !pattern.Contains('?'))
            {
                // Exact folder/file name match
                if (relativePath.Split(Path.DirectorySeparatorChar).Any(part => 
                    string.Equals(part, pattern, StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }
            else
            {
                // Wildcard pattern
                var regex = WildcardToRegex(pattern);
                if (Regex.IsMatch(fileName, regex, RegexOptions.IgnoreCase) ||
                    Regex.IsMatch(relativePath, regex, RegexOptions.IgnoreCase))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static string WildcardToRegex(string pattern)
    {
        return "^" + Regex.Escape(pattern)
            .Replace("\\*", ".*")
            .Replace("\\?", ".") + "$";
    }

    private static string GetVersionedPath(string filePath)
    {
        var dir = Path.GetDirectoryName(filePath) ?? "";
        var name = Path.GetFileNameWithoutExtension(filePath);
        var ext = Path.GetExtension(filePath);
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        return Path.Combine(dir, $"{name}_{timestamp}{ext}");
    }

    public void Cancel()
    {
        _cts?.Cancel();
        if (IsPaused) Resume(); // Ensure we can cancel if paused
    }

    public void Pause()
    {
        if (IsRunning && !IsPaused)
        {
            IsPaused = true;
            _pauseEvent.Reset();
            _logService.Info("Backup paused");
            CurrentProgress.IsPaused = true;
            CurrentProgress.StatusMessage = "Paused";
            RaiseProgressChanged(true);
        }
    }

    public void Resume()
    {
        if (IsRunning && IsPaused)
        {
            IsPaused = false;
            _pauseEvent.Set();
            _logService.Info("Backup resumed");
            CurrentProgress.IsPaused = false;
            CurrentProgress.StatusMessage = "Resuming...";
            RaiseProgressChanged(true);
        }
    }

    private void RaiseProgressChanged(bool force = false)
    {
        var now = DateTime.Now;
        if (force || (now - _lastProgressUpdate).TotalMilliseconds >= 100)
        {
            _lastProgressUpdate = now;
            ProgressChanged?.Invoke(this, new BackupProgressEventArgs(CurrentProgress));
        }
    }
}
