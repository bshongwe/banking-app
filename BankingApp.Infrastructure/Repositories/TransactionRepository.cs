using BankingApp.Domain.Entities;
using BankingApp.Infrastructure.Data;

namespace BankingApp.Infrastructure.Repositories;

/// <summary>
/// Concrete implementation of ITransactionRepository using Entity Framework Core.
/// </summary>
public class TransactionRepository(BankingDbContext context) : ITransactionRepository
{

    public async Task<Transaction> AddAsync(Transaction transaction)
    {
        await context.Transactions.AddAsync(transaction);
        return transaction;
    }

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }
}
