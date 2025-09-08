using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Antifraud.Infrastructure.Persistence;

namespace Antifraud.Infrastructure.Services;

public interface IDatabaseMigrator
{
    Task MigrateAsync();
    Task SeedDataAsync();
}

public class DatabaseMigrator : IDatabaseMigrator
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DatabaseMigrator> _logger;

    public DatabaseMigrator(ApplicationDbContext context, ILogger<DatabaseMigrator> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task MigrateAsync()
    {
        try
        {
            _logger.LogInformation("Starting database migration");
            
            await _context.Database.MigrateAsync();
            
            _logger.LogInformation("Database migration completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during database migration");
            throw;
        }
    }

    public async Task SeedDataAsync()
    {
        try
        {
            _logger.LogInformation("Starting database seeding");

            // Verificar si ya existen datos
            if (await _context.Accounts.AnyAsync())
            {
                _logger.LogInformation("Database already contains data, skipping seed");
                return;
            }

            // Crear cuentas de prueba
            var sourceAccount = Domain.Entities.Account.Create(
                Domain.ValueObjects.AccountId.From(Guid.Parse("11111111-1111-1111-1111-111111111111")),
                "ACC001",
                "John Doe");

            var targetAccount = Domain.Entities.Account.Create(
                Domain.ValueObjects.AccountId.From(Guid.Parse("22222222-2222-2222-2222-222222222222")),
                "ACC002",
                "Jane Smith");

            var targetAccount2 = Domain.Entities.Account.Create(
                Domain.ValueObjects.AccountId.From(Guid.Parse("33333333-3333-3333-3333-333333333333")),
                "ACC003",
                "Bob Johnson");

            await _context.Accounts.AddRangeAsync(sourceAccount, targetAccount, targetAccount2);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during database seeding");
            throw;
        }
    }
}