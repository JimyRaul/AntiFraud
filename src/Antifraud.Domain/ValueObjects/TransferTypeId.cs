using System;

namespace Antifraud.Domain.ValueObjects;

public sealed record TransferTypeId
{
    public int Value { get; }

    private TransferTypeId(int value)
    {
        Value = value;
    }

    public static TransferTypeId From(int value)
    {
        if (value <= 0)
            throw new ArgumentException("Transfer type ID must be greater than zero", nameof(value));
            
        return new TransferTypeId(value);
    }

    public static implicit operator int(TransferTypeId transferTypeId) => transferTypeId.Value;
    public static implicit operator TransferTypeId(int value) => From(value);

    public override string ToString() => Value.ToString();
}