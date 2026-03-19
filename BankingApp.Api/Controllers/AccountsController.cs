using Microsoft.AspNetCore.Mvc;
using BankingApp.Application.CQRS.Commands;
using BankingApp.Application.CQRS.Queries;
using BankingApp.Application.DTOs;
using BankingApp.Api.Handlers;

namespace BankingApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly AccountCommandHandlers _commands;
    private readonly AccountQueryHandlers _queries;

    public AccountsController(
        AccountCommandHandlers commands,
        AccountQueryHandlers queries)
    {
        _commands = commands;
        _queries = queries;
    }

    /// <summary>
    /// Create a new bank account
    /// </summary>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponse))]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountCommand command)
    {
        var account = await _commands.Create.HandleAsync(command);
        return CreatedAtAction(nameof(GetAccountDetails), new { id = account.Id }, account);
    }

    /// <summary>
    /// List all accounts with optional customer filtering and pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponse))]
    public async Task<IActionResult> ListAccounts(
        [FromQuery] Guid? customerId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        if (pageNumber <= 0)
            throw new ArgumentException("pageNumber must be greater than 0");

        if (pageSize <= 0 || pageSize > 100)
            throw new ArgumentException("pageSize must be between 1 and 100");

        var query = new ListAccountsQuery 
        { 
            CustomerId = customerId,
            PageNumber = pageNumber, 
            PageSize = pageSize 
        };
        var result = await _queries.List.HandleAsync(query);
        return Ok(result);
    }

    /// <summary>
    /// Get account details including customer information
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponse))]
    public async Task<IActionResult> GetAccountDetails(Guid id)
    {
        var query = new GetAccountDetailQuery { AccountId = id };
        var account = await _queries.GetDetail.HandleAsync(query);
        return Ok(account);
    }

    /// <summary>
    /// Get the current balance of an account
    /// </summary>
    [HttpGet("{id}/balance")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponse))]
    public async Task<IActionResult> GetAccountBalance(Guid id)
    {
        var query = new GetAccountBalanceQuery { AccountId = id };
        var balance = await _queries.GetBalance.HandleAsync(query);
        return Ok(new { accountId = id, balance });
    }

    /// <summary>
    /// Get transaction history for an account with pagination
    /// </summary>
    [HttpGet("{id}/transactions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponse))]
    public async Task<IActionResult> GetTransactionHistory(
        Guid id,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        // Validate pagination parameters
        if (pageNumber <= 0)
            throw new ArgumentException("pageNumber must be greater than 0");
        
        if (pageSize <= 0 || pageSize > 100)
            throw new ArgumentException("pageSize must be between 1 and 100");

        var query = new GetAccountTransactionHistoryQuery 
        { 
            AccountId = id,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        var result = await _queries.GetTransactions.HandleAsync(query);
        return Ok(result);
    }

    /// <summary>
    /// Update an existing account
    /// </summary>
    [HttpPut("{id}")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponse))]
    public async Task<IActionResult> UpdateAccount(Guid id, [FromBody] UpdateAccountCommand command)
    {
        command.AccountId = id;
        var account = await _commands.Update.HandleAsync(command);
        return Ok(account);
    }

    /// <summary>
    /// Freeze an account (prevent transactions)
    /// </summary>
    [HttpPost("{id}/freeze")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponse))]
    public async Task<IActionResult> FreezeAccount(Guid id)
    {
        var command = new FreezeAccountCommand { AccountId = id };
        var account = await _commands.Freeze.HandleAsync(command);
        return Ok(new { message = "Account frozen successfully", account });
    }

    /// <summary>
    /// Unfreeze an account (allow transactions)
    /// </summary>
    [HttpPost("{id}/unfreeze")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponse))]
    public async Task<IActionResult> UnfreezeAccount(Guid id)
    {
        var command = new UnfreezeAccountCommand { AccountId = id };
        var account = await _commands.Unfreeze.HandleAsync(command);
        return Ok(new { message = "Account unfrozen successfully", account });
    }
}
