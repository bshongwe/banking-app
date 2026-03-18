namespace BankingApp.Domain.Entities;

public class Account
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string Currency { get; set; } = "USD";
    public string Status { get; set; } = "Active"; // Active, Frozen, Closed
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public Customer? Customer { get; set; }
    public ICollection<LedgerEntry> LedgerEntries { get; set; } = new List<LedgerEntry>();
}
