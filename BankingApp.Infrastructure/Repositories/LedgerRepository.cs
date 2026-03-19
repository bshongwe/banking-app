using Microsoft.EntityFrameworkCore;
using BankingApp.Domain.Entities;
using BankingApp.Infrastructure.Data;

namespace BankingApp.Infrastructure.Repositories;

public class LedgerRepository(BankingDbContext context) : ILedgerRepository
{

    public async Task<LedgerEntry?> GetByIdAsync(Guid id)
    {
        return await context.LedgerEntries.FindAsync(id);
    }

    public async Task<IEnumerable<LedgerEntry>> GetByAccountIdAsync(Guid accountId)
    {
        return await context.LedgerEntries
            .Where(le => le.AccountId == accountId)
            .OrderByDescending(le => le.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<LedgerEntry>> GetByTransactionIdAsync(Guid transactionId)
    {
        return await context.LedgerEntries
            .Where(le => le.TransactionId == transactionId)
            .ToListAsync();
    }

    public async Task AddAsync(LedgerEntry entry)
    {
        await context.LedgerEntries.AddAsync(entry);
    }

    public async Task AddRangeAsync(IEnumerable<LedgerEntry> entries)
    {
        await context.LedgerEntries.AddRangeAsync(entries);
    }

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }
}
