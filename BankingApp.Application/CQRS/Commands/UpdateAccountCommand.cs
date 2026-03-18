namespace BankingApp.Application.CQRS.Commands;

public class UpdateAccountCommand
{
    public Guid AccountId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
}
