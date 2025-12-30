namespace KNN;

public class Digit(string kind, int[] pixels) : IDataPoint<Digit>
{
    public string Kind { get; } = kind;
    public int[] Pixels { get; } = pixels;

    public static Digit FromStringData(string[] data) => new Digit
    (
        kind: data[64],
        pixels: [.. data[..64].Select(int.Parse)]
    );

    public float Distance(Digit other) => float.Sqrt(Pixels
        .Zip(other.Pixels, (p, o) => (p - o) * (p - o))
        .Sum());
}
