using Backend.Models;

namespace Backend.Contracts;

public sealed class PracticeSessionResponse
{
    public required Guid SessionId { get; init; }
    public required PracticeState State { get; init; }
}
