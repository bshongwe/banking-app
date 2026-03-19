using Microsoft.AspNetCore.Mvc;
using BankingApp.Application.CQRS.Commands;
using BankingApp.Application.CQRS.CommandHandlers;
using BankingApp.Application.DTOs;

namespace BankingApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransfersController(EnhancedTransferMoneyCommandHandler transferHandler) : ControllerBase
{
    /// <summary>
    /// Transfer money from one account to another
    /// </summary>
    /// <remarks>
    /// This endpoint uses double-entry bookkeeping to ensure financial consistency.
    /// Each transfer creates two ledger entries: a debit on the source account and a credit on the destination account.
    /// Specify "paymentGateway": "internal" (default), "stripe", "paypal", or "sa_banks".
    /// </remarks>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponse))]
    public async Task<IActionResult> TransferMoney([FromBody] TransferMoneyCommand command)
    {
        var result = await transferHandler.HandleAsync(command);
        return Ok(new
        {
            message = "Transfer completed successfully",
            transactionId = result.TransactionId,
            reference = result.Reference,
            status = result.Status,
            amount = result.Amount,
            currency = result.Currency,
            paymentGateway = result.PaymentGateway,
            externalTransactionId = result.ExternalTransactionId,
            transferMethod = result.TransferMethod,
            createdAt = result.CreatedAt,
            estimatedCompletionTime = result.EstimatedCompletionTime
        });
    }
}