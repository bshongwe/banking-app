using BankingApp.Domain.Entities;
using BankingApp.Infrastructure.Repositories;
using BankingApp.Application.Exceptions;

namespace BankingApp.Application.CQRS.CommandHandlers;

public class TransferMoneyCommandHandler
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILedgerRepository _ledgerRepository;
    private readonly ITransactionRepository _transactionRepository;

    public TransferMoneyCommandHandler(
        IAccountRepository accountRepository,
        ILedgerRepository ledgerRepository,
        ITransactionRepository transactionRepository)
    {
        _accountRepository = accountRepository;
        _ledgerRepository = ledgerRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<Transaction> HandleAsync(Commands.TransferMoneyCommand command)
    {
        if (command.Amount <= 0)
            throw new ArgumentException("Transfer amount must be greater than zero.");

        if (command.FromAccountId == command.ToAccountId)
            throw new ArgumentException("Cannot transfer to the same account.");

        var fromAccount = await _accountRepository.GetByIdAsync(command.FromAccountId);
        if (fromAccount == null)
            throw new ResourceNotFoundException("Account", command.FromAccountId);

        var toAccount = await _accountRepository.GetByIdAsync(command.ToAccountId);
        if (toAccount == null)
            throw new ResourceNotFoundException("Account", command.ToAccountId);

        // Check if source account has sufficient balance
        var fromAccountBalance = await _accountRepository.GetBalanceAsync(command.FromAccountId);
        if (fromAccountBalance < command.Amount)
            throw new InsufficientFundsException(fromAccountBalance, command.Amount);

        // Create transaction and ledger entries using domain logic
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            Reference = command.Reference,
            CreatedAt = DateTime.UtcNow
        };

        var debitEntry = new LedgerEntry
        {
            Id = Guid.NewGuid(),
            TransactionId = transaction.Id,
            AccountId = command.FromAccountId,
            Amount = command.Amount,
            EntryType = "Debit",
            CreatedAt = DateTime.UtcNow
        };

        var creditEntry = new LedgerEntry
        {
            Id = Guid.NewGuid(),
            TransactionId = transaction.Id,
            AccountId = command.ToAccountId,
            Amount = command.Amount,
            EntryType = "Credit",
            CreatedAt = DateTime.UtcNow
        };

        // Persist all data through repositories as a single unit of work
        await _transactionRepository.AddAsync(transaction);
        await _ledgerRepository.AddRangeAsync(new[] { debitEntry, creditEntry });
        await _transactionRepository.SaveChangesAsync();

        return transaction;
    }
}
