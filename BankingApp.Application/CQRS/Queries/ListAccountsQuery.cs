namespace BankingApp.Application.CQRS.Queries;

public class ListAccountsQuery
{
    public Guid? CustomerId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
