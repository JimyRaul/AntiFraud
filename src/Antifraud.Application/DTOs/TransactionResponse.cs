using System;

namespace Antifraud.Application.DTOs;

public sealed record TransactionResponse
{
    public Guid TransactionExternalId { get; init; }
    public Guid SourceAccountId { get; init; }
    public Guid TargetAccountId { get; init; }
    public int TransferTypeId { get; init; }
    public decimal Value { get; init; }
    public string Currency { get; init; } = "USD";
    public required string Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}