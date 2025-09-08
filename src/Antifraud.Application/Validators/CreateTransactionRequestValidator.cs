using System.ComponentModel.DataAnnotations;
using Antifraud.Application.DTOs;

namespace Antifraud.Application.Validators;

public static class CreateTransactionRequestValidator
{
    public static IEnumerable<ValidationResult> Validate(CreateTransactionRequest request)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(request);

        // Validaciones usando DataAnnotations
        Validator.TryValidateObject(request, context, results, true);

        // Validaciones customizadas
        if (request.SourceAccountId == request.TargetAccountId)
        {
            results.Add(new ValidationResult(
                "Source and target accounts cannot be the same",
                new[] { nameof(request.SourceAccountId), nameof(request.TargetAccountId) }));
        }

        if (request.SourceAccountId == Guid.Empty)
        {
            results.Add(new ValidationResult(
                "Source account ID cannot be empty",
                new[] { nameof(request.SourceAccountId) }));
        }

        if (request.TargetAccountId == Guid.Empty)
        {
            results.Add(new ValidationResult(
                "Target account ID cannot be empty",
                new[] { nameof(request.TargetAccountId) }));
        }

        return results;
    }

    public static bool IsValid(CreateTransactionRequest request, out IEnumerable<string> errors)
    {
        var validationResults = Validate(request);
        errors = validationResults.Select(vr => vr.ErrorMessage ?? "Unknown validation error");
        return !validationResults.Any();
    }
}