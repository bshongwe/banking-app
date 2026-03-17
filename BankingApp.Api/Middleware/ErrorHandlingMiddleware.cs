using System.Net;
using BankingApp.Application.DTOs;
using BankingApp.Application.Exceptions;

namespace BankingApp.Api.Middleware;

/// <summary>
/// Global exception handling middleware that converts domain exceptions to standardized error responses.
/// This middleware ensures consistent error handling across all endpoints.
/// Logs expected errors (4xx) at Information level and unexpected errors (5xx) at Error level.
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            if (context.Response.HasStarted)
            {
                _logger.LogWarning(ex, "Unhandled exception after response started; skipping error body: {Method} {Path}", 
                    context.Request.Method, context.Request.Path);
                throw;
            }

            await HandleExceptionAsync(context, ex, _logger);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception, ILogger<ErrorHandlingMiddleware> logger)
    {
        // If response has already started, we cannot modify headers or status code
        if (context.Response.HasStarted)
        {
            logger.LogWarning(exception, "Exception occurred after response had already started: {Method} {Path}", 
                context.Request.Method, context.Request.Path);
            return Task.CompletedTask;
        }

        context.Response.ContentType = "application/json";

        var response = new ErrorResponse
        {
            Message = "An error occurred.",
            TraceId = context.TraceIdentifier
        };

        var path = context.Request.Path;
        var method = context.Request.Method;

        switch (exception)
        {
            case ResourceNotFoundException ex:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                response.StatusCode = StatusCodes.Status404NotFound;
                // Map ResourceType to appropriate error code
                response.ErrorCode = (int)(ex.ResourceType?.ToLower() switch
                {
                    "customer" => BankingErrorCode.CustomerNotFound,
                    "account" => BankingErrorCode.AccountNotFound,
                    _ => BankingErrorCode.ValidationFailed
                });
                // Generic message - don't expose resource IDs
                response.Message = $"{ex.ResourceType ?? "Resource"} not found.";
                logger.LogInformation("Resource not found: {Method} {Path} - Code: {ErrorCode}", method, path, response.ErrorCode);
                break;

            case TransferNotAllowedException:
                context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                response.ErrorCode = (int)BankingErrorCode.TransferNotAllowed;
                response.Message = "This transfer cannot be completed.";
                logger.LogInformation("Transfer not allowed: {Method} {Path}", method, path);
                break;

            case InsufficientFundsException:
                context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                response.ErrorCode = (int)BankingErrorCode.InsufficientFunds;
                response.Message = "Insufficient funds for this operation.";
                logger.LogInformation("Insufficient funds: {Method} {Path}", method, path);
                break;

            case DuplicateAccountNumberException:
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                response.StatusCode = StatusCodes.Status409Conflict;
                response.ErrorCode = (int)BankingErrorCode.DuplicateAccountNumber;
                response.Message = "An account with this number already exists.";
                logger.LogInformation("Duplicate account number: {Method} {Path}", method, path);
                break;

            case DuplicateEmailException:
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                response.StatusCode = StatusCodes.Status409Conflict;
                response.ErrorCode = (int)BankingErrorCode.DuplicateEmail;
                response.Message = "This email address is already registered.";
                logger.LogInformation("Duplicate email: {Method} {Path}", method, path);
                break;

            case CurrencyMismatchException ex:
                context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                response.ErrorCode = (int)BankingErrorCode.CurrencyMismatch;
                response.Message = ex.Message;
                logger.LogInformation("Currency mismatch: {Method} {Path}", method, path);
                break;

            case InvalidTransferAmountException ex:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.ErrorCode = (int)BankingErrorCode.InvalidTransferAmount;
                response.Message = ex.Message;
                logger.LogInformation("Invalid transfer amount: {Method} {Path}", method, path);
                break;

            case AccountFrozenException:
                context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                response.ErrorCode = (int)BankingErrorCode.AccountFrozen;
                response.Message = "This account is frozen and cannot process transactions.";
                logger.LogInformation("Account frozen: {Method} {Path}", method, path);
                break;

            case ArgumentException ex:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.ErrorCode = (int)BankingErrorCode.ValidationFailed;
                response.Message = ex.Message;
                logger.LogInformation("Validation error: {Method} {Path} - {Message}", method, path, ex.Message);
                break;

            case InvalidOperationException ex:
                // Check if this is a not-found scenario from query handlers
                if (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    response.StatusCode = StatusCodes.Status404NotFound;
                    response.ErrorCode = (int)BankingErrorCode.AccountNotFound;
                    response.Message = "Resource not found.";
                    logger.LogInformation("Resource not found: {Method} {Path}", method, path);
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status409Conflict;
                    response.StatusCode = StatusCodes.Status409Conflict;
                    response.ErrorCode = (int)BankingErrorCode.ValidationFailed;
                    response.Message = "Operation could not be completed.";
                    logger.LogWarning("Invalid operation: {Method} {Path} - {Message}", method, path, ex.Message);
                }
                break;

            default:
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                response.StatusCode = StatusCodes.Status500InternalServerError;
                response.ErrorCode = (int)BankingErrorCode.InternalServerError;
                response.Message = "An internal error occurred. Please try again later.";
                logger.LogError(exception, "Unhandled exception: {Method} {Path} - Code: {ErrorCode}", method, path, response.ErrorCode);
                break;
        }

        return context.Response.WriteAsJsonAsync(response);
    }
}

