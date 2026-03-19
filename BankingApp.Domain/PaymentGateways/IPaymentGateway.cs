namespace BankingApp.Domain.PaymentGateways;

/// <summary>
/// Abstraction for payment gateway providers.
/// Enables pluggable payment processing (Stripe, PayPal, South African Banks, etc.)
/// Defined in Domain so both Application and Infrastructure can reference it without circular dependencies.
/// </summary>
public interface IPaymentGateway
{
    /// <summary>
    /// Unique identifier for the payment gateway provider
    /// </summary>
    string ProviderId { get; }

    /// <summary>
    /// Process a payment/transfer through the external gateway
    /// </summary>
    Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request);

    /// <summary>
    /// Check the status of a previously initiated payment
    /// </summary>
    Task<PaymentStatusResult> GetPaymentStatusAsync(string transactionId);

    /// <summary>
    /// Validate that the gateway is properly configured and accessible
    /// </summary>
    Task<bool> ValidateConfigurationAsync();
}

/// <summary>
/// Request to process a payment through a gateway
/// </summary>
public class PaymentRequest
{
    public string Reference { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";

    // Source account details
    public string SourceAccountNumber { get; set; } = string.Empty;
    public string SourceBankCode { get; set; } = string.Empty;
    public string SourceAccountHolder { get; set; } = string.Empty;

    /// <summary>
    /// Account type for the source account (e.g. checking, savings, business)
    /// Required for external payment gateways
    /// </summary>
    public string SourceAccountType { get; set; } = string.Empty;

    // Destination account details
    public string DestinationAccountNumber { get; set; } = string.Empty;
    public string DestinationBankCode { get; set; } = string.Empty;
    public string DestinationAccountHolder { get; set; } = string.Empty;

    // Additional metadata
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// Response from payment gateway after processing
/// </summary>
public class PaymentResult
{
    public bool Success { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // PENDING, COMPLETED, FAILED
    public decimal Amount { get; set; }
    public DateTime ProcessedAt { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
}

/// <summary>
/// Payment status information from gateway
/// </summary>
public class PaymentStatusResult
{
    public string TransactionId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}
