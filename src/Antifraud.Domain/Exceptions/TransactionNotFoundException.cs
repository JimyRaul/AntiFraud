using Antifraud.Domain.ValueObjects;

namespace Antifraud.Domain.Exceptions;

public sealed class TransactionNotFoundException : DomainException
{
    public TransactionNotFoundException(TransactionId transactionId)
        : base($"Transaction with ID '{transactionId}' was not found.")
    {
    }

    public TransactionNotFoundException(Guid externalId)
        : base($"Transaction with external ID '{externalId}' was not found.")
    {
    }
}