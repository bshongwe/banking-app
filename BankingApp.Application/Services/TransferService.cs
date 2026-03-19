using BankingApp.Domain.Entities;
using BankingApp.Infrastructure.Repositories;

namespace BankingApp.Application.Services;

public class TransferService(
    ITransactionRepository transactionRepository,
    IAccountRepository accountRepository,
    ILedgerRepository ledgerRepository) : ITransferService
{

    public async Task<Transaction> TransferAsync(Guid fromAccountId, Guid toAccountId, decimal amount, string reference)
    {
        if (amount <= 0)
            throw new ArgumentException("Transfer amount must be greater than zero.", nameof(amount));

        if (fromAccountId == toAccountId)
            throw new ArgumentException("Cannot transfer to the same account.", nameof(toAccountId));

        var fromAccount = await accountRepository.GetByIdAsync(fromAccountId);
        if (fromAccount == null)
            throw new InvalidOperationException("Source account not found.");

        var toAccount = await accountRepository.GetByIdAsync(toAccountId);
        if (toAccount == null)
            throw new InvalidOperationException("Destination account not found.");

        // Check if source account has sufficient balance
        var fromAccountBalance = await accountRepository.GetBalanceAsync(fromAccountId);
        if (fromAccountBalance < amount)
            throw new InvalidOperationException("Insufficient funds.");

        // Create a transaction and two corresponding ledger entries
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            Reference = reference,
            CreatedAt = DateTime.UtcNow
        };

        var debitEntry = new LedgerEntry
        {
            Id = Guid.NewGuid(),
            TransactionId = transaction.Id,
            AccountId = fromAccountId,
            Amount = amount,
            EntryType = "Debit",
            CreatedAt = DateTime.UtcNow
        };

        var creditEntry = new LedgerEntry
        {
            Id = Guid.NewGuid(),
            TransactionId = transaction.Id,
            AccountId = toAccountId,
            Amount = amount,
            EntryType = "Credit",
            CreatedAt = DateTime.UtcNow
        };

        // Add entries and save as a single transaction
        await transactionRepository.AddAsync(transaction);
        await ledgerRepository.AddRangeAsync(new[] { debitEntry, creditEntry });
        await ledgerRepository.SaveChangesAsync();

        return transaction;
    }
}
