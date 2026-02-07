using System.ComponentModel.DataAnnotations;

namespace Backend.Utilities;

public static class ValidationExtensions
{
    public static Dictionary<string, string[]>? ValidateRequest<T>(this T request)
    {
        if (request is null)
        {
            return new Dictionary<string, string[]>
            {
                ["request"] = ["Request body is required."]
            };
        }

        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();
        var valid = Validator.TryValidateObject(request, context, results, true);
        if (valid)
        {
            return null;
        }

        return results
            .SelectMany(result =>
            {
                var members = result.MemberNames.Any() ? result.MemberNames : ["request"];
                return members.Select(member => new { member, message = result.ErrorMessage ?? "Invalid value." });
            })
            .GroupBy(item => item.member)
            .ToDictionary(
                group => group.Key,
                group => group.Select(item => item.message).Distinct(StringComparer.Ordinal).ToArray(),
                StringComparer.Ordinal);
    }
}
