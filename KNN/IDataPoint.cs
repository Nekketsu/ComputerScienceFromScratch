namespace KNN;

public interface IDataPoint<T> where T : IDataPoint<T>
{
    public string Kind { get; }

    static abstract T FromStringData(string[] data);

    abstract float Distance(T other);
}
