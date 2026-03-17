using BankingApp.Domain.Entities;
using BankingApp.Infrastructure.Repositories;

namespace BankingApp.Application.CQRS.CommandHandlers;

public class CreateAccountCommandHandler
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILedgerRepository _ledgerRepository;

    public CreateAccountCommandHandler(
        IAccountRepository accountRepository,
        ILedgerRepository ledgerRepository)
    {
        _accountRepository = accountRepository;
        _ledgerRepository = ledgerRepository;
    }

    public async Task<Account> HandleAsync(Commands.CreateAccountCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.AccountNumber))
            throw new ArgumentException("Account number is required.");

        if (command.CustomerId == Guid.Empty)
            throw new ArgumentException("Customer ID is required.");

        // Check if account number already exists
        var existingAccount = await _accountRepository.GetByAccountNumberAsync(command.AccountNumber);
        if (existingAccount != null)
            throw new InvalidOperationException($"Account number {command.AccountNumber} already exists.");

        var account = new Account
        {
            Id = Guid.NewGuid(),
            CustomerId = command.CustomerId,
            AccountNumber = command.AccountNumber,
            Currency = command.Currency,
            CreatedAt = DateTime.UtcNow
        };

        await _accountRepository.AddAsync(account);
        await _accountRepository.SaveChangesAsync();

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
            await _ledgerRepository.SaveChangesAsync();
        }

        return account;
    }
}
