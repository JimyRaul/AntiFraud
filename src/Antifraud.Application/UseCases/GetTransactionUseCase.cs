using System;
using System.Threading.Tasks;
using Antifraud.Application.DTOs;
using Antifraud.Application.Interfaces;

namespace Antifraud.Application.UseCases;

public class GetTransactionUseCase
{
    private readonly ITransactionService _transactionService;

    public GetTransactionUseCase(ITransactionService transactionService)
    {
        _transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
    }

    public async Task<Result<TransactionResponse>> ExecuteAsync(GetTransactionRequest request)
    {
        // Validaciones del caso de uso
        if (request.TransactionExternalId == Guid.Empty)
        {
            return Result.Failure<TransactionResponse>("Transaction external ID cannot be empty");
        }

        // Delegar al servicio de transacciones
        return await _transactionService.GetTransactionAsync(request);
    }
}
