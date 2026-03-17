using BankingApp.Infrastructure.Data;
using BankingApp.Application.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace BankingApp.Application.CQRS.QueryHandlers;

public class GetAccountTransactionHistoryQueryHandler
{
    private readonly BankingDbContext _context;

    public GetAccountTransactionHistoryQueryHandler(BankingDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult> HandleAsync(Queries.GetAccountTransactionHistoryQuery query)
    {
        // Verify the account exists first - throw 404 if not
        var accountExists = await _context.Accounts
            .AnyAsync(a => a.Id == query.AccountId);
        
        if (!accountExists)
            throw new ResourceNotFoundException("Account", query.AccountId);

        // Using LINQ with pagination
        var totalCount = await _context.LedgerEntries
            .Where(le => le.AccountId == query.AccountId)
            .CountAsync();

        var entries = await _context.LedgerEntries
            .Where(le => le.AccountId == query.AccountId)
            .OrderByDescending(le => le.CreatedAt)
            .ThenByDescending(le => le.Id)  // Secondary sort by ID for deterministic ordering in same second
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(le => new
            {
                le.Id,
                le.TransactionId,
                le.Amount,
                le.EntryType,
                le.CreatedAt,
                TransactionReference = le.Transaction != null ? le.Transaction.Reference : null
            })
            .ToListAsync();

        return new PaginatedResult
        {
            Items = entries,
            TotalCount = totalCount,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }
}

public class PaginatedResult
{
    public IEnumerable<dynamic> Items { get; set; } = new List<dynamic>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
