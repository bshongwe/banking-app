using Microsoft.AspNetCore.Mvc;
using BankingApp.Application.CQRS.Commands;
using BankingApp.Application.CQRS.CommandHandlers;
using BankingApp.Application.CQRS.Queries;
using BankingApp.Application.CQRS.QueryHandlers;

namespace BankingApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly CreateAccountCommandHandler _createAccountHandler;
    private readonly GetAccountBalanceQueryHandler _getBalanceHandler;
    private readonly GetAccountDetailQueryHandler _getDetailHandler;
    private readonly GetAccountTransactionHistoryQueryHandler _getTransactionsHandler;

    public AccountsController(
        CreateAccountCommandHandler createAccountHandler,
        GetAccountBalanceQueryHandler getBalanceHandler,
        GetAccountDetailQueryHandler getDetailHandler,
        GetAccountTransactionHistoryQueryHandler getTransactionsHandler)
    {
        _createAccountHandler = createAccountHandler;
        _getBalanceHandler = getBalanceHandler;
        _getDetailHandler = getDetailHandler;
        _getTransactionsHandler = getTransactionsHandler;
    }

    /// <summary>
    /// Create a new bank account
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountCommand command)
    {
        try
        {
            var account = await _createAccountHandler.HandleAsync(command);
            return CreatedAtAction(nameof(GetAccountDetails), new { id = account.Id }, account);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while creating the account", details = ex.Message });
        }
    }

    /// <summary>
    /// Get account details including customer information
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAccountDetails(Guid id)
    {
        try
        {
            var query = new GetAccountDetailQuery { AccountId = id };
            var account = await _getDetailHandler.HandleAsync(query);
            return Ok(account);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while retrieving the account", details = ex.Message });
        }
    }

    /// <summary>
    /// Get the current balance of an account
    /// </summary>
    [HttpGet("{id}/balance")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAccountBalance(Guid id)
    {
        try
        {
            var query = new GetAccountBalanceQuery { AccountId = id };
            var balance = await _getBalanceHandler.HandleAsync(query);
            return Ok(new { accountId = id, balance });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while retrieving the balance", details = ex.Message });
        }
    }

    /// <summary>
    /// Get transaction history for an account
    /// </summary>
    [HttpGet("{id}/transactions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTransactionHistory(Guid id, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var query = new GetAccountTransactionHistoryQuery 
            { 
                AccountId = id,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            var result = await _getTransactionsHandler.HandleAsync(query);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while retrieving transactions", details = ex.Message });
        }
    }
}
