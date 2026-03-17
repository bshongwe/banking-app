using Microsoft.AspNetCore.Mvc;
using BankingApp.Application.CQRS.Commands;
using BankingApp.Application.CQRS.CommandHandlers;
using BankingApp.Application.CQRS.Queries;
using BankingApp.Application.CQRS.QueryHandlers;
using BankingApp.Application.DTOs;

namespace BankingApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly CreateCustomerCommandHandler _createCustomerHandler;
    private readonly GetCustomerQueryHandler _getCustomerHandler;

    public CustomersController(
        CreateCustomerCommandHandler createCustomerHandler,
        GetCustomerQueryHandler getCustomerHandler)
    {
        _createCustomerHandler = createCustomerHandler;
        _getCustomerHandler = getCustomerHandler;
    }

    /// <summary>
    /// Get a customer by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponse))]
    public async Task<IActionResult> GetCustomerById(Guid id)
    {
        var query = new GetCustomerQuery { CustomerId = id };
        var customer = await _getCustomerHandler.HandleAsync(query);
        return Ok(customer);
    }

    /// <summary>
    /// Create a new customer
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponse))]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerCommand command)
    {
        var customer = await _createCustomerHandler.HandleAsync(command);
        return CreatedAtAction(nameof(GetCustomerById), new { id = customer.Id }, customer);
    }
}
