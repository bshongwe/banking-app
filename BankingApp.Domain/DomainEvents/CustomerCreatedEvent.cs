using BankingApp.Domain.DomainEvents;

namespace BankingApp.Domain.DomainEvents;

public class CustomerCreatedEvent : DomainEvent
{
    public Guid CustomerId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
