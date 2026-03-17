using BankingApp.Infrastructure.Data;
using BankingApp.Application.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace BankingApp.Application.CQRS.QueryHandlers;

public class GetCustomerQueryHandler
{
    private readonly BankingDbContext _context;

    public GetCustomerQueryHandler(BankingDbContext context)
    {
        _context = context;
    }

    public async Task<dynamic> HandleAsync(Queries.GetCustomerQuery query)
    {
        // Using LINQ to fetch customer with their accounts
        var customer = await _context.Customers
            .Where(c => c.Id == query.CustomerId)
            .Select(c => new
            {
                c.Id,
                c.FirstName,
                c.LastName,
                c.Email,
                c.CreatedAt,
                Accounts = c.Accounts.Select(a => new
                {
                    a.Id,
                    a.AccountNumber,
                    a.Currency,
                    a.CreatedAt
                })
            })
            .FirstOrDefaultAsync();

        if (customer == null)
            throw new ResourceNotFoundException("Customer", query.CustomerId);

        return customer;
    }
}
