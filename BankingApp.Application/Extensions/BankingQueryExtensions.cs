using BankingApp.Domain.Entities;

namespace BankingApp.Application.Extensions;

/// <summary>
/// LINQ extension methods for querying banking data
/// </summary>
public static class BankingQueryExtensions
{
    /// <summary>
    /// Gets all accounts for a customer using LINQ
    /// </summary>
    public static IQueryable<Account> WithCustomer(this IQueryable<Account> query, Guid customerId)
    {
        return query.Where(a => a.CustomerId == customerId);
    }

    /// <summary>
    /// Filters ledger entries by entry type using LINQ
    /// </summary>
    public static IQueryable<LedgerEntry> ByEntryType(this IQueryable<LedgerEntry> query, string entryType)
    {
        return query.Where(le => le.EntryType == entryType);
    }

    /// <summary>
    /// Gets ledger entries within a date range using LINQ
    /// </summary>
    public static IQueryable<LedgerEntry> WithinDateRange(
        this IQueryable<LedgerEntry> query,
        DateTime startDate,
        DateTime endDate)
    {
        return query.Where(le => le.CreatedAt >= startDate && le.CreatedAt <= endDate);
    }

    /// <summary>
    /// Calculates total debits for an account using LINQ aggregation
    /// </summary>
    public static async Task<decimal> GetTotalDebitsAsync(
        this IQueryable<LedgerEntry> query,
        Guid accountId)
    {
        return await query
            .Where(le => le.AccountId == accountId && le.EntryType == "Debit")
            .SumAsync(le => le.Amount);
    }

    /// <summary>
    /// Calculates total credits for an account using LINQ aggregation
    /// </summary>
    public static async Task<decimal> GetTotalCreditsAsync(
        this IQueryable<LedgerEntry> query,
        Guid accountId)
    {
        return await query
            .Where(le => le.AccountId == accountId && le.EntryType == "Credit")
            .SumAsync(le => le.Amount);
    }

    /// <summary>
    /// Groups transactions by date using LINQ grouping
    /// </summary>
    public static IQueryable<TransactionGroup> GroupByDate(
        this IQueryable<LedgerEntry> query,
        Guid accountId)
    {
        return query
            .Where(le => le.AccountId == accountId)
            .GroupBy(le => le.CreatedAt.Date)
            .Select(g => new TransactionGroup
            {
                Date = g.Key,
                TotalDebits = g.Where(le => le.EntryType == "Debit").Sum(le => le.Amount),
                TotalCredits = g.Where(le => le.EntryType == "Credit").Sum(le => le.Amount),
                TransactionCount = g.Count()
            })
            .OrderByDescending(tg => tg.Date);
    }
}

/// <summary>
/// DTO for transaction group summary
/// </summary>
public class TransactionGroup
{
    public DateTime Date { get; set; }
    public decimal TotalDebits { get; set; }
    public decimal TotalCredits { get; set; }
    public int TransactionCount { get; set; }
}
