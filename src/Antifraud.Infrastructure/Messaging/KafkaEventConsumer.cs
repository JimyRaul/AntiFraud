using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Antifraud.Application.Interfaces;
using Antifraud.Infrastructure.Messaging.Configuration;

namespace Antifraud.Infrastructure.Messaging;

public class KafkaEventConsumer : IEventConsumer, IDisposable
{
    private readonly IConsumer<string, string> _consumer;
    private readonly ILogger<KafkaEventConsumer> _logger;
    private readonly KafkaConfiguration _configuration;
    private CancellationTokenSource? _cancellationTokenSource;

    public KafkaEventConsumer(
        IOptions<KafkaConfiguration> configuration,
        ILogger<KafkaEventConsumer> logger)
    {
        _configuration = configuration.Value ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var config = new ConsumerConfig
        {
            BootstrapServers = _configuration.BootstrapServers,
            GroupId = _configuration.ConsumerGroupId,
            ClientId = _configuration.ClientId,
            SecurityProtocol = SecurityProtocol.Plaintext,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            EnablePartitionEof = false,
            SessionTimeoutMs = 30000,
            HeartbeatIntervalMs = 10000
        };

        _consumer = new ConsumerBuilder<string, string>(config)
            .SetErrorHandler((_, e) => _logger.LogError("Kafka consumer error: {Error}", e.Reason))
            .SetPartitionsAssignedHandler((c, partitions) =>
            {
                _logger.LogInformation("Assigned partitions: [{Partitions}]", 
                    string.Join(", ", partitions.Select(p => $"{p.Topic}[{p.Partition}]")));
            })
            .SetPartitionsRevokedHandler((c, partitions) =>
            {
                _logger.LogInformation("Revoked partitions: [{Partitions}]", 
                    string.Join(", ", partitions.Select(p => $"{p.Topic}[{p.Partition}]")));
            })
            .Build();
    }

    public async Task StartAsync(string topic, Func<string, Task> messageHandler)
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _consumer.Subscribe(topic);

        _logger.LogInformation("Kafka consumer started for topic: {Topic}", topic);

        await Task.Run(async () =>
        {
            try
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = _consumer.Consume(_cancellationTokenSource.Token);

                        if (consumeResult?.Message != null)
                        {
                            _logger.LogDebug(
                                "Received message from topic {Topic}, partition {Partition}, offset {Offset}",
                                consumeResult.Topic, consumeResult.Partition.Value, consumeResult.Offset.Value);

                            await messageHandler(consumeResult.Message.Value);

                            _consumer.Commit(consumeResult);
                        }
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError(ex, "Error consuming message from Kafka: {Error}", ex.Error.Reason);
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogInformation("Kafka consumer operation was cancelled");
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unexpected error in Kafka consumer");
                        await Task.Delay(5000, _cancellationTokenSource.Token); // Wait before retrying
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Kafka consumer stopped");
            }
        }, _cancellationTokenSource.Token);
    }

    public async Task StopAsync()
    {
        _cancellationTokenSource?.Cancel();
        _consumer.Close();
        await Task.CompletedTask;
        _logger.LogInformation("Kafka consumer stopped");
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Cancel();
        _consumer?.Close();
        _consumer?.Dispose();
        _cancellationTokenSource?.Dispose();
    }
}
