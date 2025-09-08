using Xunit;
using FluentAssertions;
using Moq;
using Antifraud.Domain.Entities;
using Antifraud.Domain.Services;
using Antifraud.Domain.Repositories;
using Antifraud.Domain.ValueObjects;

namespace Antifraud.Domain.Tests.Services;

public class TransactionDomainServiceTests
{
    private readonly Mock<ITransactionRepository> _mockRepository;
    private readonly TransactionDomainService _service;
    private readonly AccountId _sourceAccountId = AccountId.From(Guid.NewGuid());
    private readonly AccountId _targetAccountId = AccountId.From(Guid.NewGuid());
    private readonly TransferTypeId _transferTypeId = TransferTypeId.From(1);

    public TransactionDomainServiceTests()
    {
        _mockRepository = new Mock<ITransactionRepository>();
        _service = new TransactionDomainService(_mockRepository.Object);
    }

    [Theory]
    [InlineData(1999.99)]
    [InlineData(2000.00)]
    [InlineData(1500.00)]
    [InlineData(0.01)]
    public async Task ShouldRejectTransaction_AmountLessThanOrEqualTo2000_ShouldReturnFalse(decimal amount)
    {
        // Arrange
        var transaction = Transaction.Create(_sourceAccountId, _targetAccountId, _transferTypeId, Money.From(amount));
        _mockRepository.Setup(r => r.GetDailyAccumulatedAmountAsync(It.IsAny<AccountId>(), It.IsAny<DateTime>()))
                      .ReturnsAsync(Money.Zero);

        // Act
        var result = await _service.ShouldRejectTransactionAsync(transaction);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(2000.01)]
    [InlineData(2500.00)]
    [InlineData(5000.00)]
    [InlineData(10000.00)]
    public async Task ShouldRejectTransaction_AmountGreaterThan2000_ShouldReturnTrue(decimal amount)
    {
        // Arrange
        var transaction = Transaction.Create(_sourceAccountId, _targetAccountId, _transferTypeId, Money.From(amount));

        // Act
        var result = await _service.ShouldRejectTransactionAsync(transaction);

        // Assert
        result.Should().BeTrue();
        // Should not call repository when amount is already over limit
        _mockRepository.Verify(r => r.GetDailyAccumulatedAmountAsync(It.IsAny<AccountId>(), It.IsAny<DateTime>()), 
                              Times.Never);
    }

    [Fact]
    public async Task ShouldRejectTransaction_DailyLimitExceeded_ShouldReturnTrue()
    {
        // Arrange
        var transactionAmount = Money.From(1000m); // Valid amount individually
        var currentDailyAccumulated = Money.From(19500m); // Already close to limit
        var transaction = Transaction.Create(_sourceAccountId, _targetAccountId, _transferTypeId, transactionAmount);

        _mockRepository.Setup(r => r.GetDailyAccumulatedAmountAsync(_sourceAccountId, It.IsAny<DateTime>()))
                      .ReturnsAsync(currentDailyAccumulated);

        // Act
        var result = await _service.ShouldRejectTransactionAsync(transaction);

        // Assert
        result.Should().BeTrue(); // 19500 + 1000 = 20500 > 20000
        _mockRepository.Verify(r => r.GetDailyAccumulatedAmountAsync(_sourceAccountId, transaction.CreatedAt.Date), 
                              Times.Once);
    }

    [Fact]
    public async Task ShouldRejectTransaction_DailyLimitNotExceeded_ShouldReturnFalse()
    {
        // Arrange
        var transactionAmount = Money.From(1000m);
        var currentDailyAccumulated = Money.From(18000m); // Safe amount
        var transaction = Transaction.Create(_sourceAccountId, _targetAccountId, _transferTypeId, transactionAmount);

        _mockRepository.Setup(r => r.GetDailyAccumulatedAmountAsync(_sourceAccountId, It.IsAny<DateTime>()))
                      .ReturnsAsync(currentDailyAccumulated);

        // Act
        var result = await _service.ShouldRejectTransactionAsync(transaction);

        // Assert
        result.Should().BeFalse(); // 18000 + 1000 = 19000 <= 20000
        _mockRepository.Verify(r => r.GetDailyAccumulatedAmountAsync(_sourceAccountId, transaction.CreatedAt.Date), 
                              Times.Once);
    }

    [Fact]
    public async Task ShouldRejectTransaction_ExactlyAtDailyLimit_ShouldReturnFalse()
    {
        // Arrange
        var transactionAmount = Money.From(1000m);
        var currentDailyAccumulated = Money.From(19000m);
        var transaction = Transaction.Create(_sourceAccountId, _targetAccountId, _transferTypeId, transactionAmount);

        _mockRepository.Setup(r => r.GetDailyAccumulatedAmountAsync(_sourceAccountId, It.IsAny<DateTime>()))
                      .ReturnsAsync(currentDailyAccumulated);

        // Act
        var result = await _service.ShouldRejectTransactionAsync(transaction);

        // Assert
        result.Should().BeFalse(); // 19000 + 1000 = 20000 = limit (exactly at limit should be allowed)
    }

    [Fact]
    public async Task GetRejectionReason_AmountExceedsLimit_ShouldReturnAmountReason()
    {
        // Arrange
        var transactionAmount = Money.From(2500m);
        var transaction = Transaction.Create(_sourceAccountId, _targetAccountId, _transferTypeId, transactionAmount);

        // Act
        var reason = await _service.GetRejectionReasonAsync(transaction);

        // Assert
        reason.Should().Contain("Transaction amount");
        reason.Should().Contain("2500");
        reason.Should().Contain("exceeds the maximum allowed amount");
        reason.Should().Contain("2000");
    }

    [Fact]
    public async Task GetRejectionReason_DailyLimitExceeded_ShouldReturnDailyLimitReason()
    {
        // Arrange
        var transactionAmount = Money.From(1000m);
        var currentDailyAccumulated = Money.From(19500m);
        var transaction = Transaction.Create(_sourceAccountId, _targetAccountId, _transferTypeId, transactionAmount);

        _mockRepository.Setup(r => r.GetDailyAccumulatedAmountAsync(_sourceAccountId, It.IsAny<DateTime>()))
                      .ReturnsAsync(currentDailyAccumulated);

        // Act
        var reason = await _service.GetRejectionReasonAsync(transaction);

        // Assert
        reason.Should().Contain("Daily accumulated amount");
        reason.Should().Contain("20500"); // 19500 + 1000
        reason.Should().Contain("would exceed the maximum daily limit");
        reason.Should().Contain("20000");
        reason.Should().Contain("Current daily total: 19500");
    }

    [Fact]
    public async Task GetRejectionReason_ValidTransaction_ShouldReturnEmptyString()
    {
        // Arrange
        var transactionAmount = Money.From(1000m);
        var currentDailyAccumulated = Money.From(5000m);
        var transaction = Transaction.Create(_sourceAccountId, _targetAccountId, _transferTypeId, transactionAmount);

        _mockRepository.Setup(r => r.GetDailyAccumulatedAmountAsync(_sourceAccountId, It.IsAny<DateTime>()))
                      .ReturnsAsync(currentDailyAccumulated);

        // Act
        var reason = await _service.GetRejectionReasonAsync(transaction);

        // Assert
        reason.Should().BeEmpty();
    }

    [Fact]
    public async Task GetRejectionReason_AmountLimitTakesPrecedence_ShouldReturnAmountReason()
    {
        // Arrange - Transaction that violates both rules
        var transactionAmount = Money.From(3000m); // > 2000 (amount limit)
        var currentDailyAccumulated = Money.From(19000m); // Would also exceed daily limit
        var transaction = Transaction.Create(_sourceAccountId, _targetAccountId, _transferTypeId, transactionAmount);

        // Act
        var reason = await _service.GetRejectionReasonAsync(transaction);

        // Assert
        reason.Should().Contain("Transaction amount");
        reason.Should().Contain("$3,000.00 USD"); // Format with currency and USD
        reason.Should().NotContain("Daily accumulated"); // Should not check daily limit if amount is already over
    }

    [Theory]
    [InlineData(20000.01)] // Just over the limit
    [InlineData(25000.00)] // Well over the limit
    [InlineData(30000.00)] // Far over the limit
    public async Task ShouldRejectTransaction_DailyLimitExceededVariousAmounts_ShouldReturnTrue(decimal dailyAccumulated)
    {
        // Arrange
        var transactionAmount = Money.From(500m); // Small valid amount
        var currentDailyAccumulated = Money.From(dailyAccumulated);
        var transaction = Transaction.Create(_sourceAccountId, _targetAccountId, _transferTypeId, transactionAmount);

        _mockRepository.Setup(r => r.GetDailyAccumulatedAmountAsync(_sourceAccountId, It.IsAny<DateTime>()))
                      .ReturnsAsync(currentDailyAccumulated);

        // Act
        var result = await _service.ShouldRejectTransactionAsync(transaction);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ShouldRejectTransaction_ZeroDailyAccumulated_ShouldOnlyCheckAmountLimit()
    {
        // Arrange
        var transactionAmount = Money.From(1500m); // Valid amount
        var transaction = Transaction.Create(_sourceAccountId, _targetAccountId, _transferTypeId, transactionAmount);

        _mockRepository.Setup(r => r.GetDailyAccumulatedAmountAsync(_sourceAccountId, It.IsAny<DateTime>()))
                      .ReturnsAsync(Money.Zero);

        // Act
        var result = await _service.ShouldRejectTransactionAsync(transaction);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(r => r.GetDailyAccumulatedAmountAsync(_sourceAccountId, transaction.CreatedAt.Date), 
                              Times.Once);
    }

    [Fact]
    public async Task ShouldRejectTransaction_RepositoryException_ShouldThrow()
    {
        // Arrange
        var transactionAmount = Money.From(1000m);
        var transaction = Transaction.Create(_sourceAccountId, _targetAccountId, _transferTypeId, transactionAmount);

        _mockRepository.Setup(r => r.GetDailyAccumulatedAmountAsync(It.IsAny<AccountId>(), It.IsAny<DateTime>()))
                      .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _service.ShouldRejectTransactionAsync(transaction));
    }

    [Fact]
    public void Constructor_NullRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => 
            new TransactionDomainService(null!));
        exception.ParamName.Should().Be("transactionRepository");
    }

    [Fact]
    public async Task ShouldRejectTransaction_CallsRepositoryWithCorrectParameters()
    {
        // Arrange
        var transactionAmount = Money.From(1000m);
        var transaction = Transaction.Create(_sourceAccountId, _targetAccountId, _transferTypeId, transactionAmount);
        var expectedDate = transaction.CreatedAt.Date;

        _mockRepository.Setup(r => r.GetDailyAccumulatedAmountAsync(_sourceAccountId, expectedDate))
                      .ReturnsAsync(Money.Zero);

        // Act
        await _service.ShouldRejectTransactionAsync(transaction);

        // Assert
        _mockRepository.Verify(r => r.GetDailyAccumulatedAmountAsync(_sourceAccountId, expectedDate), 
                              Times.Once);
    }

    [Fact]
    public async Task GetRejectionReason_CallsRepositoryWithCorrectParameters()
    {
        // Arrange
        var transactionAmount = Money.From(1000m);
        var transaction = Transaction.Create(_sourceAccountId, _targetAccountId, _transferTypeId, transactionAmount);
        var expectedDate = transaction.CreatedAt.Date;

        _mockRepository.Setup(r => r.GetDailyAccumulatedAmountAsync(_sourceAccountId, expectedDate))
                      .ReturnsAsync(Money.From(19500m));

        // Act
        await _service.GetRejectionReasonAsync(transaction);

        // Assert
        _mockRepository.Verify(r => r.GetDailyAccumulatedAmountAsync(_sourceAccountId, expectedDate), 
                              Times.Once);
    }
}