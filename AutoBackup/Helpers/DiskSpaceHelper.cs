using System.IO;

namespace AutoBackup.Helpers;

/// <summary>
/// Helper class for disk space operations
/// </summary>
public static class DiskSpaceHelper
{
    /// <summary>
    /// Get free space in GB for a given path
    /// </summary>
    public static double GetFreeSpaceGB(string path)
    {
        try
        {
            var root = Path.GetPathRoot(path);
            if (string.IsNullOrEmpty(root))
                return -1;

            var driveInfo = new DriveInfo(root);
            return driveInfo.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0);
        }
        catch
        {
            return -1;
        }
    }

    /// <summary>
    /// Get total space in GB for a given path
    /// </summary>
    public static double GetTotalSpaceGB(string path)
    {
        try
        {
            var root = Path.GetPathRoot(path);
            if (string.IsNullOrEmpty(root))
                return -1;

            var driveInfo = new DriveInfo(root);
            return driveInfo.TotalSize / (1024.0 * 1024.0 * 1024.0);
        }
        catch
        {
            return -1;
        }
    }

    /// <summary>
    /// Get the drive letter for a given path
    /// </summary>
    public static string GetDriveLetter(string path)
    {
        try
        {
            var root = Path.GetPathRoot(path);
            return root?.TrimEnd('\\', '/') ?? "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }

    /// <summary>
    /// Check if there is enough free space for backup
    /// </summary>
    public static bool HasEnoughSpace(string targetPath, double minFreeSpaceGB)
    {
        var freeSpace = GetFreeSpaceGB(targetPath);
        return freeSpace < 0 || freeSpace >= minFreeSpaceGB;
    }

    /// <summary>
    /// Format bytes to human-readable string
    /// </summary>
    public static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }
}
