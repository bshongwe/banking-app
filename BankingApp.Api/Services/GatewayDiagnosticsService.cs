using BankingApp.Application.Services.PaymentGateways;
using BankingApp.Domain.PaymentGateways;

namespace BankingApp.Api.Services;

/// <summary>
/// Background service that periodically validates all payment gateway configurations
/// and logs alerts when a gateway becomes unavailable or misconfigured.
/// Runs every 5 minutes by default; interval is configurable via GatewayDiagnostics:IntervalSeconds.
/// </summary>
public class GatewayDiagnosticsService(
    IServiceScopeFactory scopeFactory,
    ILogger<GatewayDiagnosticsService> logger,
    GatewayDiagnosticsOptions options) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Gateway diagnostics service started. Interval: {Interval}s",
            options.IntervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            await ValidateAllGatewaysAsync();
            await Task.Delay(TimeSpan.FromSeconds(options.IntervalSeconds), stoppingToken);
        }
    }

    private async Task ValidateAllGatewaysAsync()
    {
        // IPaymentGatewayFactory is scoped — resolve via scope to avoid captive dependency
        using var scope = scopeFactory.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<IPaymentGatewayFactory>();

        IEnumerable<IPaymentGateway> gateways;
        try
        {
            gateways = factory.GetAllGateways();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Gateway diagnostics: failed to retrieve gateway list");
            return;
        }

        foreach (var gateway in gateways)
        {
            try
            {
                var isValid = await gateway.ValidateConfigurationAsync();
                if (isValid)
                {
                    logger.LogInformation(
                        "Gateway diagnostics: {ProviderId} is configured and available",
                        gateway.ProviderId);
                }
                else
                {
                    // Alert — gateway is reachable but misconfigured
                    logger.LogWarning(
                        "Gateway diagnostics: {ProviderId} configuration is invalid or incomplete. " +
                        "Transfers via this gateway will fail until resolved.",
                        gateway.ProviderId);
                }
            }
            catch (Exception ex)
            {
                // Alert — gateway threw during validation (network issue, SDK error, etc.)
                logger.LogError(ex,
                    "Gateway diagnostics: {ProviderId} validation threw an exception. " +
                    "Gateway may be unreachable.",
                    gateway.ProviderId);
            }
        }
    }
}

/// <summary>
/// Configuration options for GatewayDiagnosticsService.
/// Bind from appsettings.json under "GatewayDiagnostics".
/// </summary>
public class GatewayDiagnosticsOptions
{
    /// <summary>
    /// How often to validate all gateways, in seconds. Default: 300 (5 minutes).
    /// </summary>
    public int IntervalSeconds { get; set; } = 300;
}
