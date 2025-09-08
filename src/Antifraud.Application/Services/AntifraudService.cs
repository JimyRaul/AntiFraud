using System;
using System.Text.Json;
using System.Threading.Tasks;
using Antifraud.Application.DTOs;
using Antifraud.Application.Interfaces;
using Antifraud.Domain.Entities;
using Antifraud.Domain.Repositories;
using Antifraud.Domain.Services;
using Antifraud.Domain.ValueObjects;

namespace Antifraud.Application.Services;

public class AntifraudService : IAntifraudService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ITransactionDomainService _transactionDomainService;
    private readonly IEventPublisher _eventPublisher;
    private const string ValidationResponseTopic = "transaction-validation-response";

    public AntifraudService(
        ITransactionRepository transactionRepository,
        ITransactionDomainService transactionDomainService,
        IEventPublisher eventPublisher)
    {
        _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        _transactionDomainService = transactionDomainService ?? throw new ArgumentNullException(nameof(transactionDomainService));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
    }

    public async Task ProcessTransactionForValidationAsync(TransactionResponse transactionResponse)
    {
        try
        {
            // Obtener la transacción del repositorio
            var transaction = await _transactionRepository.GetByExternalIdAsync(transactionResponse.TransactionExternalId);
            
            if (transaction == null)
            {
                await SendValidationResponseAsync(transactionResponse.TransactionExternalId, "rejected", "Transaction not found");
                return;
            }

            // Aplicar reglas de anti-fraude
            var shouldReject = await _transactionDomainService.ShouldRejectTransactionAsync(transaction);
            
            if (shouldReject)
            {
                var rejectionReason = await _transactionDomainService.GetRejectionReasonAsync(transaction);
                await SendValidationResponseAsync(transactionResponse.TransactionExternalId, "rejected", rejectionReason);
            }
            else
            {
                await SendValidationResponseAsync(transactionResponse.TransactionExternalId, "approved", null);
            }
        }
        catch (Exception ex)
        {
            // En caso de error, rechazar por seguridad
            await SendValidationResponseAsync(transactionResponse.TransactionExternalId, "rejected", $"Validation error: {ex.Message}");
        }
    }

    private async Task SendValidationResponseAsync(Guid transactionId, string status, string? reason)
    {
        var validationEvent = new TransactionValidationEvent
        {
            TransactionExternalId = transactionId,
            Status = status,
            Reason = reason,
            ProcessedAt = DateTime.UtcNow
        };

        await _eventPublisher.PublishAsync(validationEvent, ValidationResponseTopic);
    }
}