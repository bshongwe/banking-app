using BankingApp.Application.CQRS.Commands;
using FluentValidation;

namespace BankingApp.Application.Validators;

public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("Customer ID is required.");

        RuleFor(x => x.AccountNumber)
            .NotEmpty()
            .WithMessage("Account number is required.")
            .Length(10, 20)
            .WithMessage("Account number must be between 10 and 20 characters.");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Currency is required.")
            .Length(3)
            .WithMessage("Currency must be a 3-letter ISO code.");
    }
}
