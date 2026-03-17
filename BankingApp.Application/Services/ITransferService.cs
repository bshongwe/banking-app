using BankingApp.Domain.Entities;

namespace BankingApp.Application.Services;

public interface ITransferService
{
    /// <summary>
    /// Transfers money from one account to another using double-entry bookkeeping.
    /// Each transfer creates two ledger entries: a debit on the source account and a credit on the destination account.
    /// </summary>
    Task<Transaction> TransferAsync(Guid fromAccountId, Guid toAccountId, decimal amount, string reference);
}
