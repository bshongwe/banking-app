using Microsoft.AspNetCore.Mvc;
using BankingApp.Application.CQRS.Queries;
using BankingApp.Application.CQRS.QueryHandlers;
using BankingApp.Application.DTOs;

namespace BankingApp.Api.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomerQueryController : ControllerBase
{
    private readonly GetCustomerQueryHandler _getCustomerHandler;
    private readonly ListCustomersQueryHandler _listCustomersHandler;

    public CustomerQueryController(
        GetCustomerQueryHandler getCustomerHandler,
        ListCustomersQueryHandler listCustomersHandler)
    {
        _getCustomerHandler = getCustomerHandler;
        _listCustomersHandler = listCustomersHandler;
    }

    /// <summary>
    /// List all customers with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponse))]
    public async Task<IActionResult> ListCustomers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        if (pageNumber <= 0)
            throw new ArgumentException("pageNumber must be greater than 0");

        if (pageSize <= 0 || pageSize > 100)
            throw new ArgumentException("pageSize must be between 1 and 100");

        var query = new ListCustomersQuery { PageNumber = pageNumber, PageSize = pageSize };
        var result = await _listCustomersHandler.HandleAsync(query);
        return Ok(result);
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
}
