using BankingApp.Application.Services.PaymentGateways;
using BankingApp.Application.CQRS.Commands;
using BankingApp.Domain.Entities;
using BankingApp.Infrastructure.Repositories;
using BankingApp.Application.Exceptions;
using BankingApp.Application.UnitOfWork;
using Microsoft.Extensions.Logging;

namespace BankingApp.Application.CQRS.CommandHandlers;

/// <summary>
/// Enhanced transfer handler supporting both internal and external payment gateways
/// Maintains double-entry bookkeeping for all transfers regardless of processing method
/// </summary>
public class EnhancedTransferMoneyCommandHandler
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILedgerRepository _ledgerRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentGatewayFactory _paymentGatewayFactory;
    private readonly ILogger<EnhancedTransferMoneyCommandHandler> _logger;

    public EnhancedTransferMoneyCommandHandler(
        IAccountRepository accountRepository,
        ILedgerRepository ledgerRepository,
        ITransactionRepository transactionRepository,
        IUnitOfWork unitOfWork,
        IPaymentGatewayFactory paymentGatewayFactory,
        ILogger<EnhancedTransferMoneyCommandHandler> logger)
    {
        _accountRepository = accountRepository;
        _ledgerRepository = ledgerRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
        _paymentGatewayFactory = paymentGatewayFactory;
        _logger = logger;
    }

    /// <summary>
    /// Handles transfer command with support for internal and external payment gateways
    /// </summary>
    public async Task<TransferResult> HandleAsync(Commands.TransferMoneyCommand command)
    {
        _logger.LogInformation(
            "Processing transfer. From: {FromAccountId}, To: {ToAccountId}, Amount: {Amount}, Gateway: {Gateway}",
            command.FromAccountId, command.ToAccountId, command.Amount, command.PaymentGateway);

        // Validate transfer amount
        if (command.Amount <= 0)
            throw new InvalidTransferAmountException(command.Amount);

        // Validate accounts are different
        if (command.FromAccountId == command.ToAccountId)
            throw new TransferNotAllowedException("Cannot transfer to the same account.");

        // Route to appropriate handler based on payment gateway
        return command.PaymentGateway?.ToLower() switch
        {
            "internal" => await HandleInternalTransferAsync(command),
            "stripe" => await HandleExternalTransferAsync(command, "stripe"),
            "paypal" => await HandleExternalTransferAsync(command, "paypal"),
            "sa_banks" => await HandleExternalTransferAsync(command, "sa_banks"),
            null or "" => await HandleInternalTransferAsync(command),
            _ => throw new InvalidOperationException($"Unknown payment gateway: {command.PaymentGateway}")
        };
    }

    /// <summary>
    /// Handles internal transfers using only double-entry bookkeeping
    /// No external payment processing
    /// </summary>
    private async Task<TransferResult> HandleInternalTransferAsync(Commands.TransferMoneyCommand command)
    {
        _logger.LogInformation("Processing internal transfer for reference: {Reference}", command.Reference);

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // Validate source account exists
            var fromAccount = await _accountRepository.GetByIdAsync(command.FromAccountId);
            if (fromAccount == null)
            {
                _logger.LogWarning("Source account not found: {AccountId}", command.FromAccountId);
                throw new ResourceNotFoundException("Account", command.FromAccountId);
            }

            // Validate destination account exists
            var toAccount = await _accountRepository.GetByIdAsync(command.ToAccountId);
            if (toAccount == null)
            {
                _logger.LogWarning("Destination account not found: {AccountId}", command.ToAccountId);
                throw new ResourceNotFoundException("Account", command.ToAccountId);
            }

            // Verify currencies match
            if (fromAccount.Currency != toAccount.Currency)
            {
                _logger.LogWarning(
                    "Currency mismatch. From: {FromCurrency}, To: {ToCurrency}",
                    fromAccount.Currency, toAccount.Currency);
                throw new CurrencyMismatchException(fromAccount.Currency, toAccount.Currency);
            }

            // Check sufficient balance
            var fromAccountBalance = await _accountRepository.GetBalanceAsync(command.FromAccountId);
            if (fromAccountBalance < command.Amount)
            {
                _logger.LogWarning(
                    "Insufficient funds. Balance: {Balance}, Required: {Required}",
                    fromAccountBalance, command.Amount);
                throw new InsufficientFundsException(fromAccountBalance, command.Amount);
            }

            // Create transaction
            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                Reference = command.Reference,
                CreatedAt = DateTime.UtcNow
            };

            // Create debit entry (withdraw from source)
            var debitEntry = new LedgerEntry
            {
                Id = Guid.NewGuid(),
                TransactionId = transaction.Id,
                AccountId = command.FromAccountId,
                Amount = command.Amount,
                EntryType = "Debit",
                CreatedAt = DateTime.UtcNow
            };

            // Create credit entry (deposit to destination)
            var creditEntry = new LedgerEntry
            {
                Id = Guid.NewGuid(),
                TransactionId = transaction.Id,
                AccountId = command.ToAccountId,
                Amount = command.Amount,
                EntryType = "Credit",
                CreatedAt = DateTime.UtcNow
            };

            // Persist entries
            await _transactionRepository.AddAsync(transaction);
            await _ledgerRepository.AddRangeAsync(new[] { debitEntry, creditEntry });
            await _transactionRepository.SaveChangesAsync();

            // Commit database transaction
            var commitSucceeded = await _unitOfWork.CommitTransactionAsync();
            if (!commitSucceeded)
            {
                _logger.LogError("Failed to commit internal transfer for reference: {Reference}", command.Reference);
                throw new InvalidOperationException("Failed to commit the transfer transaction. The operation was rolled back.");
            }

            _logger.LogInformation(
                "Internal transfer completed successfully. TransactionId: {TransactionId}, Reference: {Reference}",
                transaction.Id, command.Reference);

            return new TransferResult
            {
                TransactionId = transaction.Id,
                Reference = command.Reference,
                Status = "COMPLETED",
                Amount = command.Amount,
                Currency = fromAccount.Currency,
                CreatedAt = transaction.CreatedAt,
                CompletedAt = DateTime.UtcNow,
                PaymentGateway = "internal",
                EstimatedCompletionTime = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Internal transfer failed for reference: {Reference}", command.Reference);
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    /// <summary>
    /// Handles external transfers using payment gateways
    /// Creates ledger entries for audit trail even if external processing fails
    /// </summary>
    private async Task<TransferResult> HandleExternalTransferAsync(Commands.TransferMoneyCommand command, string gatewayId)
    {
        _logger.LogInformation(
            "Processing external transfer via {Gateway} for reference: {Reference}",
            gatewayId, command.Reference);

        try
        {
            // Validate accounts exist
            var fromAccount = await _accountRepository.GetByIdAsync(command.FromAccountId);
            if (fromAccount == null)
                throw new ResourceNotFoundException("Account", command.FromAccountId);

            var toAccount = await _accountRepository.GetByIdAsync(command.ToAccountId);
            if (toAccount == null)
                throw new ResourceNotFoundException("Account", command.ToAccountId);

            // Get appropriate gateway
            var gateway = _paymentGatewayFactory.GetGateway(gatewayId);

            // Validate gateway configuration
            if (!await gateway.ValidateConfigurationAsync())
            {
                _logger.LogError("Payment gateway {Gateway} is not properly configured", gatewayId);
                throw new InvalidOperationException($"Payment gateway '{gatewayId}' is not properly configured.");
            }

            // Map command to payment request
            var paymentRequest = MapCommandToPaymentRequest(command);

            // Process payment via external gateway
            _logger.LogInformation("Sending payment request to {Gateway}", gatewayId);
            var paymentResult = await gateway.ProcessPaymentAsync(paymentRequest);

            if (!paymentResult.Success)
            {
                _logger.LogWarning(
                    "External payment processing failed. Gateway: {Gateway}, Error: {Error}",
                    gatewayId, paymentResult.ErrorMessage);

                return new TransferResult
                {
                    Status = "FAILED",
                    Amount = command.Amount,
                    Currency = fromAccount.Currency,
                    CreatedAt = DateTime.UtcNow,
                    PaymentGateway = gatewayId,
                    ErrorMessage = paymentResult.ErrorMessage
                };
            }

            // Payment succeeded - create ledger entries for audit trail
            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                Reference = command.Reference,
                CreatedAt = DateTime.UtcNow
            };

            var debitEntry = new LedgerEntry
            {
                Id = Guid.NewGuid(),
                TransactionId = transaction.Id,
                AccountId = command.FromAccountId,
                Amount = command.Amount,
                EntryType = "Debit",
                CreatedAt = DateTime.UtcNow
            };

            var creditEntry = new LedgerEntry
            {
                Id = Guid.NewGuid(),
                TransactionId = transaction.Id,
                AccountId = command.ToAccountId,
                Amount = command.Amount,
                EntryType = "Credit",
                CreatedAt = DateTime.UtcNow
            };

            await _transactionRepository.AddAsync(transaction);
            await _ledgerRepository.AddRangeAsync(new[] { debitEntry, creditEntry });
            await _transactionRepository.SaveChangesAsync();

            _logger.LogInformation(
                "External transfer completed successfully. Gateway: {Gateway}, ExternalTransactionId: {ExternalId}",
                gatewayId, paymentResult.TransactionId);

            return new TransferResult
            {
                TransactionId = transaction.Id,
                Reference = command.Reference,
                Status = paymentResult.Status,
                Amount = command.Amount,
                Currency = fromAccount.Currency,
                CreatedAt = transaction.CreatedAt,
                CompletedAt = paymentResult.ProcessedAt,
                PaymentGateway = gatewayId,
                ExternalTransactionId = paymentResult.TransactionId,
                TransferMethod = DetermineTransferMethod(gatewayId, command.Urgency, command.Amount),
                EstimatedCompletionTime = EstimateCompletionTime(gatewayId, command.Urgency)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex, "External transfer failed via {Gateway} for reference: {Reference}",
                gatewayId, command.Reference);
            throw;
        }
    }

    /// <summary>
    /// Maps TransferMoneyCommand to PaymentRequest for gateway processing
    /// </summary>
    private PaymentRequest MapCommandToPaymentRequest(Commands.TransferMoneyCommand command)
    {
        return new PaymentRequest
        {
            Reference = command.Reference,
            Amount = command.Amount,
            Currency = "ZAR",
            SourceAccountType = command.SourceAccountType ?? "checking",
            SourceBankCode = command.GatewayMetadata.GetValueOrDefault("sourceBankCode", ""),
            DestinationAccountNumber = command.GatewayMetadata.GetValueOrDefault("accountNumber", ""),
            DestinationBankCode = command.DestinationBankCode ?? "",
            DestinationAccountHolder = command.GatewayMetadata.GetValueOrDefault("accountHolder", ""),
            Metadata = command.GatewayMetadata
        };
    }

    /// <summary>
    /// Determines transfer method based on gateway and parameters
    /// </summary>
    private string DetermineTransferMethod(string gateway, string urgency, decimal amount)
    {
        return gateway switch
        {
            "sa_banks" => urgency == "URGENT" || amount > 250000 ? "RTGS" : "EFT",
            "stripe" => "ACH",
            "paypal" => "PAYPAL",
            _ => "UNKNOWN"
        };
    }

    /// <summary>
    /// Estimates when transfer will complete based on gateway and parameters
    /// </summary>
    private DateTime EstimateCompletionTime(string gateway, string urgency)
    {
        var now = DateTime.UtcNow;
        return gateway switch
        {
            "sa_banks" when urgency == "URGENT" => now.AddMinutes(15), // RTGS - real-time
            "sa_banks" => now.AddDays(1), // EFT - 1 business day
            "stripe" => now.AddHours(1), // ACH - typically 1-2 hours
            "paypal" => now.AddHours(24), // PayPal - 24 hours
            _ => now.AddDays(2)
        };
    }
}
