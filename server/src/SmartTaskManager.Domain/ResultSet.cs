namespace SmartTaskManager.Domain;

public class ResultSet<T>
{
    public ResultSet() { }

    public ResultSet(ICollection<T> data, int count = 0)
    {
        Data = data ?? [];
        TotalCount = count;
    }

    public ICollection<T> Data { get; set; } = [];
    public int TotalCount { get; set; }
}
