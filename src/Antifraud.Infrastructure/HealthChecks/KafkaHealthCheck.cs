using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Confluent.Kafka;
using Antifraud.Infrastructure.Messaging.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Antifraud.Infrastructure.HealthChecks;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddKafka(this IHealthChecksBuilder builder, Action<KafkaHealthCheckOptions> configure)
    {
        var options = new KafkaHealthCheckOptions();
        configure?.Invoke(options);
        
        return builder.Services.AddSingleton<IHealthCheck>(serviceProvider =>
            new KafkaHealthCheck(options, serviceProvider.GetService<ILogger<KafkaHealthCheck>>()));
    }
}

public class KafkaHealthCheckOptions
{
    public string BootstrapServers { get; set; } = "localhost:9092";
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);
}

public class KafkaHealthCheck : IHealthCheck
{
    private readonly KafkaHealthCheckOptions _options;
    private readonly ILogger<KafkaHealthCheck>? _logger;

    public KafkaHealthCheck(KafkaHealthCheckOptions options, ILogger<KafkaHealthCheck>? logger = null)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var adminClient = new AdminClientBuilder(new AdminClientConfig
            {
                BootstrapServers = _options.BootstrapServers,
                SocketTimeoutMs = (int)_options.Timeout.TotalMilliseconds
            }).Build();

            // Intentar obtener metadata para verificar la conexión
            var metadata = adminClient.GetMetadata(_options.Timeout);
            
            if (metadata.Brokers?.Any() == true)
            {
                _logger?.LogDebug("Kafka health check passed. Connected to {BrokerCount} brokers", metadata.Brokers.Count);
                return HealthCheckResult.Healthy($"Connected to {metadata.Brokers.Count} Kafka brokers");
            }
            
            return HealthCheckResult.Unhealthy("No Kafka brokers available");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Kafka health check failed");
            return HealthCheckResult.Unhealthy($"Failed to connect to Kafka: {ex.Message}");
        }
    }
}