namespace BankingApp.Application.CQRS.Queries;

public class ListTransfersQuery
{
    public Guid? AccountId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
