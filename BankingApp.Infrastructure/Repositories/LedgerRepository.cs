using Microsoft.EntityFrameworkCore;
using BankingApp.Domain.Entities;

namespace BankingApp.Infrastructure.Repositories;

public class LedgerRepository : ILedgerRepository
{
    private readonly BankingDbContext _context;

    public LedgerRepository(BankingDbContext context)
    {
        _context = context;
    }

    public async Task<LedgerEntry?> GetByIdAsync(Guid id)
    {
        return await _context.LedgerEntries.FindAsync(id);
    }

    public async Task<IEnumerable<LedgerEntry>> GetByAccountIdAsync(Guid accountId)
    {
        return await _context.LedgerEntries
            .Where(le => le.AccountId == accountId)
            .OrderByDescending(le => le.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<LedgerEntry>> GetByTransactionIdAsync(Guid transactionId)
    {
        return await _context.LedgerEntries
            .Where(le => le.TransactionId == transactionId)
            .ToListAsync();
    }

    public async Task AddAsync(LedgerEntry entry)
    {
        await _context.LedgerEntries.AddAsync(entry);
    }

    public async Task AddRangeAsync(IEnumerable<LedgerEntry> entries)
    {
        await _context.LedgerEntries.AddRangeAsync(entries);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
