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
            0 => "tif",
            1 => "webp",
            2 => "jpg",
            _ => "png"
        };

    public static string BareFilename => File?.Name[..File.Name.LastIndexOf('.')] ?? "";

    public static async Task<bool> SetImage(IStorageFile file)
    {
        try
        {
            File = file;
            new Mat(file.Path.AbsolutePath, ImreadModes.Color | ImreadModes.AnyDepth)
                .CvtColor(ColorConversionCodes.BGR2XYZ)
                .ConvertTo(_image, MatType.CV_32FC3);
            new Mat(file.Path.AbsolutePath, ImreadModes.Color | ImreadModes.AnyDepth)
                .Resize(new Size(512, 512), 0, 0, InterpolationFlags.Lanczos4)
                .CvtColor(ColorConversionCodes.BGR2XYZ)
                .ConvertTo(_thumbnail, MatType.CV_32FC3);
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

    public static async Task<bool> AdjustAndSaveImage(IStorageFile file)
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