namespace Backend.Models;

public sealed record WordProgress
{
    public required string WordId { get; init; }
    public int Attempts { get; init; }
    public int FirstTryCorrect { get; init; }
    public int CorrectedAfterSupport { get; init; }
    public int FirstAttemptIncorrect { get; init; }
    public int HighPriorityIncorrect { get; init; }
    public required string Status { get; init; }
    public int NextFibonacciIndex { get; init; }
    public bool WasEverWrong { get; init; }
    public int Streak { get; init; }
    public int Difficulty { get; init; }
    public int DueIn { get; init; }
    public int LastSeenTurn { get; init; }
}
