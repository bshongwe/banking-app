using Microsoft.AspNetCore.Mvc;
using BankingApp.Application.CQRS.Commands;
using BankingApp.Application.CQRS.CommandHandlers;
using BankingApp.Application.Exceptions;

namespace BankingApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransfersController : ControllerBase
{
    private readonly TransferMoneyCommandHandler _transferHandler;
    private readonly ILogger<TransfersController> _logger;

    public TransfersController(
        TransferMoneyCommandHandler transferHandler,
        ILogger<TransfersController> logger)
    {
        _transferHandler = transferHandler;
        _logger = logger;
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
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> TransferMoney([FromBody] TransferMoneyCommand command)
    {
        try
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
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ResourceNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InsufficientFundsException ex)
        {
            return UnprocessableEntity(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing transfer");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An internal error occurred. Please try again later." });
        }
    }
}
