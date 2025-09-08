using Microsoft.EntityFrameworkCore;
using Antifraud.Domain.Entities;
using Antifraud.Domain.Repositories;
using Antifraud.Domain.ValueObjects;
using Antifraud.Infrastructure.Persistence;

namespace Antifraud.Infrastructure.Persistence.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly ApplicationDbContext _context;

    public TransactionRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Transaction?> GetByIdAsync(TransactionId id)
    {
        return await _context.Transactions
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Transaction?> GetByExternalIdAsync(Guid externalId)
    {
        var transactionId = TransactionId.From(externalId);
        return await _context.Transactions
            .FirstOrDefaultAsync(t => t.Id == transactionId);
    }

    public async Task<IEnumerable<Transaction>> GetByAccountIdAsync(AccountId accountId)
    {
        return await _context.Transactions
            .Where(t => t.SourceAccountId == accountId || t.TargetAccountId == accountId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Transaction>> GetByAccountIdAndDateAsync(AccountId accountId, DateTime date)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);

        return await _context.Transactions
            .Where(t => t.SourceAccountId == accountId && 
                       t.CreatedAt >= startOfDay && 
                       t.CreatedAt < endOfDay)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<Money> GetDailyAccumulatedAmountAsync(AccountId accountId, DateTime date)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);

        var transactions = await _context.Transactions
            .Where(t => t.SourceAccountId == accountId && 
                       t.CreatedAt >= startOfDay && 
                       t.CreatedAt < endOfDay &&
                       (t.Status == TransactionStatus.Pending || t.Status == TransactionStatus.Approved))
            .ToListAsync();

        if (!transactions.Any())
            return Money.Zero;

        // Sumar todas las transacciones del día
        var total = transactions
            .Select(t => t.Value)
            .Aggregate((acc, current) => acc.Add(current));

        return total;
    }

    public async Task AddAsync(Transaction transaction)
    {
        await _context.Transactions.AddAsync(transaction);
    }

    public async Task UpdateAsync(Transaction transaction)
    {
        _context.Transactions.Update(transaction);
        await Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(TransactionId id)
    {
        return await _context.Transactions
            .AnyAsync(t => t.Id == id);
    }
}