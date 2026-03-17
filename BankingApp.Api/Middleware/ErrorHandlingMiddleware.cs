using System.Net;
using BankingApp.Application.DTOs;
using BankingApp.Application.Exceptions;

namespace BankingApp.Api.Middleware;

/// <summary>
/// Global exception handling middleware that converts domain exceptions to standardized error responses.
/// This middleware ensures consistent error handling across all endpoints.
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
            _logger.LogError(ex, "Unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse
        {
            Message = "An error occurred.",
            TraceId = context.TraceIdentifier
        };

        switch (exception)
        {
            case ResourceNotFoundException ex:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                response.StatusCode = StatusCodes.Status404NotFound;
                response.ErrorCode = (int)BankingErrorCode.AccountNotFound;
                response.Message = ex.Message;
                break;

            case InsufficientFundsException ex:
                context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                response.ErrorCode = (int)BankingErrorCode.InsufficientFunds;
                response.Message = ex.Message;
                break;

            case DuplicateAccountNumberException ex:
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                response.StatusCode = StatusCodes.Status409Conflict;
                response.ErrorCode = (int)BankingErrorCode.DuplicateAccountNumber;
                response.Message = ex.Message;
                break;

            case DuplicateEmailException ex:
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                response.StatusCode = StatusCodes.Status409Conflict;
                response.ErrorCode = (int)BankingErrorCode.DuplicateEmail;
                response.Message = ex.Message;
                break;

            case CurrencyMismatchException ex:
                context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                response.ErrorCode = (int)BankingErrorCode.CurrencyMismatch;
                response.Message = ex.Message;
                break;

            case InvalidTransferAmountException ex:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.ErrorCode = (int)BankingErrorCode.InvalidTransferAmount;
                response.Message = ex.Message;
                break;

            case AccountFrozenException ex:
                context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                response.ErrorCode = (int)BankingErrorCode.AccountFrozen;
                response.Message = ex.Message;
                break;

            case ArgumentException ex:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.ErrorCode = (int)BankingErrorCode.ValidationFailed;
                response.Message = ex.Message;
                break;

            default:
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                response.StatusCode = StatusCodes.Status500InternalServerError;
                response.ErrorCode = (int)BankingErrorCode.InternalServerError;
                response.Message = "An internal error occurred. Please try again later.";
                break;
        }

        return context.Response.WriteAsJsonAsync(response);
    }
}
