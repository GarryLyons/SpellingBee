namespace Backend.Contracts;

public record PracticeSessionSummary(
    Guid Id,
    DateTimeOffset CreatedAt,
    int WordCount,
    int CompletedWords,
    int Cycle
);
