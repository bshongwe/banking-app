using BankingApp.Domain.Entities;
using BankingApp.Application.Exceptions;
using BankingApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BankingApp.Application.CQRS.CommandHandlers;

public class UpdateAccountCommandHandler
{
    private readonly BankingDbContext _context;

    public UpdateAccountCommandHandler(BankingDbContext context)
    {
        _context = context;
    }

    public async Task<Account> HandleAsync(Commands.UpdateAccountCommand command)
    {
        if (command.AccountId == Guid.Empty)
            throw new ArgumentException("Account ID is required.");

        if (string.IsNullOrWhiteSpace(command.AccountNumber))
            throw new ArgumentException("Account number is required.");

        var account = await _context.Accounts.FindAsync(command.AccountId);
        if (account == null)
            throw new ResourceNotFoundException("Account", command.AccountId);

        // Check for duplicate account number if it has changed
        if (account.AccountNumber != command.AccountNumber)
        {
            var existingAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccountNumber == command.AccountNumber);
            if (existingAccount != null)
                throw new DuplicateAccountNumberException(command.AccountNumber);
        }

        account.AccountNumber = command.AccountNumber;

        try
        {
            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase) ?? false)
        {
            throw new DuplicateAccountNumberException(command.AccountNumber);
        }

        return account;
    }
}
