using System;
using System.ComponentModel.DataAnnotations;

namespace Antifraud.Application.DTOs;

public sealed record CreateTransactionRequest
{
    [Required]
    public Guid SourceAccountId { get; init; }

    [Required]
    public Guid TargetAccountId { get; init; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Transfer type ID must be greater than 0")]
    public int TransferTypeId { get; init; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Value must be greater than 0")]
    public decimal Value { get; init; }
}