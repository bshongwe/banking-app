namespace BankingApp.Application.Exceptions;

/// <summary>
/// Thrown when a business operation cannot be completed due to insufficient funds
/// </summary>
public class InsufficientFundsException : Exception
{
    /// <summary>
    /// The current account balance. Internal use only.
    /// </summary>
    internal decimal CurrentBalance { get; }

    /// <summary>
    /// The requested transfer amount. Internal use only.
    /// </summary>
    internal decimal RequestedAmount { get; }

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
