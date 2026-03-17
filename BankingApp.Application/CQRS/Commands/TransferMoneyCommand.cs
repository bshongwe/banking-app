namespace BankingApp.Application.CQRS.Commands;

public class TransferMoneyCommand
{
    public Guid FromAccountId { get; set; }
    public Guid ToAccountId { get; set; }
    public decimal Amount { get; set; }
    public string Reference { get; set; } = string.Empty;
}
