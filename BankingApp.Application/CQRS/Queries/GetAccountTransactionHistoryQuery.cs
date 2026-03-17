namespace BankingApp.Application.CQRS.Queries;

public class GetAccountTransactionHistoryQuery
{
    public Guid AccountId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
