namespace BankingApp.Application.Exceptions;

/// <summary>
/// Thrown when attempting an operation on a frozen or inactive account.
/// Maps to HTTP 422 Unprocessable Entity.
/// </summary>
public class AccountFrozenException : Exception
{
    public Guid AccountId { get; }
    public static BankingErrorCode ErrorCode => BankingErrorCode.AccountFrozen;

    public AccountFrozenException(Guid accountId)
        : base("This account is frozen and cannot process transactions.")
    {
        AccountId = accountId;
    }
}
