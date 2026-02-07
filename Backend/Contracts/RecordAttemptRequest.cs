using System.ComponentModel.DataAnnotations;

namespace Backend.Contracts;

public sealed class RecordAttemptRequest
{
    [Required]
    [RegularExpression("^(correct|incorrect)$", ErrorMessage = "Outcome must be either 'correct' or 'incorrect'.")]
    public string Outcome { get; init; } = string.Empty;
}
