namespace KNN;

public class Fish(string kind, float weight, float length1, float length2, float length3, float height, float width) : IDataPoint<Fish>
{
    public string Kind { get; } = kind;
    public float Weight { get; } = weight;
    public float Length1 { get; } = length1;
    public float Length2 { get; } = length2;
    public float Length3 { get; } = length3;
    public float Height { get; } = height;
    public float Width { get; } = width;

    public static Fish FromStringData(string[] data) => new Fish
    (
        kind: data[0],
        weight: float.Parse(data[1]),
        length1: float.Parse(data[2]),
        length2: float.Parse(data[3]),
        length3: float.Parse(data[4]),
        height: float.Parse(data[5]),
        width: float.Parse(data[6])
    );

    public float Distance(Fish other) => float.Sqrt(
        float.Pow(Length1 - other.Length1, 2) +
        float.Pow(Length2 - other.Length2, 2) +
        float.Pow(Length3 - other.Length3, 2) +
        float.Pow(Height - other.Height, 2) +
        float.Pow(Width - other.Width, 2));
}
