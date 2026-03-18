using Microsoft.AspNetCore.Mvc;
using BankingApp.Application.CQRS.Commands;
using BankingApp.Application.CQRS.CommandHandlers;
using BankingApp.Application.CQRS.Queries;
using BankingApp.Application.CQRS.QueryHandlers;
using BankingApp.Application.DTOs;

namespace BankingApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly CreateAccountCommandHandler _createAccountHandler;
    private readonly GetAccountBalanceQueryHandler _getBalanceHandler;
    private readonly GetAccountDetailQueryHandler _getDetailHandler;
    private readonly GetAccountTransactionHistoryQueryHandler _getTransactionsHandler;
    private readonly ListAccountsQueryHandler _listAccountsHandler;

    public AccountsController(
        CreateAccountCommandHandler createAccountHandler,
        GetAccountBalanceQueryHandler getBalanceHandler,
        GetAccountDetailQueryHandler getDetailHandler,
        GetAccountTransactionHistoryQueryHandler getTransactionsHandler,
        ListAccountsQueryHandler listAccountsHandler)
    {
        _createAccountHandler = createAccountHandler;
        _getBalanceHandler = getBalanceHandler;
        _getDetailHandler = getDetailHandler;
        _getTransactionsHandler = getTransactionsHandler;
        _listAccountsHandler = listAccountsHandler;
    }

    /// <summary>
    /// Create a new bank account
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponse))]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountCommand command)
    {
        var account = await _createAccountHandler.HandleAsync(command);
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
        var result = await _listAccountsHandler.HandleAsync(query);
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
        var account = await _getDetailHandler.HandleAsync(query);
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
        var balance = await _getBalanceHandler.HandleAsync(query);
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
        var result = await _getTransactionsHandler.HandleAsync(query);
        return Ok(result);
    }
}
