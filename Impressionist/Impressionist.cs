using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Data;
using System.Diagnostics;

namespace Impressionist;

public class Impressionist
{
    private string outputFile;
    private ColorMethod method;
    private ShapeType shapeType;
    private int trials;
    private int length;
    private bool vector;
    private int animationLength;

    private const int MaxHeight = 480;
    private List<(Point[] coordinates, Color color)> shapes = [];
    private Image<Rgba32> original;
    private Image<Rgba32> glass;
    private double bestDifference;

    public Impressionist(string fileName, string outputFile, int trials, ColorMethod method, ShapeType shapeType, int length, bool vector, int animationLength)
    {
        this.outputFile = outputFile;
        this.trials = trials;
        this.method = method;
        this.shapeType = shapeType;
        this.length = length;
        this.vector = vector;
        this.animationLength = animationLength;

        original = LoadImage(fileName);
        var averageColor = GetAverageColor(original);
        glass = new Image<Rgba32>(original.Width, original.Height, averageColor);
        bestDifference = Difference(glass);
    }

    public void Create()
    {
        var lastPercent = 0;
        var time = Stopwatch.StartNew();
        for (var test = 0; test < trials; test++)
        {
            Trial();
            var percent = test * 100 / trials;
            if (percent > lastPercent)
            {
                lastPercent = percent;
                Console.WriteLine($"{percent}% Done, Best Difference {bestDifference}");
            }
        }
        time.Stop();
        Console.WriteLine($"{(int)(time.Elapsed.TotalSeconds)} seconds elapsed. {shapes.Count} shapes created.");
        CreateOutput(outputFile, length, vector, animationLength);
    }

    private static Image<Rgba32> LoadImage(string fileName)
    {
        var image = Image.Load<Rgba32>(fileName);
        var aspectRatio = image.Width / (float)image.Height;
        var (newWidth, newHeight) = ((int)(MaxHeight * aspectRatio), MaxHeight);
        image.Mutate(x => x.Resize(newWidth, newHeight, KnownResamplers.Lanczos3));

        return image;
    }

    private static Color GetAverageColor(Image<Rgba32> image)
    {
        var r = 0L;
        var g = 0L;
        var b = 0L;
        var pixelCount = 0L;

        image.ProcessPixelRows(rows =>
        {
            for (var y = 0; y < rows.Height; y++)
            {
                var row = rows.GetRowSpan(y);
                for (var x = 0; x < row.Length; x++)
                {
                    var color = row[x];
                    r += color.R;
                    g += color.G;
                    b += color.B;
                    pixelCount++;
                }
            }
        });

        var mean = new Rgba32(
            (byte)(r / pixelCount),
            (byte)(g / pixelCount),
            (byte)(b / pixelCount),
            byte.MaxValue);

        return mean;
    }

    private Color GetMostCommonColor(Image<Rgba32> image)
    {
        var colorCounts = new Dictionary<Color, int>();

        image.ProcessPixelRows(rows =>
        {
            for (var y = 0; y < rows.Height; y++)
            {
                var row = rows.GetRowSpan(y);
                for (var x = 0; x < row.Length; x++)
                {
                    var color = row[x];

                    var count = colorCounts.GetValueOrDefault(color);
                    colorCounts[color] = count + 1;
                }
            }
        });

        var mostCommonColor = colorCounts.MaxBy(kv => kv.Value).Key;

        return mostCommonColor;
    }

    private double Difference(Image<Rgba32> other)
    {
        var sumR = 0L;
        var sumG = 0L;
        var sumB = 0L;
        var pixelCount = (long)original.Width * original.Height;

        original.ProcessPixelRows(other, (originalRows, otherRows) =>
        {
            for (var y = 0; y < originalRows.Height; y++)
            {
                var originalRow = originalRows.GetRowSpan(y);
                var otherRow = otherRows.GetRowSpan(y);

                for (var x = 0; x < originalRow.Length; x++)
                {
                    var originalColor = originalRow[x];
                    var otherColor = otherRow[x];

                    sumR += Math.Abs(originalColor.R - otherColor.R);
                    sumG += Math.Abs(originalColor.G - otherColor.G);
                    sumB += Math.Abs(originalColor.B - otherColor.B);
                }
            }
        });

        var mean = (double)(sumR + sumG + sumB) / (pixelCount * 3 * byte.MaxValue);

        return mean;
    }

    private Point[] RandomCoorindates()
    {
        var numCoordinates = shapeType switch
        {
            ShapeType.Triangle => 3,
            ShapeType.Quadrilateral => 4,
            _ => 2
        };

        var coordinates = new Point[numCoordinates];
        for (var d = 0; d < numCoordinates; d++)
        {
            coordinates[d].X = Random.Shared.Next(0, original.Width);
            coordinates[d].Y = Random.Shared.Next(0, original.Height);
        }

        return coordinates;
    }

    private Rectangle BoundingBox(Point[] coordinates)
    {
        var x1 = coordinates.Select(c => c.X).Min();
        var y1 = coordinates.Select(c => c.Y).Min();
        var x2 = coordinates.Select(c => c.X).Max();
        var y2 = coordinates.Select(c => c.Y).Max();

        var boundingBox = new Rectangle(x1, y1, Math.Max(1, x2 - x1), Math.Max(1, y2 - y1));

        return boundingBox;
    }

    private void Trial()
    {
        Point[] coordinates;
        using var region = RandomCrop(out coordinates);

        var color = method switch
        {
            ColorMethod.Average => GetAverageColor(region),
            ColorMethod.Common => GetMostCommonColor(region),
            _ => Color.Random()
        };

        bool Experiment()
        {
            var newImage = glass.Clone();
            if (shapeType == ShapeType.Ellipse)
            {
                var boundingBox = BoundingBox(coordinates);
                var ellipse = new EllipsePolygon(boundingBox.X, boundingBox.Y, boundingBox.Width, boundingBox.Height);
                newImage.Mutate(x => x.Fill(color, ellipse));
                newImage.Mutate(x => x.Draw(color, 1, ellipse));
            }
            else
            {
                var points = coordinates.Select(c => new PointF(c.X, c.Y)).ToArray();
                newImage.Mutate(x => x.FillPolygon(color, points));
                newImage.Mutate(x => x.DrawPolygon(color, 1, points));
            }
            var newDifference = Difference(newImage);
            if (newDifference < bestDifference)
            {
                bestDifference = newDifference;
                glass = newImage;
                return true;
            }

            return false;
        }

        if (Experiment())
        {
            Action<Point[], int, int>[] actions =
            [
                (coordinates, index, amount) => coordinates[index].X += amount,
                (coordinates, index, amount) => coordinates[index].Y += amount,
            ];

            for (var index = 0; index < coordinates.Length; index++)
            {
                foreach (var action in actions)
                {
                    foreach (var amount in new[] { -1, 1 })
                    {
                        while (true)
                        {
                            var oldCoordinates = coordinates.Select(c => c).ToArray();
                            action(coordinates, index, amount);
                            if (!Experiment())
                            {
                                coordinates = oldCoordinates;
                                break;
                            }
                        }
                    }
                }
            }

            shapes.Add((coordinates, color));
        }
    }

    private Image<Rgba32> RandomCrop(out Point[] coordinates)
    {
        while (true)
        {
            var randomCoordinates = RandomCoorindates();
            var boundingBox = BoundingBox(randomCoordinates);
            if (boundingBox.Width > 0 && boundingBox.Height > 0)
            {
                var region = original.Clone(x => x.Crop(BoundingBox(randomCoordinates)));
                coordinates = randomCoordinates;
                return region;
            }
        }
    }

    private void CreateOutput(string outFile, int height, bool vector, int animatedLength)
    {
        var averageColor = GetAverageColor(original);
        var ratio = height / (float)original.Height;
        var outputSize = new Size((int)(original.Width * ratio), (int)(original.Height * ratio));
        using var outputImage = new Image<Rgba32>(outputSize.Width, outputSize.Height, averageColor);

        var svg = vector ? new Svg(outputSize.Width, outputSize.Height, averageColor) : null;
        using var gif = animationLength > 0 ? new Image<Rgba32>(outputSize.Width, outputSize.Height, averageColor) : null;
        foreach (var (coordinateList, color) in shapes)
        {
            var coordinates = coordinateList.Select(p => new Point((int)(p.X * ratio), (int)(p.Y * ratio))).ToArray();
            if (shapeType == ShapeType.Ellipse)
            {
                var boundingBox = BoundingBox(coordinates);
                var ellipse = new EllipsePolygon(boundingBox.X, boundingBox.Y, boundingBox.Width, boundingBox.Height);
                outputImage.Mutate(x => x.Fill(color, ellipse));
                outputImage.Mutate(x => x.Draw(color, 1, ellipse));
                if (svg is not null)
                {
                    svg.DrawEllipse(boundingBox.X, boundingBox.Y, boundingBox.X + boundingBox.Width, boundingBox.Y + boundingBox.Height, color);
                }
            }
            else
            {
                var points = coordinates.Select(p => new PointF(p.X, p.Y)).ToArray();
                outputImage.Mutate(x => x.FillPolygon(color, points));
                outputImage.Mutate(x => x.DrawPolygon(color, 1, points));
                if (svg is not null)
                {
                    if (shapeType == ShapeType.Line)
                    {
                        svg.DrawLine(coordinates[0].X, coordinates[0].Y, coordinates[1].X, coordinates[1].Y, color);
                    }
                    else
                    {
                        int[] polygonCoordinates = coordinates.SelectMany(p => new[] { p.X, p.Y }).ToArray();
                        svg.DrawPolygon(polygonCoordinates, color);
                    }
                }
            }
            if (gif is not null)
            {
                using var image = outputImage.Clone();
                var metadata = image.Frames.RootFrame.Metadata.GetGifMetadata();
                metadata.FrameDelay = animatedLength / 10;
                gif.Frames.AddFrame(image.Frames.RootFrame);
            }
        }
        var outputFileName = System.IO.Path.GetFileNameWithoutExtension(outFile);
        outputImage.SaveAsPng($"{outputFileName}.png");
        if (svg is not null)
        {
            svg.Write($"{outputFileName}.svg");
        }
        if (gif is not null)
        {
            gif.SaveAsGif($"{outputFileName}.gif");
        }
    }
}
