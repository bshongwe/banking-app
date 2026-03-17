namespace BankingApp.Application.Exceptions;

/// <summary>
/// Thrown when attempting to perform an operation between accounts with different currencies.
/// Maps to HTTP 422 Unprocessable Entity.
/// </summary>
public class CurrencyMismatchException : Exception
{
    public string FromAccountCurrency { get; }
    public string ToAccountCurrency { get; }
    public static BankingErrorCode ErrorCode => BankingErrorCode.CurrencyMismatch;

    public CurrencyMismatchException(string fromAccountCurrency, string toAccountCurrency)
        : base("Cannot transfer between accounts with different currencies.")
    {
        FromAccountCurrency = fromAccountCurrency;
        ToAccountCurrency = toAccountCurrency;
    }
}
