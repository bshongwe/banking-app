using BankingApp.Domain.Entities;
using BankingApp.Infrastructure.Repositories;
using BankingApp.Application.UnitOfWork;

namespace BankingApp.Application.CQRS.CommandHandlers;

public class CreateAccountCommandHandler
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILedgerRepository _ledgerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAccountCommandHandler(
        IAccountRepository accountRepository,
        ILedgerRepository ledgerRepository,
        IUnitOfWork unitOfWork)
    {
        _accountRepository = accountRepository;
        _ledgerRepository = ledgerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Account> HandleAsync(Commands.CreateAccountCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.AccountNumber))
            throw new ArgumentException("Account number is required.");

        if (command.CustomerId == Guid.Empty)
            throw new ArgumentException("Customer ID is required.");

        if (command.InitialBalance < 0)
            throw new ArgumentException("Initial balance cannot be negative.");

        // Check if account number already exists
        var existingAccount = await _accountRepository.GetByAccountNumberAsync(command.AccountNumber);
        if (existingAccount != null)
            throw new InvalidOperationException($"Account number {command.AccountNumber} already exists.");

        // Begin transaction to ensure account and initial balance are persisted atomically
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var account = new Account
            {
                Id = Guid.NewGuid(),
                CustomerId = command.CustomerId,
                AccountNumber = command.AccountNumber,
                Currency = command.Currency,
                CreatedAt = DateTime.UtcNow
            };

            // Add account to transaction
            await _accountRepository.AddAsync(account);

            // If initial balance is provided, create a ledger entry
            if (command.InitialBalance > 0)
            {
                var initialBalanceEntry = new LedgerEntry
                {
                    Id = Guid.NewGuid(),
                    AccountId = account.Id,
                    Amount = command.InitialBalance,
                    EntryType = "Credit",
                    CreatedAt = DateTime.UtcNow
                };

                await _ledgerRepository.AddAsync(initialBalanceEntry);
            }

            // Commit both account and ledger entry atomically
            await _accountRepository.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return account;
        }
        catch
        {
            // Rollback transaction on any error to maintain data consistency
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
