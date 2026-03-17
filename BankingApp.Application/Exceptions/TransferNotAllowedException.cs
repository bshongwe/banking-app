namespace BankingApp.Application.Exceptions;

/// <summary>
/// Thrown when a transfer violates business rules (e.g., same-account transfer).
/// Maps to HTTP 422 Unprocessable Entity.
/// </summary>
public class TransferNotAllowedException : Exception
{
    public static BankingErrorCode ErrorCode => BankingErrorCode.TransferNotAllowed;

    public TransferNotAllowedException(string message) : base(message)
    {
    }
}
