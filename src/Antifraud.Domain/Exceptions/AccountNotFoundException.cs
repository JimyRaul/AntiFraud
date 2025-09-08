using Antifraud.Domain.ValueObjects;

namespace Antifraud.Domain.Exceptions;

public sealed class AccountNotFoundException : DomainException
{
    public AccountNotFoundException(AccountId accountId)
        : base($"Account with ID '{accountId}' was not found.")
    {
    }
}