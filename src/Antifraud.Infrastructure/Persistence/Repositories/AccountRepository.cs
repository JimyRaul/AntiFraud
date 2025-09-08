using Microsoft.EntityFrameworkCore;
using Antifraud.Domain.Entities;
using Antifraud.Domain.Repositories;
using Antifraud.Domain.ValueObjects;
using Antifraud.Infrastructure.Persistence;

namespace Antifraud.Infrastructure.Persistence.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly ApplicationDbContext _context;

    public AccountRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Account?> GetByIdAsync(AccountId id)
    {
        return await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<bool> ExistsAsync(AccountId id)
    {
        return await _context.Accounts
            .AnyAsync(a => a.Id == id && a.IsActive);
    }

    public async Task AddAsync(Account account)
    {
        await _context.Accounts.AddAsync(account);
    }

    public async Task UpdateAsync(Account account)
    {
        _context.Accounts.Update(account);
        await Task.CompletedTask;
    }
}