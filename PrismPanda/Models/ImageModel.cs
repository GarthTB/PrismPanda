using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Windows.Media.Imaging;
using CS = Wacton.Unicolour.ColourSpace;

namespace PrismPanda.Models;

/// <summary> Image Processor </summary>
internal class ImageModel
{
    private readonly string _filePath;

    private readonly Mat _originalMat;

    private readonly Mat _previewMat;

    public ImageModel(string filePath)
    {
        _filePath = filePath;
        _originalMat = Cv2.ImRead(filePath, ImreadModes.Color | ImreadModes.AnyDepth);
        _previewMat = _originalMat.Clone();
        if (_previewMat.Height > 360 || _previewMat.Width > 360)
            Cv2.Resize(_previewMat, _previewMat, new Size(360, 360));
    }

    public BitmapSource GetPreview(CS colourSpace, double chromaGain)
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
        CS colourSpace,
        double chromaGain,
        string extension,
        ImageEncodingParam[] prms)
    {
        if (chromaGain == 0)
            throw new ArgumentException("No need to save image with chroma gain 0");
        var saturatedMat = _originalMat.Saturate(colourSpace, chromaGain);
        var savePath = FileHelper.DistinctSavePath(_filePath, extension);
        return Cv2.ImWrite(savePath, saturatedMat, prms);
    }
}
