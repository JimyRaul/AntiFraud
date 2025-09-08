using System;
using System.Threading.Tasks;
using Antifraud.Application.DTOs;
using Antifraud.Application.Interfaces;
using Antifraud.Domain.Entities;
using Antifraud.Domain.Exceptions;
using Antifraud.Domain.Repositories;
using Antifraud.Domain.ValueObjects;

namespace Antifraud.Application.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IAntifraudService _antifraudService;
    private readonly IUnitOfWork _unitOfWork;

    public TransactionService(
        ITransactionRepository transactionRepository,
        IAccountRepository accountRepository,
        IAntifraudService antifraudService,
        IUnitOfWork unitOfWork)
    {
        _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        _antifraudService = antifraudService ?? throw new ArgumentNullException(nameof(antifraudService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<TransactionResponse>> CreateTransactionAsync(CreateTransactionRequest request)
    {
        try
        {
            // Validar que las cuentas existan
            var sourceAccountExists = await _accountRepository.ExistsAsync(AccountId.From(request.SourceAccountId));
            var targetAccountExists = await _accountRepository.ExistsAsync(AccountId.From(request.TargetAccountId));

            if (!sourceAccountExists)
                return Result.Failure<TransactionResponse>($"Source account {request.SourceAccountId} does not exist");

            if (!targetAccountExists)
                return Result.Failure<TransactionResponse>($"Target account {request.TargetAccountId} does not exist");

            // Crear la transacción
            var transaction = Transaction.Create(
                AccountId.From(request.SourceAccountId),
                AccountId.From(request.TargetAccountId),
                TransferTypeId.From(request.TransferTypeId),
                Money.From(request.Value));

            // Guardar en repositorio
            await _transactionRepository.AddAsync(transaction);
            await _unitOfWork.SaveChangesAsync();

            // Crear respuesta
            var response = MapToResponse(transaction);

            // Enviar para validación anti-fraude (async)
            _ = Task.Run(async () => await _antifraudService.ProcessTransactionForValidationAsync(response));

            return Result.Success(response);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<TransactionResponse>(ex.Message);
        }
        catch (Exception ex)
        {
            return Result.Failure<TransactionResponse>($"An error occurred while creating the transaction: {ex.Message}");
        }
    }

    public async Task<Result<TransactionResponse>> GetTransactionAsync(GetTransactionRequest request)
    {
        try
        {
            var transaction = await _transactionRepository.GetByExternalIdAsync(request.TransactionExternalId);

            if (transaction == null)
                return Result.Failure<TransactionResponse>($"Transaction {request.TransactionExternalId} not found");

            var response = MapToResponse(transaction);
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            return Result.Failure<TransactionResponse>($"An error occurred while retrieving the transaction: {ex.Message}");
        }
    }

    public async Task<Result<bool>> ProcessTransactionValidationAsync(TransactionValidationEvent validationEvent)
    {
        try
        {
            var transaction = await _transactionRepository.GetByExternalIdAsync(validationEvent.TransactionExternalId);

            if (transaction == null)
                return Result.Failure<bool>($"Transaction {validationEvent.TransactionExternalId} not found");

            // Actualizar estado según el resultado de la validación
            var newStatus = TransactionStatus.From(validationEvent.Status);
            
            if (newStatus.IsApproved)
            {
                transaction.Approve();
            }
            else if (newStatus.IsRejected)
            {
                transaction.Reject(validationEvent.Reason ?? "Rejected by anti-fraud system");
            }

            await _transactionRepository.UpdateAsync(transaction);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>($"An error occurred while processing validation: {ex.Message}");
        }
    }

    private static TransactionResponse MapToResponse(Transaction transaction)
    {
        return new TransactionResponse
        {
            TransactionExternalId = transaction.Id.Value,
            SourceAccountId = transaction.SourceAccountId.Value,
            TargetAccountId = transaction.TargetAccountId.Value,
            TransferTypeId = transaction.TransferTypeId.Value,
            Value = transaction.Value.Amount,
            Currency = transaction.Value.Currency,
            Status = transaction.Status.Value,
            CreatedAt = transaction.CreatedAt,
            UpdatedAt = transaction.UpdatedAt
        };
    }
}