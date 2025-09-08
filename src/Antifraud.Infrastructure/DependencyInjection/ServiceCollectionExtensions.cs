using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Antifraud.Application.Interfaces;
using Antifraud.Domain.Repositories;
using Antifraud.Domain.Services;
using Antifraud.Infrastructure.Messaging;
using Antifraud.Infrastructure.Messaging.Configuration;
using Antifraud.Infrastructure.Messaging.Handlers;
using Antifraud.Infrastructure.Persistence;
using Antifraud.Infrastructure.Persistence.Repositories;
using Antifraud.Infrastructure.Services;
using Antifraud.Infrastructure.HealthChecks;


namespace Antifraud.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureLayer(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Database Configuration
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? "Host=localhost;Database=antifraud;Username=postgres;Password=postgres";
            
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
            });

            options.EnableSensitiveDataLogging(false);
            options.EnableServiceProviderCaching(true);
        });

        // Kafka Configuration
        services.Configure<KafkaConfiguration>(
            configuration.GetSection(KafkaConfiguration.SectionName));

        // Repository Pattern
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Domain Services
        services.AddScoped<ITransactionDomainService, TransactionDomainService>();

        // Messaging
        services.AddSingleton<IEventPublisher, KafkaEventPublisher>();
        services.AddSingleton<IEventConsumer, KafkaEventConsumer>();
        services.AddScoped<TransactionValidationHandler>();

        // Background Services
        services.AddSingleton<IHostedService, TransactionValidationConsumerService>();

        // Health Checks
        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>()
            .AddCheck<KafkaHealthCheck>("kafka");

        return services;
    }

    public static IServiceCollection AddDatabaseMigrations(this IServiceCollection services)
    {
        services.AddScoped<IDatabaseMigrator, DatabaseMigrator>();
        return services;
    }
}