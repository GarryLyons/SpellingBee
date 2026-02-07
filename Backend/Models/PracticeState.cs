namespace Backend.Models;

public sealed record PracticeState
{
    public required List<WordEntry> WordBank { get; init; }
    public required Dictionary<string, WordProgress> Progress { get; init; }
    public required string CurrentWordId { get; init; }
    public int CurrentSegmentationIndex { get; init; }
    public bool Level1MissedOnCurrentWord { get; init; }
    public required string Phase { get; init; }
    public int Turn { get; init; }
    public int Cycle { get; init; }
    public required List<string> SeenInCycle { get; init; }
}
