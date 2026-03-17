using Microsoft.EntityFrameworkCore;
using BankingApp.Domain.Entities;

namespace BankingApp.Infrastructure.Data;

public class BankingDbContext : DbContext
{
    /// <summary>
    /// SQLite CURRENT_TIMESTAMP provides second-resolution precision.
    /// For microsecond precision and deterministic ordering in high-frequency transactions,
    /// CreatedAt is set application-side using DateTime.UtcNow (100-nanosecond precision),
    /// with secondary ordering by ID for transactions in the same second.
    /// </summary>
    private const string SqlGetUtcDateFunction = "CURRENT_TIMESTAMP";

    public BankingDbContext(DbContextOptions<BankingDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<LedgerEntry> LedgerEntries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Customer
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(200).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql(SqlGetUtcDateFunction);
        });

        // Configure Account
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AccountNumber).HasMaxLength(20).IsRequired();
            entity.HasIndex(e => e.AccountNumber).IsUnique();
            entity.Property(e => e.Currency).HasMaxLength(3).HasDefaultValue("USD");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql(SqlGetUtcDateFunction);
            
            entity.HasOne(e => e.Customer)
                .WithMany(c => c.Accounts)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Transaction
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Reference).HasMaxLength(50).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql(SqlGetUtcDateFunction);
        });

        // Configure LedgerEntry
        modelBuilder.Entity<LedgerEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.EntryType).HasMaxLength(10).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql(SqlGetUtcDateFunction);
            
            entity.HasOne(e => e.Transaction)
                .WithMany(t => t.LedgerEntries)
                .HasForeignKey(e => e.TransactionId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Account)
                .WithMany(a => a.LedgerEntries)
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
