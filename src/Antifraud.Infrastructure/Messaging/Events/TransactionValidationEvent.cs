using System.Text.Json.Serialization;

namespace Antifraud.Infrastructure.Messaging.Events;

public class TransactionValidationEvent
{
    [JsonPropertyName("transactionExternalId")]
    public Guid TransactionExternalId { get; set; }

    [JsonPropertyName("sourceAccountId")]
    public Guid SourceAccountId { get; set; }

    [JsonPropertyName("targetAccountId")]
    public Guid TargetAccountId { get; set; }

    [JsonPropertyName("transferTypeId")]
    public int TransferTypeId { get; set; }

    [JsonPropertyName("value")]
    public decimal Value { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "USD";

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
}