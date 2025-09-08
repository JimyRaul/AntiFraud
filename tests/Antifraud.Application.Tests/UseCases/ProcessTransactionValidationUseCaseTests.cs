using Xunit;
using FluentAssertions;
using Moq;
using Antifraud.Application.UseCases;
using Antifraud.Application.Interfaces;
using Antifraud.Application.DTOs;

namespace Antifraud.Application.Tests.UseCases;

public class ProcessTransactionValidationUseCaseTests
{
    private readonly Mock<ITransactionService> _mockTransactionService;
    private readonly ProcessTransactionValidationUseCase _useCase;

    public ProcessTransactionValidationUseCaseTests()
    {
        _mockTransactionService = new Mock<ITransactionService>();
        _useCase = new ProcessTransactionValidationUseCase(_mockTransactionService.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidApprovedEvent_ShouldReturnSuccessResult()
    {
        // Arrange
        var validationEvent = new TransactionValidationEvent
        {
            TransactionExternalId = Guid.NewGuid(),
            Status = "approved",
            Reason = null,
            ProcessedAt = DateTime.UtcNow
        };

        _mockTransactionService
            .Setup(s => s.ProcessTransactionValidationAsync(validationEvent))
            .ReturnsAsync(Result.Success(true));

        // Act
        var result = await _useCase.ExecuteAsync(validationEvent);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        _mockTransactionService.Verify(s => s.ProcessTransactionValidationAsync(validationEvent), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ValidRejectedEvent_ShouldReturnSuccessResult()
    {
        // Arrange
        var validationEvent = new TransactionValidationEvent
        {
            TransactionExternalId = Guid.NewGuid(),
            Status = "rejected",
            Reason = "Amount exceeds limit",
            ProcessedAt = DateTime.UtcNow
        };

        _mockTransactionService
            .Setup(s => s.ProcessTransactionValidationAsync(validationEvent))
            .ReturnsAsync(Result.Success(true));

        // Act
        var result = await _useCase.ExecuteAsync(validationEvent);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        _mockTransactionService.Verify(s => s.ProcessTransactionValidationAsync(validationEvent), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_EmptyTransactionId_ShouldReturnFailureResult()
    {
        // Arrange
        var validationEvent = new TransactionValidationEvent
        {
            TransactionExternalId = Guid.Empty,
            Status = "approved",
            ProcessedAt = DateTime.UtcNow
        };

        // Act
        var result = await _useCase.ExecuteAsync(validationEvent);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Transaction external ID cannot be empty");
        _mockTransactionService.Verify(s => s.ProcessTransactionValidationAsync(It.IsAny<TransactionValidationEvent>()), Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_EmptyStatus_ShouldReturnFailureResult(string invalidStatus)
    {
        // Arrange
        var validationEvent = new TransactionValidationEvent
        {
            TransactionExternalId = Guid.NewGuid(),
            Status = invalidStatus,
            ProcessedAt = DateTime.UtcNow
        };

        // Act
        var result = await _useCase.ExecuteAsync(validationEvent);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Transaction status cannot be empty");
        _mockTransactionService.Verify(s => s.ProcessTransactionValidationAsync(It.IsAny<TransactionValidationEvent>()), Times.Never);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("unknown")]
    [InlineData("processing")]
    public async Task ExecuteAsync_InvalidStatus_ShouldReturnFailureResult(string invalidStatus)
    {
        // Arrange
        var validationEvent = new TransactionValidationEvent
        {
            TransactionExternalId = Guid.NewGuid(),
            Status = invalidStatus,
            ProcessedAt = DateTime.UtcNow
        };

        // Act
        var result = await _useCase.ExecuteAsync(validationEvent);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain($"Invalid transaction status: {invalidStatus}");
        _mockTransactionService.Verify(s => s.ProcessTransactionValidationAsync(It.IsAny<TransactionValidationEvent>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceFailure_ShouldReturnFailureResult()
    {
        // Arrange
        var validationEvent = new TransactionValidationEvent
        {
            TransactionExternalId = Guid.NewGuid(),
            Status = "approved",
            ProcessedAt = DateTime.UtcNow
        };

        var serviceError = "Transaction not found";
        _mockTransactionService
            .Setup(s => s.ProcessTransactionValidationAsync(validationEvent))
            .ReturnsAsync(Result.Failure<bool>(serviceError));

        // Act
        var result = await _useCase.ExecuteAsync(validationEvent);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(serviceError);
        _mockTransactionService.Verify(s => s.ProcessTransactionValidationAsync(validationEvent), Times.Once);
    }

    [Fact]
    public void Constructor_NullTransactionService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new ProcessTransactionValidationUseCase(null!));
        exception.ParamName.Should().Be("transactionService");
    }
}