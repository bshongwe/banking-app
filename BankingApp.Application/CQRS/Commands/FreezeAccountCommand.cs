namespace BankingApp.Application.CQRS.Commands;

public class FreezeAccountCommand
{
    public Guid AccountId { get; set; }
}
