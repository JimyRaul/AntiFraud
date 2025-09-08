using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Antifraud.Domain.Entities;
using Antifraud.Domain.ValueObjects;

namespace Antifraud.Domain.Repositories;

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(TransactionId id);
    Task<Transaction?> GetByExternalIdAsync(Guid externalId);
    Task<IEnumerable<Transaction>> GetByAccountIdAsync(AccountId accountId);
    Task<IEnumerable<Transaction>> GetByAccountIdAndDateAsync(AccountId accountId, DateTime date);
    Task<Money> GetDailyAccumulatedAmountAsync(AccountId accountId, DateTime date);
    Task AddAsync(Transaction transaction);
    Task UpdateAsync(Transaction transaction);
    Task<bool> ExistsAsync(TransactionId id);
}