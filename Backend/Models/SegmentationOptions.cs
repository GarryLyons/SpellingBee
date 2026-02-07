namespace Backend.Models;

public sealed class SegmentationOptions
{
    public string? MorphologicalUnits { get; init; }
    public string? Syllables { get; init; }
    public string? OnsetRime { get; init; }
    public string? GraphemeGroups { get; init; }
    public string? IndividualPhonemes { get; init; }
}
