using BankingApp.Domain.Entities;

namespace BankingApp.Infrastructure.Repositories;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id);
    Task<Account?> GetByAccountNumberAsync(string accountNumber);
    Task<IEnumerable<Account>> GetByCustomerIdAsync(Guid customerId);
    Task<decimal> GetBalanceAsync(Guid accountId);
    Task AddAsync(Account account);
    Task UpdateAsync(Account account);
    Task SaveChangesAsync();
}
