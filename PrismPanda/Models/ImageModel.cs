using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Windows.Media.Imaging;
using Wacton.Unicolour;

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
        if (_previewMat.Height * _previewMat.Width <= 100000) return;
        var scale = Math.Sqrt(100000.0 / _previewMat.Height / _previewMat.Width);
        Cv2.Resize(_previewMat, _previewMat, new Size(), scale, scale);
    }

    public BitmapSource GetPreview(ColourSpace colourSpace, double chromaGain)
    {
        var saturatedMat = _previewMat.Saturate(colourSpace, chromaGain);
        var bitmap = saturatedMat.ToBitmap();
        return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
            bitmap.GetHbitmap(),
            IntPtr.Zero,
            System.Windows.Int32Rect.Empty,
            BitmapSizeOptions.FromEmptyOptions());
    }

    public bool ProcAndSave(
        ColourSpace colourSpace,
        double chromaGain,
        string savePath,
        ImageEncodingParam[] prms)
    {
        if (chromaGain == 0)
            throw new ArgumentException("No need to save image with chroma gain 0");
        var saturatedMat = _originalMat.Saturate(colourSpace, chromaGain);
        return Cv2.ImWrite(savePath, saturatedMat, prms);
    }
}
