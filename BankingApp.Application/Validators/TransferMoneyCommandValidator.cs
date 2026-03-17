using BankingApp.Application.CQRS.Commands;
using FluentValidation;

namespace BankingApp.Application.Validators;

public class TransferMoneyCommandValidator : AbstractValidator<TransferMoneyCommand>
{
    public TransferMoneyCommandValidator()
    {
        RuleFor(x => x.FromAccountId)
            .NotEmpty()
            .WithMessage("Source account ID is required.");

        RuleFor(x => x.ToAccountId)
            .NotEmpty()
            .WithMessage("Destination account ID is required.")
            .NotEqual(x => x.FromAccountId)
            .WithMessage("Cannot transfer to the same account.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Transfer amount must be greater than zero.");

        RuleFor(x => x.Reference)
            .NotEmpty()
            .WithMessage("Transfer reference is required.")
            .MaximumLength(50)
            .WithMessage("Transfer reference cannot exceed 50 characters.");
    }
}
