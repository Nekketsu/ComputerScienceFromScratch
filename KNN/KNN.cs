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
}
