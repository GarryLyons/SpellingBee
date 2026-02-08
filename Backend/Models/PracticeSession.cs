namespace Backend.Models;

public sealed class PracticeSession
{
    public required Guid Id { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public required List<string> WordIds { get; init; }
    public required PracticeState State { get; set; }
    public object SyncRoot { get; } = new();
}
