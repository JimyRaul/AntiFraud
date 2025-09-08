using Xunit;
using FluentAssertions;
using Moq;
using Antifraud.Application.UseCases;
using Antifraud.Application.Interfaces;
using Antifraud.Application.DTOs;

namespace Antifraud.Application.Tests.UseCases;

public class GetTransactionUseCaseTests
{
    private readonly Mock<ITransactionService> _mockTransactionService;
    private readonly GetTransactionUseCase _useCase;

    public GetTransactionUseCaseTests()
    {
        _mockTransactionService = new Mock<ITransactionService>();
        _useCase = new GetTransactionUseCase(_mockTransactionService.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidRequest_ShouldReturnSuccessResult()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var request = new GetTransactionRequest
        {
            TransactionExternalId = transactionId,
            CreatedAt = DateTime.UtcNow
        };

        var expectedResponse = new TransactionResponse
        {
            TransactionExternalId = transactionId,
            SourceAccountId = Guid.NewGuid(),
            TargetAccountId = Guid.NewGuid(),
            TransferTypeId = 1,
            Value = 1000m,
            Status = "approved",
            CreatedAt = DateTime.UtcNow
        };

        _mockTransactionService
            .Setup(s => s.GetTransactionAsync(request))
            .ReturnsAsync(Result.Success(expectedResponse));

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedResponse);
        _mockTransactionService.Verify(s => s.GetTransactionAsync(request), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_EmptyTransactionId_ShouldReturnFailureResult()
    {
        // Arrange
        var request = new GetTransactionRequest
        {
            TransactionExternalId = Guid.Empty,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Transaction external ID cannot be empty");
        _mockTransactionService.Verify(s => s.GetTransactionAsync(It.IsAny<GetTransactionRequest>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceFailure_ShouldReturnFailureResult()
    {
        // Arrange
        var request = new GetTransactionRequest
        {
            TransactionExternalId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };

        var serviceError = "Transaction not found";
        _mockTransactionService
            .Setup(s => s.GetTransactionAsync(request))
            .ReturnsAsync(Result.Failure<TransactionResponse>(serviceError));

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(serviceError);
        _mockTransactionService.Verify(s => s.GetTransactionAsync(request), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_RequestWithoutCreatedAt_ShouldStillWork()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var request = new GetTransactionRequest
        {
            TransactionExternalId = transactionId,
            CreatedAt = null
        };

        var expectedResponse = new TransactionResponse
        {
            TransactionExternalId = transactionId,
            Status = "approved",
            CreatedAt = DateTime.UtcNow
        };

        _mockTransactionService
            .Setup(s => s.GetTransactionAsync(request))
            .ReturnsAsync(Result.Success(expectedResponse));

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedResponse);
        _mockTransactionService.Verify(s => s.GetTransactionAsync(request), Times.Once);
    }

    [Fact]
    public void Constructor_NullTransactionService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new GetTransactionUseCase(null!));
        exception.ParamName.Should().Be("transactionService");
    }
}