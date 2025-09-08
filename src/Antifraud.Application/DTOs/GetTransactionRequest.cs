using System;
using System.ComponentModel.DataAnnotations;

namespace Antifraud.Application.DTOs;

public sealed record GetTransactionRequest
{
    [Required]
    public Guid TransactionExternalId { get; init; }

    public DateTime? CreatedAt { get; init; }
}