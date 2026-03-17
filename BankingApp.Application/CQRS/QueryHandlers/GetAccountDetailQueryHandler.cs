using BankingApp.Infrastructure.Data;
using BankingApp.Application.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace BankingApp.Application.CQRS.QueryHandlers;

public class GetAccountDetailQueryHandler
{
    private readonly BankingDbContext _context;

    public GetAccountDetailQueryHandler(BankingDbContext context)
    {
        _context = context;
    }

    public async Task<dynamic?> HandleAsync(Queries.GetAccountDetailQuery query)
    {
        // Using LINQ with Entity Framework
        var account = await _context.Accounts
            .Where(a => a.Id == query.AccountId)
            .Select(a => new
            {
                a.Id,
                a.AccountNumber,
                a.Currency,
                a.CreatedAt,
                CustomerName = $"{a.Customer!.FirstName} {a.Customer.LastName}",
                a.Customer!.Email
            })
            .FirstOrDefaultAsync();

        if (account == null)
            throw new ResourceNotFoundException("Account", query.AccountId);

        return account;
    }
}
