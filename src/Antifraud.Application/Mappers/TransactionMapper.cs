using Antifraud.Application.DTOs;
using Antifraud.Domain.Entities;

namespace Antifraud.Application.Mappers;

public static class TransactionMapper
{
    public static TransactionResponse ToResponse(Transaction transaction)
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

    public static IEnumerable<TransactionResponse> ToResponse(IEnumerable<Transaction> transactions)
    {
        return transactions.Select(ToResponse);
    }
}