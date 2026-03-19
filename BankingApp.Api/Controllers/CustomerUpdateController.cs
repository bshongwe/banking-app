using Microsoft.AspNetCore.Mvc;
using BankingApp.Application.CQRS.Commands;
using BankingApp.Application.CQRS.CommandHandlers;
using BankingApp.Application.DTOs;

namespace BankingApp.Api.Controllers;

[ApiController]
[Route("api/customers/{id}")]
public class CustomerUpdateController : ControllerBase
{
    private readonly UpdateCustomerCommandHandler _updateCustomerHandler;

    public CustomerUpdateController(UpdateCustomerCommandHandler updateCustomerHandler)
    {
        _updateCustomerHandler = updateCustomerHandler;
    }

    /// <summary>
    /// Update an existing customer
    /// </summary>
    [HttpPut]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponse))]
    public async Task<IActionResult> UpdateCustomer(Guid id, [FromBody] UpdateCustomerCommand command)
    {
        command.CustomerId = id;
        var customer = await _updateCustomerHandler.HandleAsync(command);
        return Ok(customer);
    }
}
