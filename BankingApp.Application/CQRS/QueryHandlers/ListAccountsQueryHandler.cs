using BankingApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BankingApp.Application.CQRS.QueryHandlers;

public class ListAccountsQueryHandler
{
    private readonly BankingDbContext _context;

    public ListAccountsQueryHandler(BankingDbContext context)
    {
        _context = context;
    }

    public async Task<dynamic> HandleAsync(Queries.ListAccountsQuery query)
    {
        var baseQuery = _context.Accounts.AsQueryable();

        // Filter by customer if provided
        if (query.CustomerId.HasValue)
            baseQuery = baseQuery.Where(a => a.CustomerId == query.CustomerId.Value);

        var totalCount = await baseQuery.CountAsync();

        var accounts = await baseQuery
            .OrderBy(a => a.CreatedAt)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(a => new
            {
                a.Id,
                a.AccountNumber,
                a.Currency,
                a.CreatedAt,
                CustomerName = $"{a.Customer!.FirstName} {a.Customer.LastName}"
            })
            .ToListAsync();

        return new
        {
            items = accounts,
            pagination = new
            {
                pageNumber = query.PageNumber,
                pageSize = query.PageSize,
                totalCount = totalCount,
                totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize)
            }
        };
    }
}
