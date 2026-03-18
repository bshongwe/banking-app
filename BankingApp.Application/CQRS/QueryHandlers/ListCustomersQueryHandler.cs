using BankingApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BankingApp.Application.CQRS.QueryHandlers;

public class ListCustomersQueryHandler
{
    private readonly BankingDbContext _context;

    public ListCustomersQueryHandler(BankingDbContext context)
    {
        _context = context;
    }

    public async Task<dynamic> HandleAsync(Queries.ListCustomersQuery query)
    {
        // Validate pagination parameters
        if (query.PageNumber <= 0)
            throw new ArgumentException("PageNumber must be greater than 0");
        
        if (query.PageSize <= 0 || query.PageSize > 100)
            throw new ArgumentException("PageSize must be between 1 and 100");

        var totalCount = await _context.Customers.CountAsync();

        var customers = await _context.Customers
            .OrderBy(c => c.CreatedAt)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(c => new
            {
                c.Id,
                c.FirstName,
                c.LastName,
                c.Email,
                c.CreatedAt
            })
            .ToListAsync();

        return new
        {
            items = customers,
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
