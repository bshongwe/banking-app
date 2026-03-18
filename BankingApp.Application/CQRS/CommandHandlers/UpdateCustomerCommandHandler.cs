using BankingApp.Domain.Entities;
using BankingApp.Infrastructure.Data;
using BankingApp.Application.Exceptions;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace BankingApp.Application.CQRS.CommandHandlers;

public class UpdateCustomerCommandHandler
{
    private readonly BankingDbContext _context;

    public UpdateCustomerCommandHandler(BankingDbContext context)
    {
        _context = context;
    }

    public async Task<Customer> HandleAsync(Commands.UpdateCustomerCommand command)
    {
        if (command.CustomerId == Guid.Empty)
            throw new ArgumentException("Customer ID is required.");

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

        var customer = await _context.Customers.FindAsync(command.CustomerId);
        if (customer == null)
            throw new ResourceNotFoundException("Customer");

        // Check for duplicate email if email has changed
        if (customer.Email != command.Email)
        {
            var existingCustomer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == command.Email);
            if (existingCustomer != null)
                throw new DuplicateEmailException(command.Email);
        }

        customer.FirstName = command.FirstName;
        customer.LastName = command.LastName;
        customer.Email = command.Email;

        try
        {
            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase) ?? false)
        {
            throw new DuplicateEmailException(command.Email);
        }

        return customer;
    }
}
