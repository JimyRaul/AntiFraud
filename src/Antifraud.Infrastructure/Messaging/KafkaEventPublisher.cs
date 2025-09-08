using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Antifraud.Application.Interfaces;
using Antifraud.Infrastructure.Messaging.Configuration;

namespace Antifraud.Infrastructure.Messaging;

public class KafkaEventPublisher : IEventPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaEventPublisher> _logger;
    private readonly KafkaConfiguration _configuration;

    public KafkaEventPublisher(
        IOptions<KafkaConfiguration> configuration,
        ILogger<KafkaEventPublisher> logger)
    {
        _configuration = configuration.Value ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var config = new ProducerConfig
        {
            BootstrapServers = _configuration.BootstrapServers,
            ClientId = _configuration.ClientId,
            SecurityProtocol = SecurityProtocol.Plaintext,
            Acks = Acks.All,
            RetryBackoffMs = 1000,
            MessageSendMaxRetries = 3,
            RequestTimeoutMs = 30000,
            EnableIdempotence = true
        };

        _producer = new ProducerBuilder<string, string>(config)
            .SetErrorHandler((_, e) => _logger.LogError("Kafka producer error: {Error}", e.Reason))
            .Build();
    }

    public async Task PublishAsync<T>(T @event, string topic) where T : class
    {
        try
        {
            var message = JsonSerializer.Serialize(@event, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var kafkaMessage = new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(),
                Value = message,
                Timestamp = new Timestamp(DateTime.UtcNow)
            };

            var result = await _producer.ProduceAsync(topic, kafkaMessage);

            _logger.LogInformation(
                "Event published to Kafka. Topic: {Topic}, Partition: {Partition}, Offset: {Offset}",
                result.Topic, result.Partition.Value, result.Offset.Value);
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(ex, 
                "Failed to publish event to Kafka. Topic: {Topic}, Error: {Error}", 
                topic, ex.Error.Reason);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Unexpected error publishing event to Kafka. Topic: {Topic}", topic);
            throw;
        }
    }

    public void Dispose()
    {
        _producer?.Flush(TimeSpan.FromSeconds(30));
        _producer?.Dispose();
    }
}
