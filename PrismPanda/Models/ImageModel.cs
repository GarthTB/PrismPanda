using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Windows.Media.Imaging;
using Wacton.Unicolour;
using CS = Wacton.Unicolour.ColourSpace;

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

    public BitmapSource GetPreview(CS colourSpace, double chromaGain)
    {
        var saturatedMat = chromaGain == 1
           ? _previewMat.Clone()
           : _previewMat.Depth() switch
           {
               MatType.CV_8U => Saturate255(colourSpace, chromaGain),
               MatType.CV_16U => Saturate65535(colourSpace, chromaGain),
               MatType.CV_32F => SaturateFloat(colourSpace, chromaGain),
               MatType.CV_64F => SaturateDouble(colourSpace, chromaGain),
               _ => throw new ArgumentException("Unsupported image depth")
           };
        var bitmap = saturatedMat.ToBitmap();
        return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
            bitmap.GetHbitmap(),
            IntPtr.Zero,
            System.Windows.Int32Rect.Empty,
            BitmapSizeOptions.FromEmptyOptions());
    }

    public void ProcAndSave(CS colourSpace, double chromaGain)
    {

    }

    #region Saturate Methods

    private static (double, double, double) GetComponents(CS colourSpace, Unicolour colour)
        => colourSpace switch
        {
            CS.Hsi => colour.Hsi.Triplet.Tuple,
            CS.Tsl => colour.Tsl.Triplet.Tuple,
            CS.Lchab => colour.Lchab.Triplet.Tuple,
            CS.Lchuv => colour.Lchuv.Triplet.Tuple,
            CS.Jzczhz => colour.Jzczhz.Triplet.Tuple,
            CS.Oklch => colour.Oklch.Triplet.Tuple,
            CS.Hct => colour.Hct.Triplet.Tuple,
            _ => throw new ArgumentException("Unsupported colour space")
        };

    private static Unicolour GetNewColour(CS colourSpace, Unicolour colour, double chromaGain)
    {
        var (ch1, chroma, ch3) = GetComponents(colourSpace, colour);
        var factor = chromaGain > 1
            ? 1 + Math.Pow(chromaGain - 1, 2)
            : 1 - Math.Pow(chromaGain - 1, 2); // 边缘力度递增
        var newChroma = factor > 1
            ? chroma + (factor - 1) * (1 - chroma)
            : factor * chroma; // 0 for min and 2 for max
        return new(colourSpace, ch1, newChroma, ch3);
    }

    private unsafe Mat Saturate255(CS colourSpace, double chromaGain)
    {
        var mat = _previewMat.Clone();
        mat.ForEachAsVec3b((value, _) =>
        {
            Unicolour colour = new(
                CS.Rgb255,
                value->Item2,
                value->Item1,
                value->Item0);
            var newColour = GetNewColour(colourSpace, colour, chromaGain);
            value->Item2 = (byte)newColour.Rgb.Byte255.B;
            value->Item1 = (byte)newColour.Rgb.Byte255.G;
            value->Item0 = (byte)newColour.Rgb.Byte255.R;
        });
        return mat;
    }

    private unsafe Mat Saturate65535(CS colourSpace, double chromaGain)
    {
        var mat = _previewMat.Clone();
        mat.ForEachAsVec3s((value, _) => // 不懂为什么没有vec3w
        {
            Unicolour colour = new(
                CS.Rgb,
                (value->Item2 + 32768) / 65535,
                (value->Item1 + 32768) / 65535,
                (value->Item0 + 32768) / 65535);
            var newColour = GetNewColour(colourSpace, colour, chromaGain);
            value->Item2 = Convert.ToInt16(newColour.Rgb.B * 65535 - 32768);
            value->Item1 = Convert.ToInt16(newColour.Rgb.G * 65535 - 32768);
            value->Item0 = Convert.ToInt16(newColour.Rgb.R * 65535 - 32768);
        });
        return mat;
    }

    private unsafe Mat SaturateFloat(CS colourSpace, double chromaGain)
    {
        var mat = _previewMat.Clone();
        mat.ForEachAsVec3f((value, _) =>
        {
            Unicolour colour = new(
                CS.Rgb,
                value->Item2,
                value->Item1,
                value->Item0);
            var newColour = GetNewColour(colourSpace, colour, chromaGain);
            value->Item2 = Convert.ToSingle(newColour.Rgb.B);
            value->Item1 = Convert.ToSingle(newColour.Rgb.G);
            value->Item0 = Convert.ToSingle(newColour.Rgb.R);
        });
        return mat;
    }

    private unsafe Mat SaturateDouble(CS colourSpace, double chromaGain)
    {
        var mat = _previewMat.Clone();
        mat.ForEachAsVec3d((value, _) =>
        {
            Unicolour colour = new(
                CS.Rgb,
                value->Item2,
                value->Item1,
                value->Item0);
            var newColour = GetNewColour(colourSpace, colour, chromaGain);
            value->Item2 = newColour.Rgb.B;
            value->Item1 = newColour.Rgb.G;
            value->Item0 = newColour.Rgb.R;
        });
        return mat;
    }

    #endregion
}
