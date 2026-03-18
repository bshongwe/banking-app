namespace BankingApp.Application.CQRS.Queries;

public class ListCustomersQuery
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
