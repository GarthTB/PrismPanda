using System;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using MsBox.Avalonia;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace PrismPanda.Core;

public static class ImageManager
{
    private static Mat _image = new(), _thumbnail = new();

    private static Mat? _resultTemp;

    public static IStorageFile? File { get; private set; }

    public static int FormatIndex { private get; set; }

    public static string BareFilename => File?.Name[..File.Name.LastIndexOf('.')] ?? "";

    public static string Extension =>
        FormatIndex switch
        {
            0 => "tif",
            1 => "webp",
            2 => "jpg",
            _ => "png"
        };

    public static async Task<bool> SetImage(IStorageFile file)
    {
        try
        {
            File = file;
            new Mat(file.Path.AbsolutePath, ImreadModes.Color | ImreadModes.AnyDepth)
                .CvtColor(ColorConversionCodes.BGR2XYZ)
                .ConvertTo(_image, MatType.CV_32FC3);
            var size = _image is { Width: <= 512, Height: <= 512 } ? new Size(_image.Width, _image.Height) :
                _image.Width > _image.Height ? new Size(512, 512 * _image.Height / _image.Width) :
                new Size(512 * _image.Width / _image.Height, 512);
            new Mat(file.Path.AbsolutePath, ImreadModes.Color | ImreadModes.AnyDepth)
                .Resize(size, 0, 0, InterpolationFlags.Lanczos4)
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
        finally { _resultTemp = null; }
    }

    public static async Task<Bitmap?> GeneratePreview(
        int colorSpaceId, double ch1Gain = 1, double ch2Gain = 1, double ch3Gain = 1)
    {
        try
        {
            var eightBit = _thumbnail.Clone();
            if (colorSpaceId == -1 || (ch1Gain == 1 && ch2Gain == 1 && ch3Gain == 1))
                _thumbnail.CvtColor(ColorConversionCodes.XYZ2BGR).ConvertTo(eightBit, MatType.CV_8UC3);
            else
                _thumbnail.SplitGains(colorSpaceId, ch1Gain, ch2Gain, ch3Gain)
                    .CvtColor(ColorConversionCodes.XYZ2BGR)
                    .ConvertTo(eightBit, MatType.CV_8UC3);
            var bitmap = eightBit.ToBitmap(PixelFormat.Format24bppRgb);
            using MemoryStream memoryStream = new();
            bitmap.Save(memoryStream, ImageFormat.Png);
            memoryStream.Position = 0;
            return new Bitmap(memoryStream);
        }
        catch (Exception ex)
        {
            _ = await MessageBoxManager.GetMessageBoxStandard(
                    "Error", $"An error occurred while generating the preview: {ex.Message}")
                .ShowAsync();
            return null;
        }
        finally { _resultTemp = null; }
    }

    public static async Task<bool> AdjustAndSaveImage(
        IStorageFile file, int colorSpaceId, double ch1Gain, double ch2Gain, double ch3Gain, int formatIndex)
    {
        try
        {
            _resultTemp ??= _image.SplitGains(colorSpaceId, ch1Gain, ch2Gain, ch3Gain)
                .CvtColor(ColorConversionCodes.XYZ2BGR);
            Mat outputMat = new();
            switch (formatIndex)
            {
                case 0: // TIFF (lossless, 16-bit)
                    var multiplier = new Mat(_resultTemp.Size(), _resultTemp.Type(), new Scalar(256, 256, 256, 256));
                    Cv2.Multiply(_resultTemp, multiplier, outputMat);
                    outputMat.ConvertTo(outputMat, MatType.CV_16UC3);
                    return outputMat.SaveImage(
                        file.Path.AbsolutePath, new ImageEncodingParam(ImwriteFlags.TiffCompression, 32946));
                case 1: // WebP (lossless, 8-bit, max quality)
                    _resultTemp.ConvertTo(outputMat, MatType.CV_8UC3);
                    return outputMat.SaveImage(file.Path.AbsolutePath);
                case 2: // JPEG (lossy, 8-bit, max quality)
                    _resultTemp.ConvertTo(outputMat, MatType.CV_8UC3);
                    return outputMat.SaveImage(
                        file.Path.AbsolutePath, new ImageEncodingParam(ImwriteFlags.JpegQuality, 100));
                default: // PNG (lossless, 8-bit)
                    _resultTemp.ConvertTo(outputMat, MatType.CV_8UC3);
                    return outputMat.SaveImage(
                        file.Path.AbsolutePath, new ImageEncodingParam(ImwriteFlags.PngCompression, 9));
            }
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