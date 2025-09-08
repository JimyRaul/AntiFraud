namespace Antifraud.Infrastructure.Messaging.Configuration;

public class KafkaConfiguration
{
    public const string SectionName = "Kafka";

    public string BootstrapServers { get; set; } = "localhost:9092";
    public string ClientId { get; set; } = "antifraud-service";
    public string ConsumerGroupId { get; set; } = "antifraud-consumer-group";
    public Dictionary<string, string> Topics { get; set; } = new()
    {
        { "TransactionValidation", "transaction-validation" },
        { "TransactionValidationResponse", "transaction-validation-response" }
    };
}