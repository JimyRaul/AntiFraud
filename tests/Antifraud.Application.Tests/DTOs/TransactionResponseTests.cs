using Xunit;
using FluentAssertions;
using Antifraud.Application.DTOs;

namespace Antifraud.Application.Tests.DTOs;

public class TransactionResponseTests
{
    [Fact]
    public void TransactionResponse_DefaultValues_ShouldHaveExpectedDefaults()
    {
        // Act
        var response = new TransactionResponse { Status = string.Empty };

        // Assert
        response.TransactionExternalId.Should().Be(Guid.Empty);
        response.SourceAccountId.Should().Be(Guid.Empty);
        response.TargetAccountId.Should().Be(Guid.Empty);
        response.TransferTypeId.Should().Be(0);
        response.Value.Should().Be(0);
        response.Currency.Should().Be("USD");
        response.Status.Should().BeNull();
        response.CreatedAt.Should().Be(default);
        response.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void TransactionResponse_WithInitValues_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var sourceAccountId = Guid.NewGuid();
        var targetAccountId = Guid.NewGuid();
        var transferTypeId = 1;
        var value = 1500.50m;
        var currency = "EUR";
        var status = "approved";
        var createdAt = DateTime.UtcNow;
        var updatedAt = DateTime.UtcNow.AddMinutes(5);

        // Act
        var response = new TransactionResponse
        {
            TransactionExternalId = transactionId,
            SourceAccountId = sourceAccountId,
            TargetAccountId = targetAccountId,
            TransferTypeId = transferTypeId,
            Value = value,
            Currency = currency,
            Status = status,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        // Assert
        response.TransactionExternalId.Should().Be(transactionId);
        response.SourceAccountId.Should().Be(sourceAccountId);
        response.TargetAccountId.Should().Be(targetAccountId);
        response.TransferTypeId.Should().Be(transferTypeId);
        response.Value.Should().Be(value);
        response.Currency.Should().Be(currency);
        response.Status.Should().Be(status);
        response.CreatedAt.Should().Be(createdAt);
        response.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public void TransactionResponse_RecordEquality_ShouldWorkCorrectly()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var response1 = new TransactionResponse
        {
            TransactionExternalId = transactionId,
            SourceAccountId = Guid.NewGuid(),
            Value = 1000m,
            Status = "pending"
        };

        var response2 = new TransactionResponse
        {
            TransactionExternalId = transactionId,
            SourceAccountId = response1.SourceAccountId,
            Value = 1000m,
            Status = "pending"
        };

        // Act & Assert
        response1.Should().Be(response2);
        (response1 == response2).Should().BeTrue();
    }
}