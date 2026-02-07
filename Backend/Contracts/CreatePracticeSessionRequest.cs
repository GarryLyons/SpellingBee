using System.ComponentModel.DataAnnotations;

namespace Backend.Contracts;

public sealed class CreatePracticeSessionRequest : IValidatableObject
{
    public List<string>? WordIds { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (WordIds is { Count: 0 })
        {
            yield return new ValidationResult(
                "wordIds must include at least one value when provided.",
                [nameof(WordIds)]);
        }
    }
}
