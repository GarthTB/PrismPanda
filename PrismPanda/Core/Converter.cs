using OpenCvSharp;
using Wacton.Unicolour;

namespace PrismPanda.Core;

public static class Converter
{
    public static Mat SplitGains(this Mat image, int colorSpaceId, double ch1Gain, double ch2Gain, double ch3Gain)
    {
        var result = image.Clone();
        for (var i = 0; i < image.Rows; i++)
        for (var j = 0; j < image.Cols; j++)
        {
            var pxValue = image.At<Vec3f>(i, j);
            var (x, y, z) = (pxValue.Item0, pxValue.Item1, pxValue.Item2);
            var (ch1, ch2, ch3) = new Unicolour(ColourSpace.Xyz, x, y, z) // now in XYZ color space
                .ConvertAndGain(colorSpaceId, ch1Gain, ch2Gain, ch3Gain) // convert to the desired color space and gains
                .Xyz.Triplet; // convert back to XYZ and get the components
            result.Set(i, j, new Vec3f((float)ch1, (float)ch2, (float)ch3));
        }

        return result;
    }

    private static Unicolour ConvertAndGain(
        this Unicolour colour, int colorSpaceId, double ch1Gain, double ch2Gain, double ch3Gain) =>
        colorSpaceId switch
        {
            0 => new Unicolour(
                ColourSpace.Hsl, colour.Hsl.H.Gain360(ch1Gain), colour.Hsl.S.Gain1(ch2Gain),
                colour.Hsl.L.Gain1(ch3Gain)),
            1 => new Unicolour(
                ColourSpace.Hsb, colour.Hsb.H.Gain360(ch1Gain), colour.Hsb.S.Gain1(ch2Gain),
                colour.Hsb.B.Gain1(ch3Gain)),
            2 => new Unicolour(
                ColourSpace.Hsi, colour.Hsi.H.Gain360(ch1Gain), colour.Hsi.S.Gain1(ch2Gain),
                colour.Hsi.I.Gain1(ch3Gain)),
            3 => new Unicolour(
                ColourSpace.Tsl, colour.Tsl.T.Gain360(ch1Gain), colour.Tsl.S.Gain1(ch2Gain),
                colour.Tsl.L.Gain1(ch3Gain)),
            4 => new Unicolour(
                ColourSpace.Lchab, colour.Lchab.L.Gain100(ch1Gain), colour.Lchab.C.Gain100(ch2Gain),
                colour.Lchab.H.Gain360(ch3Gain)),
            5 => new Unicolour(
                ColourSpace.Lchuv, colour.Lchuv.L.Gain100(ch1Gain), colour.Lchuv.C.Gain100(ch2Gain),
                colour.Lchuv.H.Gain360(ch3Gain)),
            6 => new Unicolour(
                ColourSpace.Hsluv, colour.Hsluv.H.Gain360(ch1Gain), colour.Hsluv.S.Gain1(ch2Gain),
                colour.Hsluv.L.Gain1(ch3Gain)),
            7 => new Unicolour(
                ColourSpace.Jzczhz, colour.Jzczhz.J.Gain1(ch1Gain), colour.Jzczhz.C.Gain1(ch2Gain),
                colour.Jzczhz.H.Gain360(ch3Gain)),
            8 => new Unicolour(
                ColourSpace.Oklch, colour.Oklch.L.Gain1(ch1Gain), colour.Oklch.C.Gain1(ch2Gain),
                colour.Oklch.H.Gain360(ch3Gain)),
            9 => new Unicolour(
                ColourSpace.Okhsv, colour.Okhsv.H.Gain360(ch1Gain), colour.Okhsv.S.Gain1(ch2Gain),
                colour.Okhsv.V.Gain1(ch3Gain)),
            10 => new Unicolour(
                ColourSpace.Okhsl, colour.Okhsl.H.Gain360(ch1Gain), colour.Okhsl.S.Gain1(ch2Gain),
                colour.Okhsl.L.Gain1(ch3Gain)),
            _ => new Unicolour(
                ColourSpace.Hct, colour.Hct.H.Gain360(ch1Gain), colour.Hct.C.Gain100(ch2Gain),
                colour.Hct.T.Gain100(ch3Gain))
        };

    private static double Gain1(this double value, double gain) =>
        gain > 0
            ? value + gain * (1 - value)
            : (1 + gain) * value;

    private static double Gain100(this double value, double gain) =>
        gain > 0
            ? value + gain * (100 - value)
            : (1 + gain) * value;

    private static double Gain360(this double value, double gain) =>
        gain > 0
            ? value + gain * (360 - value)
            : (1 + gain) * value;
}