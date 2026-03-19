using Microsoft.AspNetCore.Mvc;

namespace BankingApp.Api.Controllers;

/// <summary>
/// Customers API - Split into CustomerCommandController (POST/PUT) and CustomerQueryController (GET)
/// </summary>
/// <remarks>
/// This controller is retained as a named anchor for route documentation.
/// All functionality has been moved to:
/// - CustomerCommandController: Create and Update operations
/// - CustomerQueryController: List and Get operations
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(IgnoreApi = true)]
[Obsolete("Functionality split into CustomerCommandController and CustomerQueryController.")]
public class CustomersController : ControllerBase;


