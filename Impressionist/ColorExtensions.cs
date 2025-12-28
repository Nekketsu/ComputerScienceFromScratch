using SixLabors.ImageSharp;

namespace Impressionist;

public static class ColorExtensions
{
    extension(Color color)
    {
        public string SvgRgbString
        {
            get
            {
                var rgb = color.ToPixel<SixLabors.ImageSharp.PixelFormats.Rgba32>();
                return $"rgb({rgb.R},{rgb.G},{rgb.B})";
            }
        }

        public static Color Random()
        {
            var maxValue = byte.MaxValue + 1;

            var newColor = Color.FromRgb(
                (byte)System.Random.Shared.Next(maxValue),
                (byte)System.Random.Shared.Next(maxValue),
                (byte)System.Random.Shared.Next(maxValue));

            return newColor;
        }
    }
}
