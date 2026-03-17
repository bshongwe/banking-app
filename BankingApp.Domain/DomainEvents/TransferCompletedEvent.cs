using BankingApp.Domain.DomainEvents;

namespace BankingApp.Domain.DomainEvents;

public class TransferCompletedEvent : DomainEvent
{
    public Guid TransactionId { get; set; }
    public Guid FromAccountId { get; set; }
    public Guid ToAccountId { get; set; }
    public decimal Amount { get; set; }
    public string Reference { get; set; } = string.Empty;
}
