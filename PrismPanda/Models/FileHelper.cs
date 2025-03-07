using Microsoft.Win32;
using System.IO;

namespace PrismPanda.Models;

/// <summary> This class is used to help with file operations. </summary>
internal static class FileHelper
{
    /// <summary>
    /// Returns a distinct path by appending a number to the end if the file already exists.
    /// </summary>
    internal static string DistinctSavePath(string filePath, string extension)
    {
        var dir = Path.GetDirectoryName(filePath)
            ?? throw new ArgumentException("Cannot determine directory from file path.");
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var newPath = Path.Combine(dir, $"{fileName}_PrismPanda{extension}");
        for (int index = 1; File.Exists(newPath); index++)
            newPath = Path.Combine(dir, $"{fileName}_PrismPanda_{index}{extension}");
        return newPath;
    }

    /// <summary> Opens a file dialog to select an image file. </summary>
    internal static string? PickImage()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select an image file",
            Multiselect = false
        };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    /// <summary> Opens a file dialog to select a save path for an image file. </summary>
    internal static string? PickSavePath(string filter)
    {
        var dialog = new SaveFileDialog
        {
            Title = "Select a save path for the image file",
            Filter = filter,
            OverwritePrompt = true
        };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
