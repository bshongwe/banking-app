namespace BankingApp.Application.CQRS.Queries;

public class GetAccountBalanceQuery
{
    public Guid AccountId { get; set; }
}
