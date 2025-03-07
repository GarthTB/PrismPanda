using OpenCvSharp;
using Wacton.Unicolour;
using CS = Wacton.Unicolour.ColourSpace;

namespace PrismPanda.Models;

/// <summary> This class contains helper methods for working with images. </summary>
internal static class ImageHelper
{
    #region Saturate

    internal static Mat Saturate(this Mat originalMat, CS colourSpace, double chromaGain)
    {
        var mat = originalMat.Clone();
        if (chromaGain == 0) return mat;

        Unicolour GainChroma(Unicolour colour)
        {
            var (ch1, chroma, ch3) = colourSpace switch
            {
                CS.Hsi => (colour.Hsi.H, colour.Hsi.S, colour.Hsi.I),
                CS.Tsl => (colour.Tsl.T, colour.Tsl.S, colour.Tsl.L),
                CS.Lchab => (colour.Lchab.L, colour.Lchab.C / 100, colour.Lchab.H),
                CS.Lchuv => (colour.Lchuv.L, colour.Lchuv.C / 100, colour.Lchuv.H),
                CS.Jzczhz => (colour.Jzczhz.J, colour.Jzczhz.C, colour.Jzczhz.H),
                CS.Oklch => (colour.Oklch.L, colour.Oklch.C, colour.Oklch.H),
                CS.Hct => (colour.Hct.H, colour.Hct.C / 100, colour.Hct.T),
                _ => throw new ArgumentException("Unsupported colour space")
            };
            var optimizedGain = Math.Pow(chromaGain, 2); // 边缘力度逐渐增大
            var newChroma = chromaGain <= 0
                ? (1 - optimizedGain) * chroma
                : chroma + (Math.Pow(chroma, 1 - optimizedGain) - chroma) * (1 - Math.Pow(colour.Hsl.S, 2));
            return colourSpace switch
            {
                CS.Hsi => new(CS.Hsi, ch1, newChroma, ch3),
                CS.Tsl => new(CS.Tsl, ch1, newChroma, ch3),
                CS.Lchab => new(CS.Lchab, ch1, newChroma * 100, ch3),
                CS.Lchuv => new(CS.Lchuv, ch1, newChroma * 100, ch3),
                CS.Jzczhz => new(CS.Jzczhz, ch1, newChroma, ch3),
                CS.Oklch => new(CS.Oklch, ch1, newChroma, ch3),
                CS.Hct => new(CS.Hct, ch1, newChroma * 100, ch3),
                _ => throw new ArgumentException("Unsupported colour space")
            };
        }

        switch (mat.Depth())
        {
            case MatType.CV_8U:
                Saturate255(mat, GainChroma);
                return mat;
            case MatType.CV_16U:
                Saturate65535(mat, GainChroma);
                return mat;
            case MatType.CV_32F:
                SaturateFloat(mat, GainChroma);
                return mat;
            case MatType.CV_64F:
                SaturateDouble(mat, GainChroma);
                return mat;
            default:
                throw new ArgumentException("Unsupported image depth");
        }
    }

    private static unsafe void Saturate255(Mat mat, Func<Unicolour, Unicolour> GainChroma)
    => mat.ForEachAsVec3b((value, _) =>
    {
        var newColour = GainChroma(
            new(CS.Rgb255, value->Item2, value->Item1, value->Item0));
        value->Item0 = (byte)newColour.Rgb.Byte255.ConstrainedB;
        value->Item1 = (byte)newColour.Rgb.Byte255.ConstrainedG;
        value->Item2 = (byte)newColour.Rgb.Byte255.ConstrainedR;
    });

    private static unsafe void Saturate65535(Mat mat, Func<Unicolour, Unicolour> GainChroma)
    => mat.ForEachAsVec3s((value, _) => // 不懂为什么没有vec3w
    {
        Unicolour colour = new(
            CS.Rgb,
            (ushort)value->Item2 / 65535.0,
            (ushort)value->Item1 / 65535.0,
            (ushort)value->Item0 / 65535.0);
        var newColour = GainChroma(colour);
        value->Item0 = (short)Convert.ToUInt16(newColour.Rgb.ConstrainedB * 65535);
        value->Item1 = (short)Convert.ToUInt16(newColour.Rgb.ConstrainedG * 65535);
        value->Item2 = (short)Convert.ToUInt16(newColour.Rgb.ConstrainedR * 65535);
    });

    private static unsafe void SaturateFloat(Mat mat, Func<Unicolour, Unicolour> GainChroma)
    => mat.ForEachAsVec3f((value, _) =>
    {
        var newColour = GainChroma(
            new(CS.Rgb, value->Item2, value->Item1, value->Item0));
        value->Item0 = Convert.ToSingle(newColour.Rgb.ConstrainedB);
        value->Item1 = Convert.ToSingle(newColour.Rgb.ConstrainedG);
        value->Item2 = Convert.ToSingle(newColour.Rgb.ConstrainedR);
    });

    private static unsafe void SaturateDouble(Mat mat, Func<Unicolour, Unicolour> GainChroma)
    => mat.ForEachAsVec3d((value, _) =>
    {
        var newColour = GainChroma(
            new(CS.Rgb, value->Item2, value->Item1, value->Item0));
        value->Item0 = newColour.Rgb.ConstrainedB;
        value->Item1 = newColour.Rgb.ConstrainedG;
        value->Item2 = newColour.Rgb.ConstrainedR;
    });

    #endregion

    #region Convert Bit Depth

    internal static Mat To8Bit(this Mat originalMat)
    {
        if (originalMat.Depth() == MatType.CV_8U)
            return originalMat;
        Mat mat = new(originalMat.Size(), MatType.CV_8U);
        switch (originalMat.Depth())
        {
            case MatType.CV_16U:
                originalMat.ConvertTo(mat, MatType.CV_8U, 1.0 / 257.0);
                return mat;
            case MatType.CV_32F:
            case MatType.CV_64F:
                originalMat.ConvertTo(mat, MatType.CV_8U, 255.0);
                return mat;
            default:
                throw new ArgumentException("Unsupported image depth");
        }
    }

    #endregion
}
