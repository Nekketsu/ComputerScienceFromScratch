using SixLabors.ImageSharp;
using System.Text;

namespace Impressionist;

public class Svg(int width, int height, Color backgroundColor)
{
    private StringBuilder content = new($"""
        <?xml version="1.0" encoding="utf-8"?>
        <svg version="1.1" baseProfile="full" width="{width}" height="{height}" xmlns="http://www.w3.org/2000/svg">
            <rect width="100%" height="100%" fill="{backgroundColor.SvgRgbString}" />

        """);

    public void DrawEllipse(int x1, int y1, int x2, int y2, Color color)
    {
        content.AppendLine($"""
                <ellipse cx="{(x1 + x2) / 2}" cy="{(y1 + y2) / 2}" rx="{Math.Abs(x1 - x2) / 2}" ry="{Math.Abs(y1 - y2) / 2}" fill="{color.SvgRgbString}" />
            """);
    }

    public void DrawLine(int x1, int y1, int x2, int y2, Color color)
    {
        content.AppendLine($"""
                <line x1="{x1}" y1="{y1}" x2="{x2}" y2="{y2}" stroke="{color.SvgRgbString}" stroke-width="1px" shape-rendering="crispEdges" />
            """);
    }

    public void DrawPolygon(int[] coordinates, Color color)
    {
        content.AppendLine($"""
                <polygon points="{string.Join(',', coordinates)}" fill="{color.SvgRgbString}" />
            """);
    }

    public void Write(string path)
    {
        content.AppendLine("""
            </svg>
            """);

        File.WriteAllText(path, content.ToString());
    }
}
