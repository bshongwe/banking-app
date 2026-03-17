namespace BankingApp.Application.Exceptions;

/// <summary>
/// Thrown when a business operation cannot be completed due to insufficient resources or invalid state
/// </summary>
public class InsufficientFundsException : Exception
{
    public decimal CurrentBalance { get; }
    public decimal RequestedAmount { get; }

    public InsufficientFundsException(decimal currentBalance, decimal requestedAmount)
        : base($"Insufficient funds. Current balance: {currentBalance:C}, requested amount: {requestedAmount:C}")
    {
        CurrentBalance = currentBalance;
        RequestedAmount = requestedAmount;
    }
}
