using BankingApp.Domain.Entities;
using BankingApp.Infrastructure.Data;

namespace BankingApp.Infrastructure.Repositories;

/// <summary>
/// Concrete implementation of ITransactionRepository using Entity Framework Core.
/// </summary>
public class TransactionRepository : ITransactionRepository
{
    private readonly BankingDbContext _context;

    public TransactionRepository(BankingDbContext context)
    {
        _context = context;
    }

    public async Task<Transaction> AddAsync(Transaction transaction)
    {
        await _context.Transactions.AddAsync(transaction);
        return transaction;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
