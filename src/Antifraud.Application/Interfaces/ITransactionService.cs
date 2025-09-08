using System;
using System.Threading.Tasks;
using Antifraud.Application.DTOs;

namespace Antifraud.Application.Interfaces;

public interface ITransactionService
{
    Task<Result<TransactionResponse>> CreateTransactionAsync(CreateTransactionRequest request);
    Task<Result<TransactionResponse>> GetTransactionAsync(GetTransactionRequest request);
    Task<Result<bool>> ProcessTransactionValidationAsync(TransactionValidationEvent validationEvent);
}