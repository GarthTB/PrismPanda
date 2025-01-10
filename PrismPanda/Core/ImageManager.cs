using System;
using System.Threading.Tasks;
using MsBox.Avalonia;
using OpenCvSharp;

namespace PrismPanda.Core;

public static class ImageManager
{
    private static Mat _image = new(), _thumbnail = new();

    public static async Task<bool> SetImage(string path)
    {
        try
        {
            _image = new Mat(path, ImreadModes.Unchanged);
            _thumbnail = _image.Resize(new Size(512, 512), 0, 0, InterpolationFlags.Lanczos4);
            return true;
        }
        catch (Exception ex)
        {
            _image = _thumbnail = new Mat();
            _ = await MessageBoxManager.GetMessageBoxStandard(
                    "Error", $"An error occurred while opening the image: {ex.Message}")
                .ShowAsync();
            return false;
        }
    }
}