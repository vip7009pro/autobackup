namespace AutoBackup.Services.Interfaces;

/// <summary>
/// Service for comparing files to detect changes
/// </summary>
public interface IFileCompareService
{
    /// <summary>
    /// Check if a file has changed compared to its backup
    /// </summary>
    /// <param name="sourcePath">Source file path</param>
    /// <param name="targetPath">Target (backup) file path</param>
    /// <param name="useHash">Use hash comparison (slower but more accurate)</param>
    /// <returns>True if file has changed or doesn't exist in target</returns>
    Task<bool> HasFileChangedAsync(string sourcePath, string targetPath, bool useHash = true);

    /// <summary>
    /// Compute hash of a file
    /// </summary>
    /// <param name="filePath">File path</param>
    /// <returns>SHA256 hash as hex string</returns>
    Task<string> ComputeHashAsync(string filePath);

    /// <summary>
    /// Compare two files using size and modification time
    /// </summary>
    bool CompareByMetadata(string sourcePath, string targetPath);

    /// <summary>
    /// Clear the hash cache
    /// </summary>
    void ClearCache();
}
