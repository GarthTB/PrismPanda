using System.IO;

namespace PrismPanda.Models;

/// <summary> This class is used to help with file operations. </summary>
internal static class FileHelper
{
    /// <summary>
    /// Returns a distinct file path by appending a number to the end of
    /// the file name if the file already exists.
    /// <paramref name="extension"/> is without a dot.
    /// </summary>
    internal static string DistinctSavePath(string filePath, string extension)
    {
        var dir = Path.GetDirectoryName(filePath)
            ?? throw new ArgumentException("Cannot determine directory from file path.");
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var newPath = Path.Combine(dir, $"{fileName}.{extension}");
        for (int index = 1; File.Exists(newPath); index++)
            newPath = Path.Combine(dir, $"{fileName}_{index}.{extension}");
        return newPath;
    }
}
