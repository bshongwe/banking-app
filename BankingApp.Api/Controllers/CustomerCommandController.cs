using Microsoft.AspNetCore.Mvc;

namespace BankingApp.Api.Controllers;

/// <summary>
/// Customer Command API - Split into CustomerCreateController (POST) and CustomerUpdateController (PUT)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CustomerCommandController : ControllerBase
{
    // This controller is now a facade/dispatcher.
    // All functionality has been moved to:
    // - CustomerCreateController: Create customer (POST /api/customers)
    // - CustomerUpdateController: Update customer (PUT /api/customers/{id})
}
