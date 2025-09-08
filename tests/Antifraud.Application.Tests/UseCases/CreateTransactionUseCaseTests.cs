using Xunit;
using FluentAssertions;
using Moq;
using Antifraud.Application.UseCases;
using Antifraud.Application.Interfaces;
using Antifraud.Application.DTOs;

namespace Antifraud.Application.Tests.UseCases;

public class CreateTransactionUseCaseTests
{
    private readonly Mock<ITransactionService> _mockTransactionService;
    private readonly CreateTransactionUseCase _useCase;

    public CreateTransactionUseCaseTests()
    {
        _mockTransactionService = new Mock<ITransactionService>();
        _useCase = new CreateTransactionUseCase(_mockTransactionService.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidRequest_ShouldReturnSuccessResult()
    {
        // Arrange
        var request = new CreateTransactionRequest
        {
            SourceAccountId = Guid.NewGuid(),
            TargetAccountId = Guid.NewGuid(),
            TransferTypeId = 1,
            Value = 1000m
        };

        var expectedResponse = new TransactionResponse
        {
            TransactionExternalId = Guid.NewGuid(),
            SourceAccountId = request.SourceAccountId,
            TargetAccountId = request.TargetAccountId,
            TransferTypeId = request.TransferTypeId,
            Value = request.Value,
            Status = "pending",
            CreatedAt = DateTime.UtcNow
        };

        _mockTransactionService
            .Setup(s => s.CreateTransactionAsync(request))
            .ReturnsAsync(Result.Success(expectedResponse));

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedResponse);
        _mockTransactionService.Verify(s => s.CreateTransactionAsync(request), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_SameSourceAndTargetAccount_ShouldReturnFailureResult()
    {
        // Arrange
        var sameAccountId = Guid.NewGuid();
        var request = new CreateTransactionRequest
        {
            SourceAccountId = sameAccountId,
            TargetAccountId = sameAccountId,
            TransferTypeId = 1,
            Value = 1000m
        };

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Source and target accounts cannot be the same");
        _mockTransactionService.Verify(s => s.CreateTransactionAsync(It.IsAny<CreateTransactionRequest>()), Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    [InlineData(-0.01)]
    public async Task ExecuteAsync_InvalidValue_ShouldReturnFailureResult(decimal invalidValue)
    {
        // Arrange
        var request = new CreateTransactionRequest
        {
            SourceAccountId = Guid.NewGuid(),
            TargetAccountId = Guid.NewGuid(),
            TransferTypeId = 1,
            Value = invalidValue
        };

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Transaction value must be greater than zero");
        _mockTransactionService.Verify(s => s.CreateTransactionAsync(It.IsAny<CreateTransactionRequest>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceFailure_ShouldReturnFailureResult()
    {
        // Arrange
        var request = new CreateTransactionRequest
        {
            SourceAccountId = Guid.NewGuid(),
            TargetAccountId = Guid.NewGuid(),
            TransferTypeId = 1,
            Value = 1000m
        };

        var serviceError = "Account not found";
        _mockTransactionService
            .Setup(s => s.CreateTransactionAsync(request))
            .ReturnsAsync(Result.Failure<TransactionResponse>(serviceError));

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(serviceError);
        _mockTransactionService.Verify(s => s.CreateTransactionAsync(request), Times.Once);
    }

    [Fact]
    public void Constructor_NullTransactionService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new CreateTransactionUseCase(null!));
        exception.ParamName.Should().Be("transactionService");
    }

    [Fact]
    public async Task ExecuteAsync_ValidPositiveValue_ShouldCallService()
    {
        // Arrange
        var request = new CreateTransactionRequest
        {
            SourceAccountId = Guid.NewGuid(),
            TargetAccountId = Guid.NewGuid(),
            TransferTypeId = 1,
            Value = 0.01m // Minimum valid value
        };

        var expectedResponse = new TransactionResponse
        {
            TransactionExternalId = Guid.NewGuid(),
            Status = "pending",
            CreatedAt = DateTime.UtcNow
        };

        _mockTransactionService
            .Setup(s => s.CreateTransactionAsync(request))
            .ReturnsAsync(Result.Success(expectedResponse));

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockTransactionService.Verify(s => s.CreateTransactionAsync(request), Times.Once);
    }
}