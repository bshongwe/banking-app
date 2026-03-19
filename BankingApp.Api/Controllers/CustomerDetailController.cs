using Microsoft.AspNetCore.Mvc;
using BankingApp.Application.CQRS.Queries;
using BankingApp.Application.CQRS.QueryHandlers;
using BankingApp.Application.DTOs;

namespace BankingApp.Api.Controllers;

[ApiController]
[Route("api/customers/{id}")]
public class CustomerDetailController(GetCustomerQueryHandler getCustomerHandler) : ControllerBase
{

    /// <summary>
    /// Get a customer by ID
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponse))]
    public async Task<IActionResult> GetCustomerById(Guid id)
    {
        var query = new GetCustomerQuery { CustomerId = id };
        var customer = await getCustomerHandler.HandleAsync(query);
        return Ok(customer);
    }
}
