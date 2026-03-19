using Microsoft.AspNetCore.Mvc;
using BankingApp.Application.Services.PaymentGateways;
using BankingApp.Application.DTOs;

namespace BankingApp.Api.Controllers;

/// <summary>
/// Payment gateway administration and selection endpoints.
/// Used for health checks, monitoring dashboards, and frontend gateway selection UI.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class GatewaysController(
    IPaymentGatewayFactory gatewayFactory,
    ILogger<GatewaysController> logger) : ControllerBase
{
    /// <summary>
    /// List all payment gateways with their current configuration status.
    /// Use this endpoint to populate a gateway selection UI or monitor gateway health.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GatewayListDto))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponse))]
    public async Task<IActionResult> ListGateways()
    {
        logger.LogInformation("Gateway health check requested");

        var gateways = gatewayFactory.GetAllGateways().ToList();
        var checkedAt = DateTime.UtcNow;

        var statuses = new List<GatewayStatusDto>();
        foreach (var gateway in gateways)
        {
            var isConfigured = await gateway.ValidateConfigurationAsync();
            statuses.Add(new GatewayStatusDto
            {
                ProviderId = gateway.ProviderId,
                IsConfigured = isConfigured,
                Status = isConfigured ? "Available" : "Unavailable",
                CheckedAt = checkedAt
            });
        }

        return Ok(new GatewayListDto
        {
            Gateways = statuses,
            TotalAvailable = statuses.Count,
            TotalConfigured = statuses.Count(g => g.IsConfigured),
            CheckedAt = checkedAt
        });
    }

    /// <summary>
    /// Get the configuration status of a specific payment gateway.
    /// Supported provider IDs: stripe, paypal, sa_banks.
    /// </summary>
    [HttpGet("{providerId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GatewayStatusDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponse))]
    public async Task<IActionResult> GetGatewayStatus(string providerId)
    {
        logger.LogInformation("Gateway status check requested for: {ProviderId}", providerId);

        IPaymentGateway gateway;
        try
        {
            gateway = gatewayFactory.GetGateway(providerId);
        }
        catch (InvalidOperationException)
        {
            return NotFound(new ErrorResponse(
                $"Payment gateway '{providerId}' not found.",
                4001,
                StatusCodes.Status404NotFound,
                HttpContext.TraceIdentifier));
        }

        var isConfigured = await gateway.ValidateConfigurationAsync();
        return Ok(new GatewayStatusDto
        {
            ProviderId = gateway.ProviderId,
            IsConfigured = isConfigured,
            Status = isConfigured ? "Available" : "Unavailable",
            CheckedAt = DateTime.UtcNow
        });
    }
}
