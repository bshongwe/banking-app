namespace BankingApp.Application.Exceptions;

/// <summary>
/// Thrown when attempting to create an account with a duplicate account number.
/// Maps to HTTP 409 Conflict.
/// </summary>
public class DuplicateAccountNumberException : Exception
{
    public string AccountNumber { get; }
    public static BankingErrorCode ErrorCode => BankingErrorCode.DuplicateAccountNumber;

    public DuplicateAccountNumberException(string accountNumber)
        : base("An account with this number already exists.")
    {
        AccountNumber = accountNumber;
    }
}
