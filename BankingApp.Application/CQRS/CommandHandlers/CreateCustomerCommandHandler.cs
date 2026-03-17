using BankingApp.Domain.Entities;
using BankingApp.Infrastructure.Data;
using BankingApp.Application.Exceptions;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

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

        try
        {
            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase) ?? false)
        {
            // Handle unique constraint violation on Email
            throw new DuplicateEmailException(command.Email);
        }

        return customer;
    }
}
