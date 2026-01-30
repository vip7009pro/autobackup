using System.Collections.Concurrent;
using System.IO;
using System.Security.Cryptography;
using AutoBackup.Services.Interfaces;

namespace AutoBackup.Services;

/// <summary>
/// Service for comparing files using hash and metadata
/// </summary>
public class FileCompareService : IFileCompareService
{
    // Cache for file hashes to improve performance
    private readonly ConcurrentDictionary<string, (string Hash, DateTime ModTime, long Size)> _hashCache = new();

    public async Task<bool> HasFileChangedAsync(string sourcePath, string targetPath, bool useHash = true)
    {
        // If target doesn't exist, file needs to be copied
        if (!File.Exists(targetPath))
        {
            return true;
        }

        if (!File.Exists(sourcePath))
        {
            return false; // Source doesn't exist, nothing to backup
        }

        // First do a quick metadata check
        if (!CompareByMetadata(sourcePath, targetPath))
        {
            return true; // Files are different based on metadata
        }

        // If using hash comparison and metadata matches, verify with hash
        if (useHash)
        {
            var sourceHash = await ComputeHashAsync(sourcePath);
            var targetHash = await ComputeHashAsync(targetPath);
            return !string.Equals(sourceHash, targetHash, StringComparison.OrdinalIgnoreCase);
        }

        return false; // Metadata matches and not using hash
    }

    public bool CompareByMetadata(string sourcePath, string targetPath)
    {
        try
        {
            var sourceInfo = new FileInfo(sourcePath);
            var targetInfo = new FileInfo(targetPath);

            if (!sourceInfo.Exists || !targetInfo.Exists)
            {
                return false;
            }

            // Compare size and last write time
            return sourceInfo.Length == targetInfo.Length &&
                   sourceInfo.LastWriteTimeUtc == targetInfo.LastWriteTimeUtc;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> ComputeHashAsync(string filePath)
    {
        try
        {
            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists)
            {
                return string.Empty;
            }

            var cacheKey = filePath.ToLowerInvariant();

            // Check cache
            if (_hashCache.TryGetValue(cacheKey, out var cached))
            {
                // Verify cache is still valid
                if (cached.ModTime == fileInfo.LastWriteTimeUtc && cached.Size == fileInfo.Length)
                {
                    return cached.Hash;
                }
            }

            // Compute hash
            using var sha256 = SHA256.Create();
            await using var stream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 81920,
                useAsync: true);

            var hashBytes = await sha256.ComputeHashAsync(stream);
            var hash = Convert.ToHexString(hashBytes);

            // Update cache
            _hashCache[cacheKey] = (hash, fileInfo.LastWriteTimeUtc, fileInfo.Length);

            return hash;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error computing hash for {filePath}: {ex.Message}");
            return string.Empty;
        }
    }

    public void ClearCache()
    {
        _hashCache.Clear();
    }
}
