using Microsoft.AspNetCore.Mvc;
using BankingApp.Application.CQRS.Commands;
using BankingApp.Application.CQRS.CommandHandlers;

namespace BankingApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly CreateCustomerCommandHandler _createCustomerHandler;

    public CustomersController(CreateCustomerCommandHandler createCustomerHandler)
    {
        _createCustomerHandler = createCustomerHandler;
    }

    /// <summary>
    /// Create a new customer
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerCommand command)
    {
        try
        {
            var customer = await _createCustomerHandler.HandleAsync(command);
            return CreatedAtAction(nameof(CreateCustomer), new { id = customer.Id }, customer);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while creating the customer", details = ex.Message });
        }
    }
}
