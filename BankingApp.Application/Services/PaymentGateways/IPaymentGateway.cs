// Payment gateway abstractions have been moved to BankingApp.Domain.PaymentGateways
// to break the circular dependency between Application and Infrastructure.
// These global aliases preserve backward compatibility for all existing Application code.
global using IPaymentGateway = BankingApp.Domain.PaymentGateways.IPaymentGateway;
global using PaymentRequest = BankingApp.Domain.PaymentGateways.PaymentRequest;
global using PaymentResult = BankingApp.Domain.PaymentGateways.PaymentResult;
global using PaymentStatusResult = BankingApp.Domain.PaymentGateways.PaymentStatusResult;

namespace BankingApp.Application.Services.PaymentGateways;
// IPaymentGatewayFactory is defined in PaymentGatewayFactory.cs

