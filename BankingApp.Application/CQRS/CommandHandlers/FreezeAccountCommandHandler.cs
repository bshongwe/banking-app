using BankingApp.Domain.Entities;
using BankingApp.Infrastructure.Data;
using BankingApp.Application.Exceptions;
using BankingApp.Application.DTOs;

namespace BankingApp.Application.CQRS.CommandHandlers;

public class FreezeAccountCommandHandler
{
    private readonly BankingDbContext _context;

    public FreezeAccountCommandHandler(BankingDbContext context)
    {
        _context = context;
    }

    public async Task<AccountDto> HandleAsync(Commands.FreezeAccountCommand command)
    {
        if (command.AccountId == Guid.Empty)
            throw new ArgumentException("Account ID is required.");

        var account = await _context.Accounts.FindAsync(command.AccountId);
        if (account == null)
            throw new ResourceNotFoundException("Account", command.AccountId);

        if (account.Status == "Frozen")
            throw new InvalidOperationException("Account is already frozen.");

        if (account.Status == "Closed")
            throw new InvalidOperationException("Cannot freeze a closed account.");

        account.Status = "Frozen";

        await _context.SaveChangesAsync();

        return new AccountDto
        {
            Id = account.Id,
            CustomerId = account.CustomerId,
            AccountNumber = account.AccountNumber,
            Currency = account.Currency,
            Status = account.Status,
            CreatedAt = account.CreatedAt
        };
    }
}
