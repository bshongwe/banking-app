using Microsoft.AspNetCore.Mvc;
using BankingApp.Application.CQRS.Commands;
using BankingApp.Application.CQRS.CommandHandlers;
using BankingApp.Application.CQRS.Queries;
using BankingApp.Application.CQRS.QueryHandlers;
using BankingApp.Application.DTOs;

namespace BankingApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransfersController : ControllerBase
{
    private readonly TransferMoneyCommandHandler _transferHandler;
    private readonly ListTransfersQueryHandler _listTransfersHandler;

    public TransfersController(
        TransferMoneyCommandHandler transferHandler,
        ListTransfersQueryHandler listTransfersHandler)
    {
        _transferHandler = transferHandler;
        _listTransfersHandler = listTransfersHandler;
    }

    /// <summary>
    /// Transfer money from one account to another
    /// </summary>
    /// <remarks>
    /// This endpoint uses double-entry bookkeeping to ensure financial consistency.
    /// Each transfer creates two ledger entries: a debit on the source account and a credit on the destination account.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponse))]
    public async Task<IActionResult> TransferMoney([FromBody] TransferMoneyCommand command)
    {
        var transaction = await _transferHandler.HandleAsync(command);
        return Ok(new 
        { 
            message = "Transfer completed successfully",
            transactionId = transaction.Id,
            reference = transaction.Reference,
            createdAt = transaction.CreatedAt
        });
    }

    /// <summary>
    /// List all transfers with optional account filtering and pagination
    /// </summary>
    /// <remarks>
    /// When accountId is provided, returns transfers where the account is either the source or destination.
    /// Results are paginated with configurable page size (1-100 items per page).
    /// This endpoint is provided for backward compatibility. Prefer GET /api/transfer-history for new clients.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponse))]
    public async Task<IActionResult> ListTransfers(
        [FromQuery] Guid? accountId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        if (pageNumber <= 0)
            throw new ArgumentException("pageNumber must be greater than 0");

        if (pageSize <= 0 || pageSize > 100)
            throw new ArgumentException("pageSize must be between 1 and 100");

        var query = new ListTransfersQuery 
        { 
            AccountId = accountId,
            PageNumber = pageNumber, 
            PageSize = pageSize 
        };
        var result = await _listTransfersHandler.HandleAsync(query);
        return Ok(result);
    }
}