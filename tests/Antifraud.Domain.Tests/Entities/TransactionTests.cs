using Xunit;
using FluentAssertions;
using Antifraud.Domain.Entities;
using Antifraud.Domain.ValueObjects;
using Antifraud.Domain.Events;

namespace Antifraud.Domain.Tests.Entities;

public class TransactionTests
{
    private readonly AccountId _sourceAccountId = AccountId.From(Guid.NewGuid());
    private readonly AccountId _targetAccountId = AccountId.From(Guid.NewGuid());
    private readonly TransferTypeId _transferTypeId = TransferTypeId.From(1);
    private readonly Money _value = Money.From(1000m);

    [Fact]
    public void Create_ValidParameters_ShouldCreateTransaction()
    {
        // Act
        var transaction = Transaction.Create(_sourceAccountId, _targetAccountId, _transferTypeId, _value);

        // Assert
        transaction.Should().NotBeNull();
        transaction.SourceAccountId.Should().Be(_sourceAccountId);
        transaction.TargetAccountId.Should().Be(_targetAccountId);
        transaction.TransferTypeId.Should().Be(_transferTypeId);
        transaction.Value.Should().Be(_value);
        transaction.Status.Should().Be(TransactionStatus.Pending);
        transaction.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        transaction.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void Create_ValidParameters_ShouldGenerateTransactionCreatedEvent()
    {
        // Act
        var transaction = Transaction.Create(_sourceAccountId, _targetAccountId, _transferTypeId, _value);

        // Assert
        transaction.DomainEvents.Should().HaveCount(1);
        var domainEvent = transaction.DomainEvents.First();
        domainEvent.Should().BeOfType<TransactionCreatedEvent>();
        
        var createdEvent = (TransactionCreatedEvent)domainEvent;
        createdEvent.TransactionId.Should().Be(transaction.Id);
        createdEvent.SourceAccountId.Should().Be(_sourceAccountId);
        createdEvent.TargetAccountId.Should().Be(_targetAccountId);
        createdEvent.TransferTypeId.Should().Be(_transferTypeId);
        createdEvent.Value.Should().Be(_value);
    }

    [Fact]
    public void Create_SameSourceAndTargetAccount_ShouldThrowArgumentException()
    {
        // Arrange
        var sameAccountId = AccountId.From(Guid.NewGuid());

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            Transaction.Create(sameAccountId, sameAccountId, _transferTypeId, _value));
        exception.Message.Should().Contain("Source and target accounts cannot be the same");
    }

    [Fact]
    public void Create_ZeroValue_ShouldThrowArgumentException()
    {
        // Arrange
        var zeroValue = Money.From(0m);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            Transaction.Create(_sourceAccountId, _targetAccountId, _transferTypeId, zeroValue));
        exception.Message.Should().Contain("Transaction value must be greater than zero");
    }

    [Fact]
    public void Approve_PendingTransaction_ShouldApproveAndGenerateEvent()
    {
        // Arrange
        var transaction = Transaction.Create(_sourceAccountId, _targetAccountId, _transferTypeId, _value);
        transaction.ClearDomainEvents(); // Clear creation event

        // Act
        transaction.Approve();

        // Assert
        transaction.Status.Should().Be(TransactionStatus.Approved);
        transaction.UpdatedAt.Should().NotBeNull();
        transaction.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        
        transaction.DomainEvents.Should().HaveCount(1);
        var statusEvent = (TransactionStatusUpdatedEvent)transaction.DomainEvents.First();
        statusEvent.TransactionId.Should().Be(transaction.Id);
        statusEvent.PreviousStatus.Should().Be(TransactionStatus.Pending);
        statusEvent.NewStatus.Should().Be(TransactionStatus.Approved);
    }

    [Fact]
    public void Approve_NonPendingTransaction_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var transaction = Transaction.Create(_sourceAccountId, _targetAccountId, _transferTypeId, _value);
        transaction.Approve(); // First approval

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => transaction.Approve());
        exception.Message.Should().Contain("Only pending transactions can be approved");
    }

    [Fact]
    public void Reject_PendingTransactionWithReason_ShouldRejectAndGenerateEvent()
    {
        // Arrange
        var transaction = Transaction.Create(_sourceAccountId, _targetAccountId, _transferTypeId, _value);
        transaction.ClearDomainEvents(); // Clear creation event
        var rejectionReason = "Amount exceeds limit";

        // Act
        transaction.Reject(rejectionReason);

        // Assert
        transaction.Status.Should().Be(TransactionStatus.Rejected);
        transaction.UpdatedAt.Should().NotBeNull();
        
        transaction.DomainEvents.Should().HaveCount(1);
        var statusEvent = (TransactionStatusUpdatedEvent)transaction.DomainEvents.First();
        statusEvent.TransactionId.Should().Be(transaction.Id);
        statusEvent.PreviousStatus.Should().Be(TransactionStatus.Pending);
        statusEvent.NewStatus.Should().Be(TransactionStatus.Rejected);
        statusEvent.Reason.Should().Be(rejectionReason);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Reject_EmptyReason_ShouldThrowArgumentException(string invalidReason)
    {
        // Arrange
        var transaction = Transaction.Create(_sourceAccountId, _targetAccountId, _transferTypeId, _value);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => transaction.Reject(invalidReason));
        exception.Message.Should().Contain("Rejection reason is required");
    }

    [Fact]
    public void Reject_NonPendingTransaction_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var transaction = Transaction.Create(_sourceAccountId, _targetAccountId, _transferTypeId, _value);
        transaction.Approve(); // First make it approved

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => transaction.Reject("Some reason"));
        exception.Message.Should().Contain("Only pending transactions can be rejected");
    }

    [Fact]
    public void UpdateStatus_SameStatus_ShouldNotGenerateEvent()
    {
        // Arrange
        var transaction = Transaction.Create(_sourceAccountId, _targetAccountId, _transferTypeId, _value);
        transaction.ClearDomainEvents();
        var initialUpdatedAt = transaction.UpdatedAt;

        // Act
        transaction.UpdateStatus(TransactionStatus.Pending);

        // Assert
        transaction.Status.Should().Be(TransactionStatus.Pending);
        transaction.UpdatedAt.Should().Be(initialUpdatedAt);
        transaction.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void IsFromAccount_SourceAccount_ShouldReturnTrue()
    {
        // Arrange
        var transaction = Transaction.Create(_sourceAccountId, _targetAccountId, _transferTypeId, _value);

        // Act
        var result = transaction.IsFromAccount(_sourceAccountId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsFromAccount_DifferentAccount_ShouldReturnFalse()
    {
        // Arrange
        var transaction = Transaction.Create(_sourceAccountId, _targetAccountId, _transferTypeId, _value);
        var differentAccountId = AccountId.From(Guid.NewGuid());

        // Act
        var result = transaction.IsFromAccount(differentAccountId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsToAccount_TargetAccount_ShouldReturnTrue()
    {
        // Arrange
        var transaction = Transaction.Create(_sourceAccountId, _targetAccountId, _transferTypeId, _value);

        // Act
        var result = transaction.IsToAccount(_targetAccountId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void InvolvesAccount_SourceOrTargetAccount_ShouldReturnTrue()
    {
        // Arrange
        var transaction = Transaction.Create(_sourceAccountId, _targetAccountId, _transferTypeId, _value);

        // Act & Assert
        transaction.InvolvesAccount(_sourceAccountId).Should().BeTrue();
        transaction.InvolvesAccount(_targetAccountId).Should().BeTrue();
    }

    [Fact]
    public void InvolvesAccount_DifferentAccount_ShouldReturnFalse()
    {
        // Arrange
        var transaction = Transaction.Create(_sourceAccountId, _targetAccountId, _transferTypeId, _value);
        var differentAccountId = AccountId.From(Guid.NewGuid());

        // Act
        var result = transaction.InvolvesAccount(differentAccountId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsCreatedOnDate_SameDate_ShouldReturnTrue()
    {
        // Arrange
        var transaction = Transaction.Create(_sourceAccountId, _targetAccountId, _transferTypeId, _value);
        var creationDate = transaction.CreatedAt.Date;

        // Act
        var result = transaction.IsCreatedOnDate(creationDate);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsCreatedOnDate_DifferentDate_ShouldReturnFalse()
    {
        // Arrange
        var transaction = Transaction.Create(_sourceAccountId, _targetAccountId, _transferTypeId, _value);
        var differentDate = DateTime.UtcNow.AddDays(-1);

        // Act
        var result = transaction.IsCreatedOnDate(differentDate);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        // Arrange
        var transaction = Transaction.Create(_sourceAccountId, _targetAccountId, _transferTypeId, _value);
        transaction.DomainEvents.Should().HaveCount(1); // Creation event

        // Act
        transaction.ClearDomainEvents();

        // Assert
        transaction.DomainEvents.Should().BeEmpty();
    }
}