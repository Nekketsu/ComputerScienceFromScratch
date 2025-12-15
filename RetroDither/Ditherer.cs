using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace RetroDither;

public class Ditherer
{
    private const int Threshhold = 127;

    private record PatternPart(int Dc, int Dr, int Numerator, int Denominator);

    private PatternPart[] atkinson =
    [
        new(1, 0, 1, 8),
        new(2, 0, 1, 8),
        new(-1, 1, 1, 8),
        new(0, 1, 1, 8),
        new(1, 1, 1, 8),
        new(0, 2, 1, 8),
    ];

    public byte[] Dither(Image<L8> image)
    {
        void Diffuse(int c, int r, int error, PatternPart[] pattern, PixelAccessor<L8> accessor)
        {
            foreach (var part in pattern)
            {
                var col = c + part.Dc;
                var row = r + part.Dr;
                if (col < 0 || col >= accessor.Width || row >= accessor.Height)
                {
                    continue;
                }
                var currentPixel = accessor.GetRowSpan(row)[col].PackedValue;
                var errorPart = error * part.Numerator / part.Denominator;
                var colorValue = (byte)(currentPixel + errorPart);
                accessor.GetRowSpan(row)[col].PackedValue = colorValue;
            }
        }

        var bytes = new byte[image.Width * image.Height];
        image.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (var x = 0; x < accessor.Width; x++)
                {
                    var oldPixel = row[x].PackedValue;
                    var newPixel = oldPixel > Threshhold ? (byte)255 : (byte)0;
                    bytes[y * accessor.Width + x] = newPixel;
                    var difference = oldPixel - newPixel;
                    Diffuse(x, y, difference, atkinson, accessor);
                }
            }
        });

        return bytes;
    }

    public Image<L8> Prepare(string fileName)
    {
        var image = Image.Load(fileName);
        Resize(image, MacPaint.MaxWidth, MacPaint.MaxHeight);
        var grayImage = image.CloneAs<L8>();
        return grayImage;
    }

    private void Resize(Image image, int width, int height)
    {
        if (image.Width > width || image.Height > height)
        {
            var desiredRatio = (double)width / height;
            var ratio = (double)image.Width / image.Height;

            var (newWidth, newHeight) = (ratio >= desiredRatio)
                ? (width, (int)(image.Height * ((double)width / image.Width)))
                : ((int)(image.Width * (double)(height / image.Height)), height);

            image.Mutate(x => x.Resize(newWidth, newHeight, KnownResamplers.Lanczos3));
        }
    }
}
