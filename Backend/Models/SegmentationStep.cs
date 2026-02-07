namespace Backend.Models;

public sealed class SegmentationStep
{
    public required string Level { get; init; }
    public int LevelNumber { get; init; }
    public required string Title { get; init; }
    public required string Value { get; init; }
}
