namespace BankingApp.Application.CQRS.QueryHandlers;

/// <summary>
/// Data transfer object for a single ledger entry in an account's transaction history.
/// </summary>
public sealed class AccountTransactionHistoryItem
{
    public Guid Id { get; init; }
    public Guid? TransactionId { get; init; }
    public decimal Amount { get; init; }
    public string EntryType { get; init; } = default!;
    public DateTime CreatedAt { get; init; }
    public string? TransactionReference { get; init; }
}
