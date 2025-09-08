using System;

namespace Antifraud.Domain.ValueObjects;

public sealed record TransactionId
{
    public Guid Value { get; }

    private TransactionId(Guid value)
    {
        Value = value;
    }

    public static TransactionId New() => new(Guid.NewGuid());
    
    public static TransactionId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Transaction ID cannot be empty", nameof(value));
            
        return new TransactionId(value);
    }

    public static implicit operator Guid(TransactionId transactionId) => transactionId.Value;
    public static implicit operator TransactionId(Guid value) => From(value);

    public override string ToString() => Value.ToString();
}