namespace Backend.Models;

public sealed class WordEntry
{
    public required string Id { get; init; }
    public required string Word { get; init; }
    public required PhonicsBreakdown Phonics { get; init; }
    public SegmentationOptions? Segmentation { get; init; }
}
