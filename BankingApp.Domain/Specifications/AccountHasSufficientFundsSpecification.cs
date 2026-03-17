using BankingApp.Domain.Entities;

namespace BankingApp.Domain.Specifications;

public class AccountHasSufficientFundsSpecification
{
    private readonly decimal _requiredAmount;

    public AccountHasSufficientFundsSpecification(decimal requiredAmount)
    {
        _requiredAmount = requiredAmount;
    }

    public bool IsSatisfiedBy(decimal currentBalance)
    {
        return currentBalance >= _requiredAmount;
    }
}
