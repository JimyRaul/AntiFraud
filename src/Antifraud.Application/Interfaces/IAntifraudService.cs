using System.Threading.Tasks;
using Antifraud.Application.DTOs;

namespace Antifraud.Application.Interfaces;

public interface IAntifraudService
{
    Task ProcessTransactionForValidationAsync(TransactionResponse transaction);
}