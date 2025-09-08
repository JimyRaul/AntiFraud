using System;
using Antifraud.Domain.ValueObjects;

namespace Antifraud.Domain.Events;

public sealed record TransactionCreatedEvent(
    TransactionId TransactionId,
    AccountId SourceAccountId,
    AccountId TargetAccountId,
    TransferTypeId TransferTypeId,
    Money Value,
    DateTime CreatedAt) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}