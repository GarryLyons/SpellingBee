namespace Backend.Models;

public sealed class PhonicsBreakdown
{
    public required List<string> Phonemes { get; init; }
    public required List<string> Graphemes { get; init; }
    public required string Segmenting { get; init; }
    public required string Blending { get; init; }
    public required List<string> Digraphs { get; init; }
    public required List<string> SplitDigraphs { get; init; }
    public required List<string> Rules { get; init; }
}
