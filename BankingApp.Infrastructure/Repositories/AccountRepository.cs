using Microsoft.EntityFrameworkCore;
using BankingApp.Domain.Entities;

namespace BankingApp.Infrastructure.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly BankingDbContext _context;

    public AccountRepository(BankingDbContext context)
    {
        _context = context;
    }

    public async Task<Account?> GetByIdAsync(Guid id)
    {
        return await _context.Accounts.FindAsync(id);
    }

    public async Task<Account?> GetByAccountNumberAsync(string accountNumber)
    {
        return await _context.Accounts.FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);
    }

    public async Task<IEnumerable<Account>> GetByCustomerIdAsync(Guid customerId)
    {
        return await _context.Accounts
            .Where(a => a.CustomerId == customerId)
            .ToListAsync();
    }

    public async Task<decimal> GetBalanceAsync(Guid accountId)
    {
        var balance = await _context.LedgerEntries
            .Where(le => le.AccountId == accountId)
            .GroupBy(le => le.AccountId)
            .Select(g => new
            {
                Debits = g.Where(le => le.EntryType == "Debit").Sum(le => le.Amount),
                Credits = g.Where(le => le.EntryType == "Credit").Sum(le => le.Amount)
            })
            .FirstOrDefaultAsync();

        if (balance == null)
            return 0m;

        // Balance = Credits - Debits (in banking, credits increase balance, debits decrease it)
        return balance.Credits - balance.Debits;
    }

    public async Task AddAsync(Account account)
    {
        await _context.Accounts.AddAsync(account);
    }

    public async Task UpdateAsync(Account account)
    {
        _context.Accounts.Update(account);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
