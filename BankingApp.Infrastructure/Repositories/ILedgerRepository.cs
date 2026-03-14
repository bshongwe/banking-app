using BankingApp.Domain.Entities;

namespace BankingApp.Infrastructure.Repositories;

public interface ILedgerRepository
{
    Task<LedgerEntry?> GetByIdAsync(Guid id);
    Task<IEnumerable<LedgerEntry>> GetByAccountIdAsync(Guid accountId);
    Task<IEnumerable<LedgerEntry>> GetByTransactionIdAsync(Guid transactionId);
    Task AddAsync(LedgerEntry entry);
    Task AddRangeAsync(IEnumerable<LedgerEntry> entries);
    Task SaveChangesAsync();
}
