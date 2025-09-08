using Xunit;
using FluentAssertions;
using Antifraud.Domain.ValueObjects;

namespace Antifraud.Domain.Tests.ValueObjects;

public class TransactionStatusTests
{
    [Theory]
    [InlineData("pending")]
    [InlineData("PENDING")]
    [InlineData("Pending")]
    public void From_ValidPendingStatus_ShouldReturnPending(string statusValue)
    {
        // Act
        var status = TransactionStatus.From(statusValue);

        // Assert
        status.Should().Be(TransactionStatus.Pending);
        status.IsPending.Should().BeTrue();
        status.IsApproved.Should().BeFalse();
        status.IsRejected.Should().BeFalse();
    }

    [Theory]
    [InlineData("approved")]
    [InlineData("APPROVED")]
    [InlineData("Approved")]
    public void From_ValidApprovedStatus_ShouldReturnApproved(string statusValue)
    {
        // Act
        var status = TransactionStatus.From(statusValue);

        // Assert
        status.Should().Be(TransactionStatus.Approved);
        status.IsApproved.Should().BeTrue();
        status.IsPending.Should().BeFalse();
        status.IsRejected.Should().BeFalse();
    }

    [Theory]
    [InlineData("rejected")]
    [InlineData("REJECTED")]
    [InlineData("Rejected")]
    public void From_ValidRejectedStatus_ShouldReturnRejected(string statusValue)
    {
        // Act
        var status = TransactionStatus.From(statusValue);

        // Assert
        status.Should().Be(TransactionStatus.Rejected);
        status.IsRejected.Should().BeTrue();
        status.IsPending.Should().BeFalse();
        status.IsApproved.Should().BeFalse();
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("unknown")]
    [InlineData("")]
    [InlineData(null)]
    public void From_InvalidStatus_ShouldThrowArgumentException(string invalidStatus)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => TransactionStatus.From(invalidStatus));
        exception.Message.Should().Contain("Invalid transaction status");
    }

    [Fact]
    public void ImplicitConversion_FromString_ShouldWork()
    {
        // Act
        TransactionStatus status = "pending";

        // Assert
        status.Should().Be(TransactionStatus.Pending);
    }

    [Fact]
    public void ImplicitConversion_ToString_ShouldWork()
    {
        // Arrange
        var status = TransactionStatus.Approved;

        // Act
        string statusString = status;

        // Assert
        statusString.Should().Be("approved");
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var status = TransactionStatus.Pending;

        // Act
        var result = status.ToString();

        // Assert
        result.Should().Be("pending");
    }

    [Fact]
    public void StaticValues_ShouldHaveCorrectValues()
    {
        // Assert
        TransactionStatus.Pending.Value.Should().Be("pending");
        TransactionStatus.Approved.Value.Should().Be("approved");
        TransactionStatus.Rejected.Value.Should().Be("rejected");
    }
}