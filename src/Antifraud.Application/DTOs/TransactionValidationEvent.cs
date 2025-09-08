using System;

namespace Antifraud.Application.DTOs;

public sealed record TransactionValidationEvent
{
    public Guid TransactionExternalId { get; init; }
    public required string Status { get; init; }
    public string? Reason { get; init; }
    public DateTime ProcessedAt { get; init; }
}