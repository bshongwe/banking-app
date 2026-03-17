using BankingApp.Domain.Entities;
using BankingApp.Infrastructure.Repositories;
using BankingApp.Application.Exceptions;
using BankingApp.Application.UnitOfWork;

namespace BankingApp.Application.CQRS.CommandHandlers;

public class TransferMoneyCommandHandler
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILedgerRepository _ledgerRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TransferMoneyCommandHandler(
        IAccountRepository accountRepository,
        ILedgerRepository ledgerRepository,
        ITransactionRepository transactionRepository,
        IUnitOfWork unitOfWork)
    {
        _accountRepository = accountRepository;
        _ledgerRepository = ledgerRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Transaction> HandleAsync(Commands.TransferMoneyCommand command)
    {
        // Validate transfer amount first
        if (command.Amount <= 0)
            throw new InvalidTransferAmountException(command.Amount);

        if (command.FromAccountId == command.ToAccountId)
            throw new TransferNotAllowedException("Cannot transfer to the same account.");

        // Start database transaction to prevent concurrent race conditions on balance checks
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var fromAccount = await _accountRepository.GetByIdAsync(command.FromAccountId);
            if (fromAccount == null)
                throw new ResourceNotFoundException("Account", command.FromAccountId);

            var toAccount = await _accountRepository.GetByIdAsync(command.ToAccountId);
            if (toAccount == null)
                throw new ResourceNotFoundException("Account", command.ToAccountId);

            // Verify currencies match before transfer
            if (fromAccount.Currency != toAccount.Currency)
                throw new CurrencyMismatchException(fromAccount.Currency, toAccount.Currency);

            // Check if source account has sufficient balance
            // This check is now atomic with the persistence below due to transaction wrapping
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

            // Persist all data through repositories within the transaction scope
            await _transactionRepository.AddAsync(transaction);
            await _ledgerRepository.AddRangeAsync(new[] { debitEntry, creditEntry });
            await _transactionRepository.SaveChangesAsync();

            // Commit the transaction after all writes succeed and verify commit succeeded
            var commitSucceeded = await _unitOfWork.CommitTransactionAsync();
            if (!commitSucceeded)
            {
                throw new InvalidOperationException("Failed to commit the transfer transaction. The operation was rolled back.");
            }

            return transaction;
        }
        catch
        {
            // Rollback transaction on any error to maintain data consistency
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
