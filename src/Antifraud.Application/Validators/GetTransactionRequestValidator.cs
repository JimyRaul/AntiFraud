using System.ComponentModel.DataAnnotations;
using Antifraud.Application.DTOs;

namespace Antifraud.Application.Validators;

public static class GetTransactionRequestValidator
{
    public static IEnumerable<ValidationResult> Validate(GetTransactionRequest request)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(request);

        // Validaciones usando DataAnnotations
        Validator.TryValidateObject(request, context, results, true);

        // Validaciones customizadas
        if (request.TransactionExternalId == Guid.Empty)
        {
            results.Add(new ValidationResult(
                "Transaction external ID cannot be empty",
                new[] { nameof(request.TransactionExternalId) }));
        }

        return results;
    }

    public static bool IsValid(GetTransactionRequest request, out IEnumerable<string> errors)
    {
        var validationResults = Validate(request);
        errors = validationResults.Select(vr => vr.ErrorMessage ?? "Unknown validation error");
        return !validationResults.Any();
    }
}