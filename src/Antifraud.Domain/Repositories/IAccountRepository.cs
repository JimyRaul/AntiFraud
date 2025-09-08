using System.Threading.Tasks;
using Antifraud.Domain.Entities;
using Antifraud.Domain.ValueObjects;

namespace Antifraud.Domain.Repositories;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(AccountId id);
    Task<bool> ExistsAsync(AccountId id);
    Task AddAsync(Account account);
    Task UpdateAsync(Account account);
}