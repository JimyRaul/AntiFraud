using System;
using System.Threading.Tasks;
using Antifraud.Domain.Entities;
using Antifraud.Domain.Repositories;
using Antifraud.Domain.ValueObjects;

namespace Antifraud.Domain.Services;

public class TransactionDomainService : ITransactionDomainService
{
    private readonly ITransactionRepository _transactionRepository;
    private static readonly Money MaxTransactionAmount = Money.From(2000);
    private static readonly Money MaxDailyAmount = Money.From(20000);

    public TransactionDomainService(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
    }

    public async Task<bool> ShouldRejectTransactionAsync(Transaction transaction)
    {
        // Regla 1: Transacciones mayores a $2000
        if (transaction.Value.IsGreaterThan(MaxTransactionAmount))
            return true;

        // Regla 2: Acumulado diario mayor a $20000
        var dailyAccumulated = await _transactionRepository
            .GetDailyAccumulatedAmountAsync(transaction.SourceAccountId, transaction.CreatedAt.Date);
        
        var newDailyTotal = dailyAccumulated.Add(transaction.Value);
        
        return newDailyTotal.IsGreaterThan(MaxDailyAmount);
    }

    public async Task<string> GetRejectionReasonAsync(Transaction transaction)
    {
        // Verificar regla de monto máximo por transacción
        if (transaction.Value.IsGreaterThan(MaxTransactionAmount))
        {
            return $"Transaction amount {transaction.Value} exceeds the maximum allowed amount of {MaxTransactionAmount}";
        }

        // Verificar regla de acumulado diario
        var dailyAccumulated = await _transactionRepository
            .GetDailyAccumulatedAmountAsync(transaction.SourceAccountId, transaction.CreatedAt.Date);
        
        var newDailyTotal = dailyAccumulated.Add(transaction.Value);
        
        if (newDailyTotal.IsGreaterThan(MaxDailyAmount))
        {
            return $"Daily accumulated amount {newDailyTotal} would exceed the maximum daily limit of {MaxDailyAmount}. Current daily total: {dailyAccumulated}";
        }

        return string.Empty;
    }
}