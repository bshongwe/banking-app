namespace BankingApp.Application.Exceptions;

/// <summary>
/// Thrown when a business operation cannot be completed due to insufficient funds
/// </summary>
public class InsufficientFundsException : Exception
{
    public decimal CurrentBalance { get; }
    public decimal RequestedAmount { get; }

    public InsufficientFundsException(decimal currentBalance, decimal requestedAmount)
        : base("Insufficient funds.")
    {
        CurrentBalance = currentBalance;
        RequestedAmount = requestedAmount;
    }

    /// <summary>
    /// Returns detailed balance information for internal logging only.
    /// Do NOT expose this to clients.
    /// </summary>
    public string GetSensitiveDetails()
    {
        return $"Current balance: {CurrentBalance:C}, requested amount: {RequestedAmount:C}";
    }
}
