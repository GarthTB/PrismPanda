using System.Threading.Tasks;
using OpenCvSharp;
using Wacton.Unicolour;

namespace PrismPanda.Core;

public static class Converter
{
    public static Mat SplitGains(this Mat image, int colorSpaceId, double ch1Gain, double ch2Gain, double ch3Gain)
    {
        var result = image.Clone();
        Parallel.For(
            0, image.Rows, i =>
            {
                for (var j = 0; j < image.Cols; j++)
                {
                    var pxValue = image.At<Vec3f>(i, j);
                    var (x, y, z) = (pxValue.Item0, pxValue.Item1, pxValue.Item2);
                    var (ch1, ch2, ch3) = new Unicolour(ColourSpace.Xyz, x, y, z) // now in XYZ color space
                        .ConvertAndGain(colorSpaceId, ch1Gain, ch2Gain, ch3Gain) // convert color space and gains
                        .Xyz.Triplet; // convert back to XYZ and get the components
                    result.Set(i, j, new Vec3f((float)ch1, (float)ch2, (float)ch3));
                }
            });
        return result;
    }

    private static Unicolour ConvertAndGain(
        this Unicolour colour, int colorSpaceId, double ch1Gain, double ch2Gain, double ch3Gain) =>
        colorSpaceId switch
        {
            0 => new Unicolour(
                ColourSpace.Hsl, colour.Hsl.H.Gain(ch1Gain, 360), colour.Hsl.S.Gain(ch2Gain, 1),
                colour.Hsl.L.Gain(ch3Gain, 1)),
            1 => new Unicolour(
                ColourSpace.Hsb, colour.Hsb.H.Gain(ch1Gain, 360), colour.Hsb.S.Gain(ch2Gain, 1),
                colour.Hsb.B.Gain(ch3Gain, 1)),
            2 => new Unicolour(
                ColourSpace.Hsi, colour.Hsi.H.Gain(ch1Gain, 360), colour.Hsi.S.Gain(ch2Gain, 1),
                colour.Hsi.I.Gain(ch3Gain, 1)),
            3 => new Unicolour(
                ColourSpace.Tsl, colour.Tsl.T.Gain(ch1Gain, 360), colour.Tsl.S.Gain(ch2Gain, 1),
                colour.Tsl.L.Gain(ch3Gain, 1)),
            4 => new Unicolour(
                ColourSpace.Lchab, colour.Lchab.L.Gain(ch1Gain, 100), colour.Lchab.C.Gain(ch2Gain, 100),
                colour.Lchab.H.Gain(ch3Gain, 360)),
            5 => new Unicolour(
                ColourSpace.Lchuv, colour.Lchuv.L.Gain(ch1Gain, 100), colour.Lchuv.C.Gain(ch2Gain, 150),
                colour.Lchuv.H.Gain(ch3Gain, 360)),
            6 => new Unicolour(
                ColourSpace.Hsluv, colour.Hsluv.H.Gain(ch1Gain, 360), colour.Hsluv.S.Gain(ch2Gain, 1),
                colour.Hsluv.L.Gain(ch3Gain, 1)),
            7 => new Unicolour(
                ColourSpace.Jzczhz, colour.Jzczhz.J.Gain(ch1Gain, 1), colour.Jzczhz.C.Gain(ch2Gain, 1),
                colour.Jzczhz.H.Gain(ch3Gain, 360)),
            8 => new Unicolour(
                ColourSpace.Oklch, colour.Oklch.L.Gain(ch1Gain, 1), colour.Oklch.C.Gain(ch2Gain, 0.37),
                colour.Oklch.H.Gain(ch3Gain, 360)),
            9 => new Unicolour(
                ColourSpace.Okhsv, colour.Okhsv.H.Gain(ch1Gain, 360), colour.Okhsv.S.Gain(ch2Gain, 1),
                colour.Okhsv.V.Gain(ch3Gain, 1)),
            10 => new Unicolour(
                ColourSpace.Okhsl, colour.Okhsl.H.Gain(ch1Gain, 360), colour.Okhsl.S.Gain(ch2Gain, 1),
                colour.Okhsl.L.Gain(ch3Gain, 1)),
            _ => new Unicolour(
                ColourSpace.Hct, colour.Hct.H.Gain(ch1Gain, 360), colour.Hct.C.Gain(ch2Gain, 100),
                colour.Hct.T.Gain(ch3Gain, 100))
        };

    private static double Gain(this double value, double gain, double max) =>
        gain > 0
            ? value + gain * (max - value)
            : (1 + gain) * value;
}