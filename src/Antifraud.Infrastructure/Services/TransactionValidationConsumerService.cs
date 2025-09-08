using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Antifraud.Application.Interfaces;
using Antifraud.Infrastructure.Messaging.Configuration;
using Antifraud.Infrastructure.Messaging.Handlers;

namespace Antifraud.Infrastructure.Services;

public class TransactionValidationConsumerService : IHostedService
{
    private readonly IEventConsumer _eventConsumer;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TransactionValidationConsumerService> _logger;
    private readonly KafkaConfiguration _kafkaConfiguration;
    private Task? _executingTask;
    private CancellationTokenSource? _cancellationTokenSource;

    public TransactionValidationConsumerService(
        IEventConsumer eventConsumer,
        IServiceProvider serviceProvider,
        ILogger<TransactionValidationConsumerService> logger,
        IOptions<KafkaConfiguration> kafkaConfiguration)
    {
        _eventConsumer = eventConsumer ?? throw new ArgumentNullException(nameof(eventConsumer));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _kafkaConfiguration = kafkaConfiguration.Value ?? throw new ArgumentNullException(nameof(kafkaConfiguration));
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting transaction validation consumer service");
        
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _executingTask = ExecuteAsync(_cancellationTokenSource.Token);

        return _executingTask.IsCompleted ? _executingTask : Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping transaction validation consumer service");
        
        if (_executingTask == null)
            return;

        _cancellationTokenSource?.Cancel();
        await _eventConsumer.StopAsync();
        
        await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
    }

    private async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var topic = _kafkaConfiguration.Topics["TransactionValidationResponse"];
            
            await _eventConsumer.StartAsync(topic, async message =>
            {
                using var scope = _serviceProvider.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<TransactionValidationHandler>();
                await handler.HandleAsync(message);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in transaction validation consumer service");
            throw;
        }
    }
}