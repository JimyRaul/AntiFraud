using Xunit;
using FluentAssertions;
using Antifraud.Application.DTOs;

namespace Antifraud.Application.Tests.DTOs;

public class CreateTransactionRequestTests
{
    [Fact]
    public void CreateTransactionRequest_DefaultValues_ShouldHaveExpectedDefaults()
    {
        // Act
        var request = new CreateTransactionRequest();

        // Assert
        request.SourceAccountId.Should().Be(Guid.Empty);
        request.TargetAccountId.Should().Be(Guid.Empty);
        request.TransferTypeId.Should().Be(0);
        request.Value.Should().Be(0);
    }

    [Fact]
    public void CreateTransactionRequest_WithInitValues_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var sourceAccountId = Guid.NewGuid();
        var targetAccountId = Guid.NewGuid();
        var transferTypeId = 1;
        var value = 2500.75m;

        // Act
        var request = new CreateTransactionRequest
        {
            SourceAccountId = sourceAccountId,
            TargetAccountId = targetAccountId,
            TransferTypeId = transferTypeId,
            Value = value
        };

        // Assert
        request.SourceAccountId.Should().Be(sourceAccountId);
        request.TargetAccountId.Should().Be(targetAccountId);
        request.TransferTypeId.Should().Be(transferTypeId);
        request.Value.Should().Be(value);
    }

    [Fact]
    public void CreateTransactionRequest_RecordEquality_ShouldWorkCorrectly()
    {
        // Arrange
        var sourceAccountId = Guid.NewGuid();
        var targetAccountId = Guid.NewGuid();

        var request1 = new CreateTransactionRequest
        {
            SourceAccountId = sourceAccountId,
            TargetAccountId = targetAccountId,
            TransferTypeId = 1,
            Value = 1000m
        };

        var request2 = new CreateTransactionRequest
        {
            SourceAccountId = sourceAccountId,
            TargetAccountId = targetAccountId,
            TransferTypeId = 1,
            Value = 1000m
        };

        // Act & Assert
        request1.Should().Be(request2);
        (request1 == request2).Should().BeTrue();
    }

    [Fact]
    public void CreateTransactionRequest_DifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var request1 = new CreateTransactionRequest
        {
            SourceAccountId = Guid.NewGuid(),
            TargetAccountId = Guid.NewGuid(),
            TransferTypeId = 1,
            Value = 1000m
        };

        var request2 = new CreateTransactionRequest
        {
            SourceAccountId = request1.SourceAccountId,
            TargetAccountId = request1.TargetAccountId,
            TransferTypeId = 1,
            Value = 2000m // Different value
        };

        // Act & Assert
        request1.Should().NotBe(request2);
        (request1 != request2).Should().BeTrue();
    }
}