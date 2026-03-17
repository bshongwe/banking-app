namespace BankingApp.Application.Exceptions;

/// <summary>
/// Banking-specific domain error codes for standardized error responses.
/// These codes provide more granular information than HTTP status codes alone.
/// </summary>
public enum BankingErrorCode
{
    // Account errors
    AccountNotFound = 4001,
    AccountInvalidStatus = 4002,
    DuplicateAccountNumber = 4003,
    AccountFrozen = 4004,

    // Transaction errors
    InsufficientFunds = 4005,
    InvalidTransferAmount = 4006,
    TransferNotAllowed = 4007,
    CurrencyMismatch = 4008,
    DuplicateTransactionId = 4009,

    // Customer errors
    CustomerNotFound = 4010,
    DuplicateEmail = 4011,
    InvalidCustomerData = 4012,

    // Business rule violations
    DailyLimitExceeded = 4013,
    InvalidAccountBalance = 4014,

    // Validation errors
    ValidationFailed = 4000,

    // Rate limiting
    TooManyRequests = 4029,

    // Server errors
    InternalServerError = 5000,
    ServiceUnavailable = 5003,
}
