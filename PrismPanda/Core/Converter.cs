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
                        .Xyz; // convert back to XYZ and get the components
                    value->Item0 = (float)result.X;
                    value->Item1 = (float)result.Y;
                    value->Item2 = (float)result.Z;
                });
        }
        return processedImage;
    }

    private static Unicolour ConvertAndGain(
        this Unicolour ori, int colorSpaceId, double ch1Gain, double ch2Gain, double ch3Gain) =>
        colorSpaceId switch
        {
            0 => Gain(ColourSpace.Hsl, ori.Hsl.Triplet, ch1Gain, ch2Gain, ch3Gain),
            1 => Gain(ColourSpace.Hsb, ori.Hsb.Triplet, ch1Gain, ch2Gain, ch3Gain),
            2 => Gain(ColourSpace.Hsi, ori.Hsi.Triplet, ch1Gain, ch2Gain, ch3Gain),
            3 => Gain(ColourSpace.Tsl, ori.Tsl.Triplet, ch1Gain, ch2Gain, ch3Gain),
            4 => Gain(ColourSpace.Lchab, ori.Lchab.Triplet, ch1Gain, ch2Gain, ch3Gain),
            5 => Gain(ColourSpace.Lchuv, ori.Lchuv.Triplet, ch1Gain, ch2Gain, ch3Gain),
            6 => Gain(ColourSpace.Hsluv, ori.Hsluv.Triplet, ch1Gain, ch2Gain, ch3Gain),
            7 => Gain(ColourSpace.Jzczhz, ori.Jzczhz.Triplet, ch1Gain, ch2Gain, ch3Gain),
            8 => Gain(ColourSpace.Oklch, ori.Oklch.Triplet, ch1Gain, ch2Gain, ch3Gain),
            9 => Gain(ColourSpace.Okhsv, ori.Okhsv.Triplet, ch1Gain, ch2Gain, ch3Gain),
            10 => Gain(ColourSpace.Okhsl, ori.Okhsl.Triplet, ch1Gain, ch2Gain, ch3Gain),
            _ => Gain(ColourSpace.Hct, ori.Hct.Triplet, ch1Gain, ch2Gain, ch3Gain)
        };

    private static Unicolour Gain(
        ColourSpace colorSpace, ColourTriplet triplet, double ch1Gain, double ch2Gain, double ch3Gain) =>
        new(colorSpace, ch1Gain * triplet.First, ch2Gain * triplet.Second, ch3Gain * triplet.Third);
}