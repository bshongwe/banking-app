namespace BankingApp.Application.DTOs;

/// <summary>
/// Standardized error response for all API error cases.
/// Includes both HTTP status codes and banking-specific domain error codes.
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Human-readable error message (generic for security)
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Banking-specific error code for programmatic handling
    /// </summary>
    public int ErrorCode { get; set; }

    /// <summary>
    /// HTTP status code
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Unique error identifier for support/logging purposes
    /// </summary>
    public string TraceId { get; set; } = string.Empty;

    /// <summary>
    /// Field-level validation errors (for 400 Bad Request)
    /// </summary>
    public Dictionary<string, string[]>? ValidationErrors { get; set; }

    /// <summary>
    /// Additional contextual information (e.g., retry-after for rate limits)
    /// </summary>
    public Dictionary<string, object>? Details { get; set; }

    public ErrorResponse()
    {
    }

    public ErrorResponse(string message, int errorCode, int statusCode, string traceId)
    {
        Message = message;
        ErrorCode = errorCode;
        StatusCode = statusCode;
        TraceId = traceId;
    }
}
