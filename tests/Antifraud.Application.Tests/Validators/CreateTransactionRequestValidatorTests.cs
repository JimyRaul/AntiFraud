using Xunit;
using FluentAssertions;
using Antifraud.Application.DTOs;
using Antifraud.Application.Validators;

namespace Antifraud.Application.Tests.Validators;

public class CreateTransactionRequestValidatorTests
{
    [Fact]
    public void IsValid_ValidRequest_ShouldReturnTrueWithNoErrors()
    {
        // Arrange
        var request = new CreateTransactionRequest
        {
            SourceAccountId = Guid.NewGuid(),
            TargetAccountId = Guid.NewGuid(),
            TransferTypeId = 1,
            Value = 1000m
        };

        // Act
        var isValid = CreateTransactionRequestValidator.IsValid(request, out var errors);

        // Assert
        isValid.Should().BeTrue();
        errors.Should().BeEmpty();
    }

    [Fact]
    public void IsValid_SameSourceAndTargetAccount_ShouldReturnFalseWithError()
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
        var isValid = CreateTransactionRequestValidator.IsValid(request, out var errors);

        // Assert
        isValid.Should().BeFalse();
        errors.Should().Contain("Source and target accounts cannot be the same");
    }

    [Fact]
    public void IsValid_EmptySourceAccountId_ShouldReturnFalseWithError()
    {
        // Arrange
        var request = new CreateTransactionRequest
        {
            SourceAccountId = Guid.Empty,
            TargetAccountId = Guid.NewGuid(),
            TransferTypeId = 1,
            Value = 1000m
        };

        // Act
        var isValid = CreateTransactionRequestValidator.IsValid(request, out var errors);

        // Assert
        isValid.Should().BeFalse();
        errors.Should().Contain("Source account ID cannot be empty");
    }

    [Fact]
    public void IsValid_EmptyTargetAccountId_ShouldReturnFalseWithError()
    {
        // Arrange
        var request = new CreateTransactionRequest
        {
            SourceAccountId = Guid.NewGuid(),
            TargetAccountId = Guid.Empty,
            TransferTypeId = 1,
            Value = 1000m
        };

        // Act
        var isValid = CreateTransactionRequestValidator.IsValid(request, out var errors);

        // Assert
        isValid.Should().BeFalse();
        errors.Should().Contain("Target account ID cannot be empty");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void IsValid_InvalidTransferTypeId_ShouldReturnFalseWithError(int invalidTransferTypeId)
    {
        // Arrange
        var request = new CreateTransactionRequest
        {
            SourceAccountId = Guid.NewGuid(),
            TargetAccountId = Guid.NewGuid(),
            TransferTypeId = invalidTransferTypeId,
            Value = 1000m
        };

        // Act
        var isValid = CreateTransactionRequestValidator.IsValid(request, out var errors);

        // Assert
        isValid.Should().BeFalse();
        errors.Should().Contain("Transfer type ID must be greater than 0");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    [InlineData(-100)]
    public void IsValid_InvalidValue_ShouldReturnFalseWithError(decimal invalidValue)
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
        var isValid = CreateTransactionRequestValidator.IsValid(request, out var errors);

        // Assert
        isValid.Should().BeFalse();
        errors.Should().Contain("Value must be greater than 0");
    }

    [Fact]
    public void IsValid_MultipleValidationErrors_ShouldReturnAllErrors()
    {
        // Arrange
        var sameAccountId = Guid.Empty;
        var request = new CreateTransactionRequest
        {
            SourceAccountId = sameAccountId,
            TargetAccountId = sameAccountId,
            TransferTypeId = 0,
            Value = -100m
        };

        // Act
        var isValid = CreateTransactionRequestValidator.IsValid(request, out var errors);

        // Assert
        isValid.Should().BeFalse();
        errors.Should().HaveCountGreaterThan(1);
        errors.Should().Contain("Source account ID cannot be empty");
        errors.Should().Contain("Target account ID cannot be empty");
        errors.Should().Contain("Transfer type ID must be greater than 0");
        errors.Should().Contain("Value must be greater than 0");
    }

    [Fact]
    public void Validate_ValidRequest_ShouldReturnEmptyValidationResults()
    {
        // Arrange
        var request = new CreateTransactionRequest
        {
            SourceAccountId = Guid.NewGuid(),
            TargetAccountId = Guid.NewGuid(),
            TransferTypeId = 1,
            Value = 1000m
        };

        // Act
        var validationResults = CreateTransactionRequestValidator.Validate(request);

        // Assert
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void Validate_InvalidRequest_ShouldReturnValidationResults()
    {
        // Arrange
        var request = new CreateTransactionRequest
        {
            SourceAccountId = Guid.Empty,
            TargetAccountId = Guid.NewGuid(),
            TransferTypeId = 1,
            Value = 1000m
        };

        // Act
        var validationResults = CreateTransactionRequestValidator.Validate(request);

        // Assert
        validationResults.Should().NotBeEmpty();
        validationResults.Should().HaveCountGreaterThan(0);
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(1)]
    [InlineData(1000.50)]
    [InlineData(999999.99)]
    public void IsValid_ValidPositiveValues_ShouldReturnTrue(decimal validValue)
    {
        // Arrange
        var request = new CreateTransactionRequest
        {
            SourceAccountId = Guid.NewGuid(),
            TargetAccountId = Guid.NewGuid(),
            TransferTypeId = 1,
            Value = validValue
        };

        // Act
        var isValid = CreateTransactionRequestValidator.IsValid(request, out var errors);

        // Assert
        isValid.Should().BeTrue();
        errors.Should().BeEmpty();
    }
}