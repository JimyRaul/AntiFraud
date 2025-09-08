using System;
using System.Threading.Tasks;
using Antifraud.Application.DTOs;
using Antifraud.Application.Interfaces;

namespace Antifraud.Application.UseCases;

public class CreateTransactionUseCase
{
    private readonly ITransactionService _transactionService;

    public CreateTransactionUseCase(ITransactionService transactionService)
    {
        _transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
    }

    public async Task<Result<TransactionResponse>> ExecuteAsync(CreateTransactionRequest request)
    {
        // Validaciones adicionales del caso de uso si son necesarias
        if (request.SourceAccountId == request.TargetAccountId)
        {
            return Result.Failure<TransactionResponse>("Source and target accounts cannot be the same");
        }

        if (request.Value <= 0)
        {
            return Result.Failure<TransactionResponse>("Transaction value must be greater than zero");
        }

        // Delegar al servicio de transacciones
        return await _transactionService.CreateTransactionAsync(request);
    }
}