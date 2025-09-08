using Antifraud.Domain.ValueObjects;

namespace Antifraud.Domain.Entities;

public class Account
{
    public AccountId Id { get; private set; }
    public string AccountNumber { get; private set; }
    public string HolderName { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsActive { get; private set; }

    // Constructor privado para EF Core
    private Account()
    {
        Id = default!;
        AccountNumber = string.Empty;
        HolderName = string.Empty;
        CreatedAt = DateTime.UtcNow;
        IsActive = false;
    }

    private Account(AccountId id, string accountNumber, string holderName)
    {
        Id = id;
        AccountNumber = accountNumber;
        HolderName = holderName;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
    }

    public static Account Create(AccountId id, string accountNumber, string holderName)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
            throw new ArgumentException("Account number cannot be null or empty", nameof(accountNumber));

        if (string.IsNullOrWhiteSpace(holderName))
            throw new ArgumentException("Holder name cannot be null or empty", nameof(holderName));

        return new Account(id, accountNumber, holderName);
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}