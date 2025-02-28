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
        if (_previewMat.Height > 360 || _previewMat.Width > 360)
            Cv2.Resize(_previewMat, _previewMat, new Size(360, 360));
    }

    public BitmapSource GetPreview(ColourSpace colourSpace, double chromaGain)
    {
        var factor = chromaGain < 1
            ? 1 - Math.Pow(chromaGain - 1, 2)
            : 1 + Math.Pow(chromaGain - 1, 2); // 边缘力度递增
        var saturatedMat = chromaGain == 1
           ? _previewMat.Clone()
           : _previewMat.Depth() switch
           {
               MatType.CV_8U => Saturate255(colourSpace, factor),
               MatType.CV_16U => Saturate65535(colourSpace, factor),
               MatType.CV_32F => SaturateFloat(colourSpace, factor),
               MatType.CV_64F => SaturateDouble(colourSpace, factor),
               _ => throw new ArgumentException("Unsupported image depth")
           };
        var bitmap = saturatedMat.ToBitmap();
        return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
            bitmap.GetHbitmap(),
            IntPtr.Zero,
            System.Windows.Int32Rect.Empty,
            BitmapSizeOptions.FromEmptyOptions());
    }

    private static double GetChromaValue(Unicolour colour, ColourSpace colourSpace)
        => colourSpace switch
        {
            ColourSpace.Hsi => colour.Hsi.S,
            ColourSpace.Tsl => colour.Tsl.S,
            ColourSpace.Lchab => colour.Lchab.C,
            ColourSpace.Lchuv => colour.Lchuv.C,
            ColourSpace.Jzczhz => colour.Jzczhz.C,
            ColourSpace.Oklch => colour.Oklch.C,
            ColourSpace.Hct => colour.Hct.C,
            _ => throw new ArgumentException("Unsupported colour space")
        };

    private static unsafe Unicolour GetNewColour(ColourSpace colourSpace, Unicolour colour, double factor)
    {
        var chroma = GetChromaValue(colour, colourSpace);
        var newChroma = factor > 1
            ? chroma + (factor - 1) * (1 - chroma)
            : chroma * factor; // 2 for max and 0 for min
        return colourSpace switch
        {
            ColourSpace.Hsi => new(ColourSpace.Hsi, colour.Hsi.H, newChroma, colour.Hsi.I),
            ColourSpace.Tsl => new(ColourSpace.Tsl, colour.Tsl.T, newChroma, colour.Tsl.L),
            ColourSpace.Lchab => new(ColourSpace.Lchab, colour.Lchab.L, newChroma, colour.Lchab.H),
            ColourSpace.Lchuv => new(ColourSpace.Lchuv, colour.Lchuv.L, newChroma, colour.Lchuv.H),
            ColourSpace.Jzczhz => new(ColourSpace.Jzczhz, colour.Jzczhz.J, newChroma, colour.Jzczhz.H),
            ColourSpace.Oklch => new(ColourSpace.Oklch, colour.Oklch.L, newChroma, colour.Oklch.H),
            ColourSpace.Hct => new(ColourSpace.Hct, colour.Hct.H, newChroma, colour.Hct.T),
            _ => throw new ArgumentException("Unsupported colour space")
        };
    }

    private unsafe Mat Saturate255(ColourSpace colourSpace, double factor)
    {
        var mat = _previewMat.Clone();
        mat.ForEachAsVec3b((value, _) =>
        {
            Unicolour colour = new(
                ColourSpace.Rgb255,
                value->Item2,
                value->Item1,
                value->Item0);
            var newColour = GetNewColour(colourSpace, colour, factor);
            value->Item2 = (byte)newColour.Rgb.Byte255.B;
            value->Item1 = (byte)newColour.Rgb.Byte255.G;
            value->Item0 = (byte)newColour.Rgb.Byte255.R;
        });
        return mat;
    }

    private unsafe Mat Saturate65535(ColourSpace colourSpace, double factor)
    {
        var mat = _previewMat.Clone();
        mat.ForEachAsVec3s((value, _) => // 不懂为什么没有vec3w
        {
            Unicolour colour = new(
                ColourSpace.Rgb,
                (value->Item2 + 32768) / 65535,
                (value->Item1 + 32768) / 65535,
                (value->Item0 + 32768) / 65535);
            var newColour = GetNewColour(colourSpace, colour, factor);
            value->Item2 = Convert.ToInt16(newColour.Rgb.B * 65535 - 32768);
            value->Item1 = Convert.ToInt16(newColour.Rgb.G * 65535 - 32768);
            value->Item0 = Convert.ToInt16(newColour.Rgb.R * 65535 - 32768);
        });
        return mat;
    }

    private unsafe Mat SaturateFloat(ColourSpace colourSpace, double factor)
    {
        var mat = _previewMat.Clone();
        mat.ForEachAsVec3f((value, _) =>
        {
            Unicolour colour = new(
                ColourSpace.Rgb,
                value->Item2,
                value->Item1,
                value->Item0);
            var newColour = GetNewColour(colourSpace, colour, factor);
            value->Item2 = Convert.ToSingle(newColour.Rgb.B);
            value->Item1 = Convert.ToSingle(newColour.Rgb.G);
            value->Item0 = Convert.ToSingle(newColour.Rgb.R);
        });
        return mat;
    }

    private unsafe Mat SaturateDouble(ColourSpace colourSpace, double factor)
    {
        var mat = _previewMat.Clone();
        mat.ForEachAsVec3d((value, _) =>
        {
            Unicolour colour = new(
                ColourSpace.Rgb,
                value->Item2,
                value->Item1,
                value->Item0);
            var newColour = GetNewColour(colourSpace, colour, factor);
            value->Item2 = newColour.Rgb.B;
            value->Item1 = newColour.Rgb.G;
            value->Item0 = newColour.Rgb.R;
        });
        return mat;
    }
}
