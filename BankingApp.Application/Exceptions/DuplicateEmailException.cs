namespace BankingApp.Application.Exceptions;

/// <summary>
/// Thrown when attempting to create a customer with a duplicate email address.
/// Maps to HTTP 409 Conflict.
/// </summary>
public class DuplicateEmailException : Exception
{
    public string Email { get; }
    public static BankingErrorCode ErrorCode => BankingErrorCode.DuplicateEmail;

    public DuplicateEmailException(string email)
        : base($"An account with email '{email}' already exists.")
    {
        Email = email;
    }
}
