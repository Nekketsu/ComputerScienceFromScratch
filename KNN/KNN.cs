namespace KNN;

public class KNN<DP> where DP : IDataPoint<DP>
{
    private DP[] dataPoints;

    public KNN(string filePath, bool hasHeader = true)
    {
        dataPoints = ReadCsv(filePath, hasHeader);
    }

    // Read a CSV file and return a list of data points
    private DP[] ReadCsv(string filePath, bool HasHeader) => File.ReadLines(filePath)
        .Skip(HasHeader ? 1 : 0)
        .Select(row => DP.FromStringData(row.Split(',')))
        .ToArray();

    public DP[] Nearest(int k, DP dataPoint) => dataPoints
        .OrderBy(dataPoint.Distance)
        .Take(k)
        .ToArray();

    public string Classify(int k, DP dataPoint) => Nearest(k, dataPoint)
        .CountBy(neighbor => neighbor.Kind)
        .MaxBy(neighbor => neighbor.Value)
        .Key;

    // Predict a numeric property of a data point based on the k-nearest neighbors.
    // Find the average of that property from the neighbors and return it.
    public float Predit(int k, DP dataPoint, string propertyName)
    {
        var neighbors = Nearest(k, dataPoint);
        return neighbors.Select(neighbor => (float)(typeof(DP).GetProperty(propertyName)?.GetValue(neighbor) ?? 0f)).Sum() / neighbors.Length;
    }

    // Predict a NumPy array property of a data point based on the k-nearest neighbors.
    // Find the average of that property from the neighbors and return it.
    public float[] PredictArray(int k, DP dataPoint, string propertyName)
    {
        var neighbors = Nearest(k, dataPoint);
        var arrays = neighbors
            .Select(neighbor => (int[])(typeof(DP).GetProperty(propertyName)?.GetValue(neighbor) ?? Array.Empty<int>()))
            .ToArray();

        var length = arrays.Min(array => array.Length);
        var seed = new float[length];

        return [.. arrays
            .Aggregate(seed, (accumulator, array) => [.. accumulator.Zip(array, (first, second) => first + second)])
            .Select(sum => sum / neighbors.Length)];
    }
}
