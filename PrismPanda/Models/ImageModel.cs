using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Windows.Media.Imaging;

namespace PrismPanda.Models;

/// <summary> Image Processor </summary>
internal class ImageModel
{
    private readonly Mat _originalMat;

    private readonly Mat _previewMat;

    public ImageModel(string filePath)
    {
        _originalMat = Cv2.ImRead(filePath, ImreadModes.Color | ImreadModes.AnyDepth);
        _previewMat = _originalMat.Clone();
        if (_previewMat.Height > 360 || _previewMat.Width > 360)
            Cv2.Resize(_previewMat, _previewMat, new Size(360, 360));
    }

    public BitmapSource GetPreveiw(double chromaGain)
    {
        var factor = chromaGain < 1
            ? 1 - Math.Pow(chromaGain - 1, 2)
            : 1 + Math.Pow(chromaGain - 1, 2);
        var previewMat = Saturate(factor);
        var bitmap = BitmapConverter.ToBitmap(previewMat);
        return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
            bitmap.GetHbitmap(),
            IntPtr.Zero,
            System.Windows.Int32Rect.Empty,
            BitmapSizeOptions.FromEmptyOptions());
    }

    private Mat Saturate(double factor) => throw new NotImplementedException();
}
