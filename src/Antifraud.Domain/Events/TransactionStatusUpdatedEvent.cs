using System;
using Antifraud.Domain.ValueObjects;

namespace Antifraud.Domain.Events;

public sealed record TransactionStatusUpdatedEvent(
    TransactionId TransactionId,
    TransactionStatus PreviousStatus,
    TransactionStatus NewStatus,
    string Reason,
    DateTime UpdatedAt) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}