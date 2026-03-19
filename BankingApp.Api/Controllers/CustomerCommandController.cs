using Microsoft.AspNetCore.Mvc;

namespace BankingApp.Api.Controllers;

/// <summary>
/// Customer Command API - Split into CustomerCreateController (POST) and CustomerUpdateController (PUT)
/// </summary>
/// <remarks>
/// This controller is retained as a named anchor for route documentation.
/// All functionality has been moved to:
/// - CustomerCreateController: Create customer (POST /api/customers)
/// - CustomerUpdateController: Update customer (PUT /api/customers/{id})
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(IgnoreApi = true)]
[Obsolete("Functionality split into CustomerCreateController and CustomerUpdateController.")]
public class CustomerCommandController : ControllerBase;

