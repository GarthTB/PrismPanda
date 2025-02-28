using OpenCvSharp;
using System.Windows.Media.Imaging;

namespace PrismPanda.Models;

/// <summary> Image Processor </summary>
internal class ImageModel
{
    private readonly Mat _originalMat;

    private readonly Mat _previewMat;

    public ImageModel(string filePath)
    {
        _originalMat = Cv2.ImRead(filePath);
        _previewMat = _originalMat.Clone().Resize(new Size(360, 360));
    }

    public BitmapSource GetPreveiw(double chromaGain)
    {
        var factor = chromaGain > 1 ? Math.Pow(10, chromaGain - 1) : chromaGain;
    }
}
