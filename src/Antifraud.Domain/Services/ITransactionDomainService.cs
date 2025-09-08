using System;
using System.Threading.Tasks;
using Antifraud.Domain.Entities;
using Antifraud.Domain.ValueObjects;

namespace Antifraud.Domain.Services;

public interface ITransactionDomainService
{
    Task<bool> ShouldRejectTransactionAsync(Transaction transaction);
    Task<string> GetRejectionReasonAsync(Transaction transaction);
}