using BankingApp.Domain.Entities;
using BankingApp.Infrastructure.Data;
using BankingApp.Infrastructure.Repositories;

namespace BankingApp.Application.Services;

public class TransferService : ITransferService
{
    private readonly BankingDbContext _context;
    private readonly IAccountRepository _accountRepository;
    private readonly ILedgerRepository _ledgerRepository;

    public TransferService(
        BankingDbContext context,
        IAccountRepository accountRepository,
        ILedgerRepository ledgerRepository)
    {
        _context = context;
        _accountRepository = accountRepository;
        _ledgerRepository = ledgerRepository;
    }

    public async Task<Transaction> TransferAsync(Guid fromAccountId, Guid toAccountId, decimal amount, string reference)
    {
        if (amount <= 0)
            throw new ArgumentException("Transfer amount must be greater than zero.", nameof(amount));

        if (fromAccountId == toAccountId)
            throw new ArgumentException("Cannot transfer to the same account.", nameof(toAccountId));

        var fromAccount = await _accountRepository.GetByIdAsync(fromAccountId);
        if (fromAccount == null)
            throw new InvalidOperationException($"Source account {fromAccountId} not found.");

        var toAccount = await _accountRepository.GetByIdAsync(toAccountId);
        if (toAccount == null)
            throw new InvalidOperationException($"Destination account {toAccountId} not found.");

        // Check if source account has sufficient balance
        var fromAccountBalance = await _accountRepository.GetBalanceAsync(fromAccountId);
        if (fromAccountBalance < amount)
            throw new InvalidOperationException(
                $"Insufficient funds. Current balance: {fromAccountBalance}, requested transfer: {amount}");

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
        await _context.Transactions.AddAsync(transaction);
        await _ledgerRepository.AddRangeAsync(new[] { debitEntry, creditEntry });
        await _ledgerRepository.SaveChangesAsync();

        return transaction;
    }
}
