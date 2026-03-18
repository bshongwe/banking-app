using Microsoft.AspNetCore.Mvc;

namespace BankingApp.Api.Controllers;

/// <summary>
/// Customer Query API - Split into CustomerListController (GET list) and CustomerDetailController (GET by ID)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CustomerQueryController : ControllerBase
{
    // This controller is now a facade/dispatcher.
    // All functionality has been moved to:
    // - CustomerListController: List customers (GET /api/customers)
    // - CustomerDetailController: Get customer by ID (GET /api/customers/{id})
}
