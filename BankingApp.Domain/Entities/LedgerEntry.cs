namespace BankingApp.Domain.Entities;

public class LedgerEntry
{
    public Guid Id { get; set; }
    public Guid? TransactionId { get; set; }
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
    public string EntryType { get; set; } = string.Empty; // "Debit" or "Credit"
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public Transaction? Transaction { get; set; }
    public Account? Account { get; set; }
}
