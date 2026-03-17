namespace BankingApp.Application.Exceptions;

/// <summary>
/// Thrown when attempting a transfer with an invalid amount (zero, negative, etc.).
/// Maps to HTTP 400 Bad Request.
/// </summary>
public class InvalidTransferAmountException : Exception
{
    public decimal Amount { get; }
    public static BankingErrorCode ErrorCode => BankingErrorCode.InvalidTransferAmount;

    public InvalidTransferAmountException(decimal amount)
        : base("Transfer amount must be greater than zero.")
    {
        Amount = amount;
    }
}
