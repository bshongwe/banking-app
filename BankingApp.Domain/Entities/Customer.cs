namespace BankingApp.Domain.Entities;

public class Customer
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // Navigation property
    public ICollection<Account> Accounts { get; set; } = new List<Account>();
}
