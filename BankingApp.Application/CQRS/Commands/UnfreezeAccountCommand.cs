namespace BankingApp.Application.CQRS.Commands;

public class UnfreezeAccountCommand
{
    public Guid AccountId { get; set; }
}
