namespace BindingListThreadPitfallDemo;

public sealed class DemoRow
{
    public int No { get; init; }

    public string Message { get; init; } = string.Empty;

    public int CreatedOnThreadId { get; init; }

    public DateTime CreatedAt { get; init; }
}
