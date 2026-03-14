namespace BankingApp.Domain.Entities;

public class Transaction
{
    public Guid Id { get; set; }
    public string Reference { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // Navigation property
    public ICollection<LedgerEntry> LedgerEntries { get; set; } = new List<LedgerEntry>();
}
