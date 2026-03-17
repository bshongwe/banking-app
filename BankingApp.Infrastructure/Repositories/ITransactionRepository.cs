using BankingApp.Domain.Entities;

namespace BankingApp.Infrastructure.Repositories;

public interface ITransactionRepository
{
    Task<Transaction> AddAsync(Transaction transaction);
    Task SaveChangesAsync();
}
