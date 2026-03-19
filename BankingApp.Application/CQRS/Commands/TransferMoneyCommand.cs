namespace BankingApp.Application.CQRS.Commands;

public class TransferMoneyCommand
{
    public Guid FromAccountId { get; set; }
    public Guid ToAccountId { get; set; }
    public decimal Amount { get; set; }
    public string Reference { get; set; } = string.Empty;

    /// <summary>
    /// Payment gateway to use for external transfers.
    /// Supported values: "internal", "stripe", "paypal", "sa_banks"
    /// Default: "internal" (double-entry bookkeeping only)
    /// </summary>
    public string PaymentGateway { get; set; } = "internal";

    /// <summary>
    /// Account type for external transfers (e.g. checking, savings, business)
    /// </summary>
    public string? SourceAccountType { get; set; }

    /// <summary>
    /// Bank code for South African bank transfers (sa_banks gateway)
    /// </summary>
    public string? DestinationBankCode { get; set; }

    /// <summary>
    /// Routing number for US ACH transfers (Stripe gateway)
    /// </summary>
    public string? DestinationRoutingNumber { get; set; }

    /// <summary>
    /// Transfer urgency for SA banks.
    /// "NORMAL" (EFT - 1 business day) or "URGENT" (RTGS - real-time)
    /// </summary>
    public string Urgency { get; set; } = "NORMAL";

    /// <summary>
    /// Additional metadata passed to the payment gateway
    /// </summary>
    public Dictionary<string, string> GatewayMetadata { get; set; } = new();
}

/// <summary>
/// Transfer result including payment gateway status
/// </summary>
public class TransferResult
{
    public Guid TransactionId { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // COMPLETED, PENDING, FAILED
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Payment gateway used for the transfer
    /// </summary>
    public string PaymentGateway { get; set; } = string.Empty;

    /// <summary>
    /// External transaction ID from payment gateway (if applicable)
    /// </summary>
    public string? ExternalTransactionId { get; set; }

    /// <summary>
    /// Transfer method for SA banks (EFT or RTGS)
    /// </summary>
    public string? TransferMethod { get; set; }

    /// <summary>
    /// Estimated completion time
    /// </summary>
    public DateTime? EstimatedCompletionTime { get; set; }

    /// <summary>
    /// Error message if transfer failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}
