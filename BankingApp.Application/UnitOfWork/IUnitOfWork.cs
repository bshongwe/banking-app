using BankingApp.Infrastructure.Data;
using BankingApp.Infrastructure.Repositories;

namespace BankingApp.Application.UnitOfWork;

/// <summary>
/// Unit of Work pattern for coordinating multiple repositories
/// Works with Entity Framework Core
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IAccountRepository Accounts { get; }
    ILedgerRepository Ledgers { get; }
    Task<int> SaveChangesAsync();
    Task<bool> BeginTransactionAsync();
    Task<bool> CommitTransactionAsync();
    Task<bool> RollbackTransactionAsync();
}

public class UnitOfWork : IUnitOfWork
{
    private readonly BankingDbContext _context;
    private IAccountRepository? _accountRepository;
    private ILedgerRepository? _ledgerRepository;
    private bool _disposed = false;

    public UnitOfWork(BankingDbContext context)
    {
        _context = context;
    }

    public IAccountRepository Accounts =>
        _accountRepository ??= new AccountRepository(_context);

    public ILedgerRepository Ledgers =>
        _ledgerRepository ??= new LedgerRepository(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task<bool> BeginTransactionAsync()
    {
        try
        {
            await _context.Database.BeginTransactionAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> CommitTransactionAsync()
    {
        try
        {
            await _context.Database.CommitTransactionAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RollbackTransactionAsync()
    {
        try
        {
            await _context.Database.RollbackTransactionAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _context?.Dispose();
            }
            _disposed = true;
        }
    }
}
