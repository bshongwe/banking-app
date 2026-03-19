using Microsoft.EntityFrameworkCore;
using BankingApp.Domain.Entities;
using BankingApp.Infrastructure.Data;

namespace BankingApp.Infrastructure.Repositories;

public class AccountRepository(BankingDbContext context) : IAccountRepository
{

    public async Task<Account?> GetByIdAsync(Guid id)
    {
        return await context.Accounts.FindAsync(id);
    }

    public async Task<Account?> GetByAccountNumberAsync(string accountNumber)
    {
        return await context.Accounts.FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);
    }

    public async Task<IEnumerable<Account>> GetByCustomerIdAsync(Guid customerId)
    {
        return await context.Accounts
            .Where(a => a.CustomerId == customerId)
            .ToListAsync();
    }

    public async Task<decimal> GetBalanceAsync(Guid accountId)
    {
        var balance = await context.LedgerEntries
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
        await context.Accounts.AddAsync(account);
    }

    public async Task UpdateAsync(Account account)
    {
        context.Accounts.Update(account);
    }

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }
}
