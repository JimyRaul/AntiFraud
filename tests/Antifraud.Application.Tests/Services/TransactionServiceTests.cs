using Xunit;
using FluentAssertions;
using Moq;
using Antifraud.Application.Services;
using Antifraud.Application.Interfaces;
using Antifraud.Application.DTOs;
using Antifraud.Domain.Entities;
using Antifraud.Domain.Repositories;
using Antifraud.Domain.ValueObjects;

namespace Antifraud.Application.Tests.Services;

public class TransactionServiceTests
{
    private readonly Mock<ITransactionRepository> _mockTransactionRepository;
    private readonly Mock<IAccountRepository> _mockAccountRepository;
    private readonly Mock<IAntifraudService> _mockAntifraudService;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly TransactionService _service;

    public TransactionServiceTests()
    {
        _mockTransactionRepository = new Mock<ITransactionRepository>();
        _mockAccountRepository = new Mock<IAccountRepository>();
        _mockAntifraudService = new Mock<IAntifraudService>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        _service = new TransactionService(
            _mockTransactionRepository.Object,
            _mockAccountRepository.Object,
            _mockAntifraudService.Object,
            _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task CreateTransactionAsync_ValidRequest_ShouldReturnSuccessResult()
    {
        // Arrange
        var request = new CreateTransactionRequest
        {
            SourceAccountId = Guid.NewGuid(),
            TargetAccountId = Guid.NewGuid(),
            TransferTypeId = 1,
            Value = 1000m
        };

        _mockAccountRepository.Setup(r => r.ExistsAsync(It.IsAny<AccountId>())).ReturnsAsync(true);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _service.CreateTransactionAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.SourceAccountId.Should().Be(request.SourceAccountId);
        result.Value.TargetAccountId.Should().Be(request.TargetAccountId);
        result.Value.TransferTypeId.Should().Be(request.TransferTypeId);
        result.Value.Value.Should().Be(request.Value);
        result.Value.Status.Should().Be("pending");

        _mockTransactionRepository.Verify(r => r.AddAsync(It.IsAny<Transaction>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        _mockAntifraudService.Verify(s => s.ProcessTransactionForValidationAsync(It.IsAny<TransactionResponse>()), Times.Once);
    }

    [Fact]
    public async Task CreateTransactionAsync_SourceAccountNotExists_ShouldReturnFailureResult()
    {
        // Arrange
        var request = new CreateTransactionRequest
        {
            SourceAccountId = Guid.NewGuid(),
            TargetAccountId = Guid.NewGuid(),
            TransferTypeId = 1,
            Value = 1000m
        };

        _mockAccountRepository.Setup(r => r.ExistsAsync(AccountId.From(request.SourceAccountId))).ReturnsAsync(false);
        _mockAccountRepository.Setup(r => r.ExistsAsync(AccountId.From(request.TargetAccountId))).ReturnsAsync(true);

        // Act
        var result = await _service.CreateTransactionAsync(request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain($"Source account {request.SourceAccountId} does not exist");

        _mockTransactionRepository.Verify(r => r.AddAsync(It.IsAny<Transaction>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateTransactionAsync_TargetAccountNotExists_ShouldReturnFailureResult()
    {
        // Arrange
        var request = new CreateTransactionRequest
        {
            SourceAccountId = Guid.NewGuid(),
            TargetAccountId = Guid.NewGuid(),
            TransferTypeId = 1,
            Value = 1000m
        };

        _mockAccountRepository.Setup(r => r.ExistsAsync(AccountId.From(request.SourceAccountId))).ReturnsAsync(true);
        _mockAccountRepository.Setup(r => r.ExistsAsync(AccountId.From(request.TargetAccountId))).ReturnsAsync(false);

        // Act
        var result = await _service.CreateTransactionAsync(request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain($"Target account {request.TargetAccountId} does not exist");

        _mockTransactionRepository.Verify(r => r.AddAsync(It.IsAny<Transaction>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateTransactionAsync_InvalidTransactionData_ShouldReturnFailureResult()
    {
        // Arrange
        var request = new CreateTransactionRequest
        {
            SourceAccountId = Guid.NewGuid(),
            TargetAccountId = Guid.NewGuid(),
            TransferTypeId = 0, // Invalid transfer type
            Value = 1000m
        };

        _mockAccountRepository.Setup(r => r.ExistsAsync(It.IsAny<AccountId>())).ReturnsAsync(true);

        // Act
        var result = await _service.CreateTransactionAsync(request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Transfer type ID must be greater than zero");

        _mockTransactionRepository.Verify(r => r.AddAsync(It.IsAny<Transaction>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task GetTransactionAsync_ExistingTransaction_ShouldReturnSuccessResult()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var request = new GetTransactionRequest
        {
            TransactionExternalId = transactionId
        };

        var transaction = Transaction.Create(
            AccountId.From(Guid.NewGuid()),
            AccountId.From(Guid.NewGuid()),
            TransferTypeId.From(1),
            Money.From(1000m));

        _mockTransactionRepository.Setup(r => r.GetByExternalIdAsync(transactionId)).ReturnsAsync(transaction);

        // Act
        var result = await _service.GetTransactionAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TransactionExternalId.Should().Be(transaction.Id.Value);
        result.Value.Value.Should().Be(1000m);
        result.Value.Status.Should().Be("pending");

        _mockTransactionRepository.Verify(r => r.GetByExternalIdAsync(transactionId), Times.Once);
    }

    [Fact]
    public async Task GetTransactionAsync_NonExistingTransaction_ShouldReturnFailureResult()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var request = new GetTransactionRequest
        {
            TransactionExternalId = transactionId
        };

        _mockTransactionRepository.Setup(r => r.GetByExternalIdAsync(transactionId)).ReturnsAsync((Transaction?)null);

        // Act
        var result = await _service.GetTransactionAsync(request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain($"Transaction {transactionId} not found");

        _mockTransactionRepository.Verify(r => r.GetByExternalIdAsync(transactionId), Times.Once);
    }

    [Fact]
    public async Task ProcessTransactionValidationAsync_ApproveValidation_ShouldReturnSuccessResult()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var validationEvent = new TransactionValidationEvent
        {
            TransactionExternalId = transactionId,
            Status = "approved",
            ProcessedAt = DateTime.UtcNow
        };

        var transaction = Transaction.Create(
            AccountId.From(Guid.NewGuid()),
            AccountId.From(Guid.NewGuid()),
            TransferTypeId.From(1),
            Money.From(1000m));

        _mockTransactionRepository.Setup(r => r.GetByExternalIdAsync(transactionId)).ReturnsAsync(transaction);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _service.ProcessTransactionValidationAsync(validationEvent);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        transaction.Status.Should().Be(TransactionStatus.Approved);

        _mockTransactionRepository.Verify(r => r.UpdateAsync(transaction), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ProcessTransactionValidationAsync_RejectValidation_ShouldReturnSuccessResult()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var rejectionReason = "Amount exceeds limit";
        var validationEvent = new TransactionValidationEvent
        {
            TransactionExternalId = transactionId,
            Status = "rejected",
            Reason = rejectionReason,
            ProcessedAt = DateTime.UtcNow
        };

        var transaction = Transaction.Create(
            AccountId.From(Guid.NewGuid()),
            AccountId.From(Guid.NewGuid()),
            TransferTypeId.From(1),
            Money.From(1000m));

        _mockTransactionRepository.Setup(r => r.GetByExternalIdAsync(transactionId)).ReturnsAsync(transaction);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _service.ProcessTransactionValidationAsync(validationEvent);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        transaction.Status.Should().Be(TransactionStatus.Rejected);

        _mockTransactionRepository.Verify(r => r.UpdateAsync(transaction), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ProcessTransactionValidationAsync_TransactionNotFound_ShouldReturnFailureResult()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var validationEvent = new TransactionValidationEvent
        {
            TransactionExternalId = transactionId,
            Status = "approved",
            ProcessedAt = DateTime.UtcNow
        };

        _mockTransactionRepository.Setup(r => r.GetByExternalIdAsync(transactionId)).ReturnsAsync((Transaction?)null);

        // Act
        var result = await _service.ProcessTransactionValidationAsync(validationEvent);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain($"Transaction {transactionId} not found");

        _mockTransactionRepository.Verify(r => r.UpdateAsync(It.IsAny<Transaction>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ProcessTransactionValidationAsync_RejectionWithoutReason_ShouldUseDefaultReason(string invalidReason)
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var validationEvent = new TransactionValidationEvent
        {
            TransactionExternalId = transactionId,
            Status = "rejected",
            Reason = invalidReason,
            ProcessedAt = DateTime.UtcNow
        };

        var transaction = Transaction.Create(
            AccountId.From(Guid.NewGuid()),
            AccountId.From(Guid.NewGuid()),
            TransferTypeId.From(1),
            Money.From(1000m));

        _mockTransactionRepository.Setup(r => r.GetByExternalIdAsync(transactionId)).ReturnsAsync(transaction);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _service.ProcessTransactionValidationAsync(validationEvent);

        // Assert
        result.IsSuccess.Should().BeTrue();
        transaction.Status.Should().Be(TransactionStatus.Rejected);

        _mockTransactionRepository.Verify(r => r.UpdateAsync(transaction), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public void Constructor_NullDependencies_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new TransactionService(null!, _mockAccountRepository.Object, _mockAntifraudService.Object, _mockUnitOfWork.Object))
            .ParamName.Should().Be("transactionRepository");

        Assert.Throws<ArgumentNullException>(() => 
            new TransactionService(_mockTransactionRepository.Object, null!, _mockAntifraudService.Object, _mockUnitOfWork.Object))
            .ParamName.Should().Be("accountRepository");

        Assert.Throws<ArgumentNullException>(() => 
            new TransactionService(_mockTransactionRepository.Object, _mockAccountRepository.Object, null!, _mockUnitOfWork.Object))
            .ParamName.Should().Be("antifraudService");

        Assert.Throws<ArgumentNullException>(() => 
            new TransactionService(_mockTransactionRepository.Object, _mockAccountRepository.Object, _mockAntifraudService.Object, null!))
            .ParamName.Should().Be("unitOfWork");
    }

    [Fact]
    public async Task CreateTransactionAsync_DatabaseException_ShouldReturnFailureResult()
    {
        // Arrange
        var request = new CreateTransactionRequest
        {
            SourceAccountId = Guid.NewGuid(),
            TargetAccountId = Guid.NewGuid(),
            TransferTypeId = 1,
            Value = 1000m
        };

        _mockAccountRepository.Setup(r => r.ExistsAsync(It.IsAny<AccountId>())).ReturnsAsync(true);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _service.CreateTransactionAsync(request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("An error occurred while creating the transaction");
    }

    [Fact]
    public async Task GetTransactionAsync_RepositoryException_ShouldReturnFailureResult()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var request = new GetTransactionRequest
        {
            TransactionExternalId = transactionId
        };

        _mockTransactionRepository.Setup(r => r.GetByExternalIdAsync(transactionId))
            .ThrowsAsync(new InvalidOperationException("Database connection error"));

        // Act
        var result = await _service.GetTransactionAsync(request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("An error occurred while retrieving the transaction");
    }
}