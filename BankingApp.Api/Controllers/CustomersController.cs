using Microsoft.AspNetCore.Mvc;

namespace BankingApp.Api.Controllers;

/// <summary>
/// Customers API - Split into CustomerCommandController (POST/PUT) and CustomerQueryController (GET)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    // This controller is now a facade/dispatcher.
    // All functionality has been moved to:
    // - CustomerCommandController: Create and Update operations
    // - CustomerQueryController: List and Get operations
}

