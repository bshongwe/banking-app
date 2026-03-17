namespace BankingApp.Application.CQRS.Commands;

public class CreateAccountCommand
{
    public Guid CustomerId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string Currency { get; set; } = "USD";
    public decimal InitialBalance { get; set; } = 0;
}
