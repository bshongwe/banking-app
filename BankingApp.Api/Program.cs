using BankingApp.Infrastructure.Data;
using BankingApp.Infrastructure.Repositories;
using BankingApp.Application.Services;
using BankingApp.Application.CQRS.CommandHandlers;
using BankingApp.Application.CQRS.QueryHandlers;
using BankingApp.Application.Validators;
using BankingApp.Application.UnitOfWork;
using BankingApp.Api.Middleware;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

// Configure API Controller behavior and validation error response format
builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                );

            var errorResponse = new BankingApp.Application.DTOs.ErrorResponse
            {
                Message = "One or more validation errors occurred.",
                ErrorCode = (int)BankingApp.Application.Exceptions.BankingErrorCode.ValidationFailed,
                StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest,
                TraceId = context.HttpContext.TraceIdentifier,
                ValidationErrors = errors
            };

            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(errorResponse);
        };
    });

// Register DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Replace |DataDirectory| token with the application's data directory
var dataDirectory = Path.Combine(AppContext.BaseDirectory, "Data");
Directory.CreateDirectory(dataDirectory);
connectionString = connectionString.Replace("|DataDirectory|", dataDirectory);

builder.Services.AddDbContext<BankingDbContext>(options =>
    options.UseSqlite(connectionString));

// Register repositories
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<ILedgerRepository, LedgerRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

// Register Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register services
builder.Services.AddScoped<ITransferService, TransferService>();

// Register CQRS Command Handlers
builder.Services.AddScoped<TransferMoneyCommandHandler>();
builder.Services.AddScoped<CreateAccountCommandHandler>();
builder.Services.AddScoped<CreateCustomerCommandHandler>();

// Register CQRS Query Handlers
builder.Services.AddScoped<GetAccountBalanceQueryHandler>();
builder.Services.AddScoped<GetAccountDetailQueryHandler>();
builder.Services.AddScoped<GetAccountTransactionHistoryQueryHandler>();
builder.Services.AddScoped<GetCustomerQueryHandler>();
builder.Services.AddScoped<ListCustomersQueryHandler>();
builder.Services.AddScoped<ListAccountsQueryHandler>();
builder.Services.AddScoped<ListTransfersQueryHandler>();

var app = builder.Build();

// Apply migrations automatically on startup (Development environment only)
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var db = scope.ServiceProvider.GetRequiredService<BankingDbContext>();
            await db.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while migrating the database");
            throw;
        }
    }
}
else
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogWarning("Database migrations are not applied automatically in non-Development environments. " +
        "Please ensure migrations are applied manually before running the application.");
}

// Configure the HTTP request pipeline.
// Add global error handling middleware (must be near the top of the pipeline)
app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    // Only use HTTPS redirect in production
    app.UseHttpsRedirection();
}

// Enable static files (for API docs HTML)
app.UseStaticFiles();

// Map controllers
app.MapControllers();

app.Run();
