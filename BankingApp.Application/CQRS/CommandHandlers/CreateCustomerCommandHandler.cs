using BankingApp.Domain.Entities;
using BankingApp.Infrastructure.Data;
using System.ComponentModel.DataAnnotations;

namespace BankingApp.Application.CQRS.CommandHandlers;

public class CreateCustomerCommandHandler
{
    private readonly BankingDbContext _context;

    public CreateCustomerCommandHandler(BankingDbContext context)
    {
        _context = context;
    }

    public async Task<Customer> HandleAsync(Commands.CreateCustomerCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.FirstName))
            throw new ArgumentException("First name is required.");

        if (string.IsNullOrWhiteSpace(command.LastName))
            throw new ArgumentException("Last name is required.");

        if (string.IsNullOrWhiteSpace(command.Email))
            throw new ArgumentException("Email is required.");

        // Validate email format
        var emailAttribute = new EmailAddressAttribute();
        if (!emailAttribute.IsValid(command.Email))
            throw new ArgumentException("Invalid email format.");

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = command.FirstName,
            LastName = command.LastName,
            Email = command.Email,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();

        return customer;
    }
}
