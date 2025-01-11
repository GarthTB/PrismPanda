using OpenCvSharp;
using Wacton.Unicolour;

namespace PrismPanda.Core;

public static class Converter
{
    public static Mat SplitGains(this Mat image, int colorSpaceId, double ch1Gain, double ch2Gain, double ch3Gain)
    {
        var processedImage = image.Clone();
        unsafe
        {
            processedImage.ForEachAsVec3f(
                (value, _) =>
                {
                    var ch1 = value->Item0;
                    var ch2 = value->Item1;
                    var ch3 = value->Item2;
                    var result = new Unicolour(ColourSpace.Xyz, ch1, ch2, ch3) // now in XYZ color space
                        .ConvertAndGain(colorSpaceId, ch1Gain, ch2Gain, ch3Gain) // convert color space and gains
                        .Xyz.Triplet; // convert back to XYZ and get the components
                    value->Item0 = (float)result.First;
                    value->Item1 = (float)result.Second;
                    value->Item2 = (float)result.Third;
                });
        }
        return processedImage;
    }

    private static Unicolour ConvertAndGain(
        this Unicolour ori, int colorSpaceId, double ch1Gain, double ch2Gain, double ch3Gain)
    {
        return colorSpaceId switch
        {
            0 => Gain(ColourSpace.Hsl, ori.Hsl.Triplet),
            1 => Gain(ColourSpace.Hsb, ori.Hsb.Triplet),
            2 => Gain(ColourSpace.Hsi, ori.Hsi.Triplet),
            3 => Gain(ColourSpace.Tsl, ori.Tsl.Triplet),
            4 => Gain(ColourSpace.Lchab, ori.Lchab.Triplet),
            5 => Gain(ColourSpace.Lchuv, ori.Lchuv.Triplet),
            6 => Gain(ColourSpace.Hsluv, ori.Hsluv.Triplet),
            7 => Gain(ColourSpace.Jzczhz, ori.Jzczhz.Triplet),
            8 => Gain(ColourSpace.Oklch, ori.Oklch.Triplet),
            9 => Gain(ColourSpace.Okhsv, ori.Okhsv.Triplet),
            10 => Gain(ColourSpace.Okhsl, ori.Okhsl.Triplet),
            _ => Gain(ColourSpace.Hct, ori.Hct.Triplet)
        };

        Unicolour Gain(ColourSpace colorSpace, ColourTriplet triplet) =>
            new(colorSpace, ch1Gain * triplet.First, ch2Gain * triplet.Second, ch3Gain * triplet.Third);
    }
}