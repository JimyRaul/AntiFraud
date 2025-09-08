using Xunit;
using FluentAssertions;
using Antifraud.Domain.ValueObjects;

namespace Antifraud.Domain.Tests.ValueObjects;

public class TransactionIdTests
{
    [Fact]
    public void New_ShouldCreateUniqueIds()
    {
        // Act
        var id1 = TransactionId.New();
        var id2 = TransactionId.New();

        // Assert
        id1.Should().NotBe(id2);
        id1.Value.Should().NotBe(Guid.Empty);
        id2.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void From_ValidGuid_ShouldCreateTransactionId()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var transactionId = TransactionId.From(guid);

        // Assert
        transactionId.Value.Should().Be(guid);
    }

    [Fact]
    public void From_EmptyGuid_ShouldThrowArgumentException()
    {
        // Arrange
        var emptyGuid = Guid.Empty;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => TransactionId.From(emptyGuid));
        exception.Message.Should().Contain("Transaction ID cannot be empty");
    }

    [Fact]
    public void ImplicitConversion_FromGuid_ShouldWork()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        TransactionId transactionId = guid;

        // Assert
        transactionId.Value.Should().Be(guid);
    }

    [Fact]
    public void ImplicitConversion_ToGuid_ShouldWork()
    {
        // Arrange
        var transactionId = TransactionId.New();

        // Act
        Guid guid = transactionId;

        // Assert
        guid.Should().Be(transactionId.Value);
    }

    [Fact]
    public void ToString_ShouldReturnGuidString()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var transactionId = TransactionId.From(guid);

        // Act
        var result = transactionId.ToString();

        // Assert
        result.Should().Be(guid.ToString());
    }

    [Fact]
    public void Equality_SameValue_ShouldBeEqual()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id1 = TransactionId.From(guid);
        var id2 = TransactionId.From(guid);

        // Act & Assert
        id1.Should().Be(id2);
        (id1 == id2).Should().BeTrue();
    }

    [Fact]
    public void Equality_DifferentValue_ShouldNotBeEqual()
    {
        // Arrange
        var id1 = TransactionId.New();
        var id2 = TransactionId.New();

        // Act & Assert
        id1.Should().NotBe(id2);
        (id1 != id2).Should().BeTrue();
    }
}