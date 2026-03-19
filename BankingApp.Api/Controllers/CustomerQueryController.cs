using Microsoft.AspNetCore.Mvc;

namespace BankingApp.Api.Controllers;

/// <summary>
/// Customer Query API - Split into CustomerListController (GET list) and CustomerDetailController (GET by ID)
/// </summary>
/// <remarks>
/// This controller is retained as a named anchor for route documentation.
/// All functionality has been moved to:
/// - CustomerListController: List customers (GET /api/customers)
/// - CustomerDetailController: Get customer by ID (GET /api/customers/{id})
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(IgnoreApi = true)]
[Obsolete("Functionality split into CustomerListController and CustomerDetailController.")]
public class CustomerQueryController : ControllerBase;

