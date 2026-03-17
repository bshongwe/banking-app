using BankingApp.Infrastructure.Data;
using BankingApp.Infrastructure.Repositories;
using BankingApp.Application.Services;
using BankingApp.Application.CQRS.CommandHandlers;
using BankingApp.Application.CQRS.QueryHandlers;
using BankingApp.Application.Validators;
using BankingApp.Application.UnitOfWork;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddControllers();

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

var app = builder.Build();

// Apply migrations automatically on startup
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Map controllers
app.MapControllers();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
