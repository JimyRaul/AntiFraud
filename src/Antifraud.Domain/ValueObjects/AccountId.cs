using System;

namespace Antifraud.Domain.ValueObjects;

public sealed record AccountId
{
    public Guid Value { get; }

    private AccountId(Guid value)
    {
        Value = value;
    }

    public static AccountId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Account ID cannot be empty", nameof(value));
            
        return new AccountId(value);
    }

    public static implicit operator Guid(AccountId accountId) => accountId.Value;
    public static implicit operator AccountId(Guid value) => From(value);

    public override string ToString() => Value.ToString();
}