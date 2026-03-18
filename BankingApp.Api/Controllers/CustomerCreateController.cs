using Microsoft.AspNetCore.Mvc;
using BankingApp.Application.CQRS.Commands;
using BankingApp.Application.CQRS.CommandHandlers;
using BankingApp.Application.DTOs;

namespace BankingApp.Api.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomerCreateController : ControllerBase
{
    private readonly CreateCustomerCommandHandler _createCustomerHandler;

    public CustomerCreateController(CreateCustomerCommandHandler createCustomerHandler)
    {
        _createCustomerHandler = createCustomerHandler;
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
        return CreatedAtAction("GetCustomerById", "CustomerDetail", new { id = customer.Id }, customer);
    }
}
