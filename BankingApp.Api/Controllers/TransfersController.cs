using Microsoft.AspNetCore.Mvc;
using BankingApp.Application.CQRS.Commands;
using BankingApp.Application.CQRS.CommandHandlers;

namespace BankingApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransfersController : ControllerBase
{
    private readonly TransferMoneyCommandHandler _transferHandler;

    public TransfersController(TransferMoneyCommandHandler transferHandler)
    {
        _transferHandler = transferHandler;
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
        catch (InvalidOperationException ex)
        {
            // Could be account not found or insufficient funds
            if (ex.Message.Contains("not found"))
                return NotFound(new { error = ex.Message });
            
            return UnprocessableEntity(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while processing the transfer", details = ex.Message });
        }
    }
}
