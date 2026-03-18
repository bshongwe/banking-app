using BankingApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BankingApp.Application.CQRS.QueryHandlers;

public class ListTransfersQueryHandler
{
    private readonly BankingDbContext _context;

    public ListTransfersQueryHandler(BankingDbContext context)
    {
        _context = context;
    }

    public async Task<dynamic> HandleAsync(Queries.ListTransfersQuery query)
    {
        var baseQuery = _context.Transactions.AsQueryable();

        // Filter by account if provided (either source or destination)
        if (query.AccountId.HasValue)
        {
            var accountId = query.AccountId.Value;
            baseQuery = baseQuery.Where(t =>
                t.LedgerEntries.Any(le => le.AccountId == accountId));
        }

        var totalCount = await baseQuery.CountAsync();

        var transfers = await baseQuery
            .OrderByDescending(t => t.CreatedAt)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(t => new
            {
                t.Id,
                t.Reference,
                Amount = t.LedgerEntries
                    .Where(le => le.EntryType == "Debit")
                    .Sum(le => le.Amount),
                SourceAccountId = t.LedgerEntries
                    .Where(le => le.EntryType == "Debit")
                    .Select(le => le.AccountId)
                    .FirstOrDefault(),
                DestinationAccountId = t.LedgerEntries
                    .Where(le => le.EntryType == "Credit")
                    .Select(le => le.AccountId)
                    .FirstOrDefault(),
                t.CreatedAt
            })
            .ToListAsync();

        return new
        {
            items = transfers,
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
