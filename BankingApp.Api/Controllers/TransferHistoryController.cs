using Microsoft.AspNetCore.Mvc;
using BankingApp.Application.CQRS.Queries;
using BankingApp.Application.CQRS.QueryHandlers;
using BankingApp.Application.DTOs;

namespace BankingApp.Api.Controllers;

[ApiController]
[Route("api/transfer-history")]
public class TransferHistoryController : ControllerBase
{
    private readonly ListTransfersQueryHandler _listTransfersHandler;

    public TransferHistoryController(ListTransfersQueryHandler listTransfersHandler)
    {
        _listTransfersHandler = listTransfersHandler;
    }

    /// <summary>
    /// List all transfers with optional account filtering and pagination
    /// </summary>
    /// <remarks>
    /// When accountId is provided, returns transfers where the account is either the source or destination.
    /// Results are paginated with configurable page size (1-100 items per page).
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponse))]
    public async Task<IActionResult> GetTransferHistory(
        [FromQuery] Guid? accountId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        if (pageNumber <= 0)
            throw new ArgumentException("pageNumber must be greater than 0");

        if (pageSize <= 0 || pageSize > 100)
            throw new ArgumentException("pageSize must be between 1 and 100");

        var query = new ListTransfersQuery 
        { 
            AccountId = accountId,
            PageNumber = pageNumber, 
            PageSize = pageSize 
        };
        var result = await _listTransfersHandler.HandleAsync(query);
        return Ok(result);
    }
}
