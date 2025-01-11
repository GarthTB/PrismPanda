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
        this Unicolour ori, int colorSpaceId, double ch1Gain, double ch2Gain, double ch3Gain)
    {
        return colorSpaceId switch
        {
            0 => new Unicolour(ColourSpace.Hsl, Gain1(ori.Hsl.H), Gain2(ori.Hsl.S), Gain3(ori.Hsl.L)),
            1 => new Unicolour(ColourSpace.Hsb, Gain1(ori.Hsb.H), Gain2(ori.Hsb.S), Gain3(ori.Hsb.B)),
            2 => new Unicolour(ColourSpace.Hsi, Gain1(ori.Hsi.H), Gain2(ori.Hsi.S), Gain3(ori.Hsi.I)),
            3 => new Unicolour(ColourSpace.Tsl, Gain1(ori.Tsl.T), Gain2(ori.Tsl.S), Gain3(ori.Tsl.L)),
            4 => new Unicolour(ColourSpace.Lchab, Gain1(ori.Lchab.L), Gain2(ori.Lchab.C), Gain3(ori.Lchab.H)),
            5 => new Unicolour(ColourSpace.Lchuv, Gain1(ori.Lchuv.L), Gain2(ori.Lchuv.C), Gain3(ori.Lchuv.H)),
            6 => new Unicolour(ColourSpace.Hsluv, Gain1(ori.Hsluv.H), Gain2(ori.Hsluv.S), Gain3(ori.Hsluv.L)),
            7 => new Unicolour(ColourSpace.Jzczhz, Gain1(ori.Jzczhz.J), Gain2(ori.Jzczhz.C), Gain3(ori.Jzczhz.H)),
            8 => new Unicolour(ColourSpace.Oklch, Gain1(ori.Oklch.L), Gain2(ori.Oklch.C), Gain3(ori.Oklch.H)),
            9 => new Unicolour(ColourSpace.Okhsv, Gain1(ori.Okhsv.H), Gain2(ori.Okhsv.S), Gain3(ori.Okhsv.V)),
            10 => new Unicolour(ColourSpace.Okhsl, Gain1(ori.Okhsl.H), Gain2(ori.Okhsl.S), Gain3(ori.Okhsl.L)),
            _ => new Unicolour(ColourSpace.Hct, Gain1(ori.Hct.H), Gain2(ori.Hct.C), Gain3(ori.Hct.T))
        };
        double Gain1(double x) => (1 + ch1Gain) * x;
        double Gain2(double x) => (1 + ch2Gain) * x;
        double Gain3(double x) => (1 + ch3Gain) * x;
    }
}