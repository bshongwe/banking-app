using BankingApp.Domain.DomainEvents;

namespace BankingApp.Domain.DomainEvents;

public class AccountCreatedEvent : DomainEvent
{
    public Guid AccountId { get; set; }
    public Guid CustomerId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
}
