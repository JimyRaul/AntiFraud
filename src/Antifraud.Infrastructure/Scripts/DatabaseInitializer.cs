using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Antifraud.Infrastructure.Services;

namespace Antifraud.Infrastructure.Scripts;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<DatabaseMigrator>>();

        try
        {
            var migrator = services.GetRequiredService<IDatabaseMigrator>();
            
            logger.LogInformation("Initializing database...");
            
            await migrator.MigrateAsync();
            await migrator.SeedDataAsync();
            
            logger.LogInformation("Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database");
            throw;
        }
    }
}