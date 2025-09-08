using System;

namespace Antifraud.Domain.ValueObjects;

public sealed record TransactionStatus
{
    public string Value { get; }

    private TransactionStatus(string value)
    {
        Value = value;
    }

    public static readonly TransactionStatus Pending = new("pending");
    public static readonly TransactionStatus Approved = new("approved");
    public static readonly TransactionStatus Rejected = new("rejected");

    public static TransactionStatus From(string value)
    {
        return value?.ToLowerInvariant() switch
        {
            "pending" => Pending,
            "approved" => Approved,
            "rejected" => Rejected,
            _ => throw new ArgumentException($"Invalid transaction status: {value}", nameof(value))
        };
    }

    public bool IsPending => this == Pending;
    public bool IsApproved => this == Approved;
    public bool IsRejected => this == Rejected;

    public static implicit operator string(TransactionStatus status) => status.Value;
    public static implicit operator TransactionStatus(string value) => From(value);

    public override string ToString() => Value;
}
