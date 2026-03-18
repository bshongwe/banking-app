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
        // Validate pagination parameters
        if (query.PageNumber <= 0)
            throw new ArgumentException("PageNumber must be greater than 0");
        
        if (query.PageSize <= 0 || query.PageSize > 100)
            throw new ArgumentException("PageSize must be between 1 and 100");

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
                CustomerName = a.Customer != null 
                    ? $"{a.Customer.FirstName} {a.Customer.LastName}".Trim()
                    : "Unknown"
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
