using System;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using MsBox.Avalonia;
using OpenCvSharp;

namespace PrismPanda.Core;

public static class ImageManager
{
    private static Mat _image = new(), _thumbnail = new();

    public static IStorageFile? File { get; private set; }

    public static int FormatIndex { private get; set; }

    public static string Extension =>
        FormatIndex switch
        {
            0 => ".tif",
            1 => ".webp",
            2 => ".jpg",
            _ => ".png"
        };

    public static string BareFilename => File?.Name[..File.Name.LastIndexOf('.')] ?? "";

    public static async Task<bool> SetImage(IStorageFile file)
    {
        try
        {
            File = file;
            _image = new Mat(file.Path.AbsolutePath, ImreadModes.Unchanged);
            _thumbnail = _image.Resize(new Size(512, 512), 0, 0, InterpolationFlags.Lanczos4);
            return true;
        }
        catch (Exception ex)
        {
            File = null;
            _image = _thumbnail = new Mat();
            _ = await MessageBoxManager.GetMessageBoxStandard(
                    "Error", $"An error occurred while opening the image: {ex.Message}")
                .ShowAsync();
            return false;
        }
    }

    public static async Task<bool> SaveImage(IStorageFile file, int formatIndex)
    {
        try
        {
            if (File == null) return false;
            return true;
        }
        catch (Exception ex)
        {
            _ = await MessageBoxManager.GetMessageBoxStandard(
                    "Error", $"An error occurred while saving the image: {ex.Message}")
                .ShowAsync();
            return false;
        }
    }
}