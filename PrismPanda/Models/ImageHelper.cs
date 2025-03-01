using OpenCvSharp;
using Wacton.Unicolour;
using CS = Wacton.Unicolour.ColourSpace;

namespace PrismPanda.Models;

/// <summary> This class contains helper methods for working with images. </summary>
internal static class ImageHelper
{
    internal static Mat Saturate(this Mat originalMat, CS colourSpace, double chromaGain)
    {
        var mat = originalMat.Clone();
        if (chromaGain == 0) return mat;

        Unicolour GainChroma(Unicolour colour)
        {
            var (ch1, chroma, ch3) = colourSpace switch
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
            var factor = chromaGain > 0
                ? +Math.Pow(chromaGain, 2)
                : -Math.Pow(chromaGain, 2); // 边缘力度递增
            var newChroma = factor > 0
                ? chroma + factor * (1 - chroma)
                : chroma + factor * chroma; // -1 for min and 1 for max
            return new(colourSpace, ch1, newChroma, ch3);
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
        value->Item0 = (byte)newColour.Rgb.Byte255.B;
        value->Item1 = (byte)newColour.Rgb.Byte255.G;
        value->Item2 = (byte)newColour.Rgb.Byte255.R;
    });

    private static unsafe void Saturate65535(Mat mat, Func<Unicolour, Unicolour> GainChroma)
    => mat.ForEachAsVec3s((value, _) => // 不懂为什么没有vec3w
    {
        Unicolour colour = new(
            CS.Rgb,
            (value->Item2 + 32768) / 65535,
            (value->Item1 + 32768) / 65535,
            (value->Item0 + 32768) / 65535);
        var newColour = GainChroma(colour);
        value->Item0 = Convert.ToInt16(newColour.Rgb.B * 65535 - 32768);
        value->Item1 = Convert.ToInt16(newColour.Rgb.G * 65535 - 32768);
        value->Item2 = Convert.ToInt16(newColour.Rgb.R * 65535 - 32768);
    });

    private static unsafe void SaturateFloat(Mat mat, Func<Unicolour, Unicolour> GainChroma)
    => mat.ForEachAsVec3f((value, _) =>
    {
        var newColour = GainChroma(
            new(CS.Rgb, value->Item2, value->Item1, value->Item0));
        value->Item0 = Convert.ToSingle(newColour.Rgb.B);
        value->Item1 = Convert.ToSingle(newColour.Rgb.G);
        value->Item2 = Convert.ToSingle(newColour.Rgb.R);
    });

    private static unsafe void SaturateDouble(Mat mat, Func<Unicolour, Unicolour> GainChroma)
    => mat.ForEachAsVec3d((value, _) =>
    {
        var newColour = GainChroma(
            new(CS.Rgb, value->Item2, value->Item1, value->Item0));
        value->Item0 = newColour.Rgb.B;
        value->Item1 = newColour.Rgb.G;
        value->Item2 = newColour.Rgb.R;
    });
}
