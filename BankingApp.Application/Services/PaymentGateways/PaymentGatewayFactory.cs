using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BankingApp.Infrastructure.PaymentGateways;

namespace BankingApp.Application.Services.PaymentGateways;

/// <summary>
/// Factory for managing multiple payment gateway providers
/// Enables dynamic selection of payment processor based on requirements
/// </summary>
public interface IPaymentGatewayFactory
{
    /// <summary>
    /// Get a specific payment gateway by provider ID
    /// </summary>
    IPaymentGateway GetGateway(string providerId);

    /// <summary>
    /// Get all available payment gateways
    /// </summary>
    IEnumerable<IPaymentGateway> GetAllGateways();

    /// <summary>
    /// Automatically select the best gateway based on transfer details
    /// </summary>
    Task<IPaymentGateway> SelectOptimalGatewayAsync(PaymentRequest request);
}

/// <summary>
/// Default implementation of payment gateway factory
/// </summary>
public class PaymentGatewayFactory : IPaymentGatewayFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PaymentGatewayFactory> _logger;
    private readonly Dictionary<string, Type> _registeredGateways;

    public PaymentGatewayFactory(
        IServiceProvider serviceProvider,
        ILogger<PaymentGatewayFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _registeredGateways = new Dictionary<string, Type>();
    }

    public IPaymentGateway GetGateway(string providerId)
    {
        try
        {
            var gateway = providerId.ToLower() switch
            {
                "stripe" => (IPaymentGateway)_serviceProvider.GetRequiredService<StripePaymentGateway>(),
                "paypal" => (IPaymentGateway)_serviceProvider.GetRequiredService<PayPalPaymentGateway>(),
                "sa_banks" => (IPaymentGateway)_serviceProvider.GetRequiredService<SouthAfricanBankPaymentGateway>(),
                _ => throw new KeyNotFoundException($"Payment gateway '{providerId}' not found")
            };

            _logger.LogInformation("Retrieved payment gateway: {ProviderId}", providerId);
            return gateway;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve payment gateway: {ProviderId}", providerId);
            throw;
        }
    }

    public IEnumerable<IPaymentGateway> GetAllGateways()
    {
        try
        {
            var gateways = new List<IPaymentGateway>
            {
                _serviceProvider.GetRequiredService<StripePaymentGateway>(),
                _serviceProvider.GetRequiredService<PayPalPaymentGateway>(),
                _serviceProvider.GetRequiredService<SouthAfricanBankPaymentGateway>()
            };

            _logger.LogInformation("Retrieved {Count} payment gateways", gateways.Count);
            return gateways;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve all payment gateways");
            throw;
        }
    }

    public async Task<IPaymentGateway> SelectOptimalGatewayAsync(PaymentRequest request)
    {
        _logger.LogInformation("Selecting optimal payment gateway for {Amount} {Currency}", 
            request.Amount, request.Currency);

        var gateways = GetAllGateways().ToList();

        foreach (var gateway in gateways)
        {
            if (await gateway.ValidateConfigurationAsync())
            {
                if (SupportsRequest(gateway, request))
                {
                    _logger.LogInformation("Selected gateway: {ProviderId}", gateway.ProviderId);
                    return gateway;
                }
            }
        }

        throw new InvalidOperationException("No suitable payment gateway found for the requested transfer");
    }

    /// <summary>
    /// Determines if a gateway supports the given payment request
    /// </summary>
    private bool SupportsRequest(IPaymentGateway gateway, PaymentRequest request)
    {
        return gateway.ProviderId switch
        {
            "stripe" => IsSupportedByStripe(request),
            "paypal" => IsSupportedByPayPal(request),
            "sa_banks" => IsSupportedBySouthAfricanBanks(request),
            _ => false
        };
    }

    private bool IsSupportedByStripe(PaymentRequest request)
    {
        var config = _serviceProvider.GetRequiredService<IOptions<StripeConfig>>();
        return config.Value.SupportedCurrencies.Contains(request.Currency.ToUpper());
    }

    private bool IsSupportedByPayPal(PaymentRequest request)
    {
        var config = _serviceProvider.GetRequiredService<IOptions<PayPalConfig>>();
        return config.Value.SupportedCurrencies.Contains(request.Currency.ToUpper());
    }

    private bool IsSupportedBySouthAfricanBanks(PaymentRequest request)
    {
        var config = _serviceProvider.GetRequiredService<IOptions<SouthAfricanBankConfig>>();
        return config.Value.SupportedCurrencies.Contains(request.Currency.ToUpper());
    }
}
