using System;
using Antifraud.Domain.ValueObjects;
using Antifraud.Domain.Events;

namespace Antifraud.Domain.Entities;

public class Transaction
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public TransactionId Id { get; private set; }
    public AccountId SourceAccountId { get; private set; }
    public AccountId TargetAccountId { get; private set; }
    public TransferTypeId TransferTypeId { get; private set; }
    public Money Value { get; private set; }
    public TransactionStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    // Constructor privado para EF Core
    private Transaction() { }

    // Constructor para crear nueva transacción
    private Transaction(
        TransactionId id,
        AccountId sourceAccountId,
        AccountId targetAccountId,
        TransferTypeId transferTypeId,
        Money value)
    {
        Id = id;
        SourceAccountId = sourceAccountId;
        TargetAccountId = targetAccountId;
        TransferTypeId = transferTypeId;
        Value = value;
        Status = TransactionStatus.Pending;
        CreatedAt = DateTime.UtcNow;

        // Agregar evento de dominio
        AddDomainEvent(new TransactionCreatedEvent(
            Id,
            SourceAccountId,
            TargetAccountId,
            TransferTypeId,
            Value,
            CreatedAt));
    }

    public static Transaction Create(
        AccountId sourceAccountId,
        AccountId targetAccountId,
        TransferTypeId transferTypeId,
        Money value)
    {
        ValidateCreationParameters(sourceAccountId, targetAccountId, transferTypeId, value);

        return new Transaction(
            TransactionId.New(),
            sourceAccountId,
            targetAccountId,
            transferTypeId,
            value);
    }

    public void UpdateStatus(TransactionStatus newStatus, string reason = null)
    {
        if (Status == newStatus)
            return;

        var previousStatus = Status;
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new TransactionStatusUpdatedEvent(
            Id,
            previousStatus,
            newStatus,
            reason,
            UpdatedAt.Value));
    }

    public void Approve()
    {
        if (!Status.IsPending)
            throw new InvalidOperationException("Only pending transactions can be approved");

        UpdateStatus(TransactionStatus.Approved);
    }

    public void Reject(string reason)
    {
        if (!Status.IsPending)
            throw new InvalidOperationException("Only pending transactions can be rejected");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Rejection reason is required", nameof(reason));

        UpdateStatus(TransactionStatus.Rejected, reason);
    }

    public bool IsFromAccount(AccountId accountId) => SourceAccountId == accountId;
    public bool IsToAccount(AccountId accountId) => TargetAccountId == accountId;
    public bool InvolvesAccount(AccountId accountId) => IsFromAccount(accountId) || IsToAccount(accountId);

    public bool IsCreatedOnDate(DateTime date) => 
        CreatedAt.Date == date.Date;

    private static void ValidateCreationParameters(
        AccountId sourceAccountId,
        AccountId targetAccountId,
        TransferTypeId transferTypeId,
        Money value)
    {
        if (sourceAccountId == targetAccountId)
            throw new ArgumentException("Source and target accounts cannot be the same");

        if (value.Amount <= 0)
            throw new ArgumentException("Transaction value must be greater than zero");
    }

    private void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}