using System;
using System.Threading.Tasks;
using Antifraud.Application.DTOs;
using Antifraud.Application.Interfaces;

namespace Antifraud.Application.UseCases;

public class ProcessTransactionValidationUseCase
{
    private readonly ITransactionService _transactionService;

    public ProcessTransactionValidationUseCase(ITransactionService transactionService)
    {
        _transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
    }

    public async Task<Result<bool>> ExecuteAsync(TransactionValidationEvent validationEvent)
    {
        // Validaciones del caso de uso
        if (validationEvent.TransactionExternalId == Guid.Empty)
        {
            return Result.Failure<bool>("Transaction external ID cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(validationEvent.Status))
        {
            return Result.Failure<bool>("Transaction status cannot be empty");
        }

        var validStatuses = new[] { "pending", "approved", "rejected" };
        if (!Array.Exists(validStatuses, status => 
            string.Equals(status, validationEvent.Status, StringComparison.OrdinalIgnoreCase)))
        {
            return Result.Failure<bool>($"Invalid transaction status: {validationEvent.Status}");
        }

        // Delegar al servicio de transacciones
        return await _transactionService.ProcessTransactionValidationAsync(validationEvent);
    }
}