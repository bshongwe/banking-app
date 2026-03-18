namespace BankingApp.Application.DTOs;

/// <summary>
/// Data transfer object for account information.
/// Excludes circular references to prevent serialization issues.
/// </summary>
public class AccountDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string Currency { get; set; } = "USD";
    public string Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; }
}
