using Xunit;
using FluentAssertions;
using Antifraud.Application.DTOs;
using Antifraud.Application.Validators;

namespace Antifraud.Application.Tests.Validators;

public class GetTransactionRequestValidatorTests
{
    [Fact]
    public void IsValid_ValidRequest_ShouldReturnTrueWithNoErrors()
    {
        // Arrange
        var request = new GetTransactionRequest
        {
            TransactionExternalId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var isValid = GetTransactionRequestValidator.IsValid(request, out var errors);

        // Assert
        isValid.Should().BeTrue();
        errors.Should().BeEmpty();
    }

    [Fact]
    public void IsValid_ValidRequestWithoutCreatedAt_ShouldReturnTrueWithNoErrors()
    {
        // Arrange
        var request = new GetTransactionRequest
        {
            TransactionExternalId = Guid.NewGuid(),
            CreatedAt = null
        };

        // Act
        var isValid = GetTransactionRequestValidator.IsValid(request, out var errors);

        // Assert
        isValid.Should().BeTrue();
        errors.Should().BeEmpty();
    }

    [Fact]
    public void IsValid_EmptyTransactionExternalId_ShouldReturnFalseWithError()
    {
        // Arrange
        var request = new GetTransactionRequest
        {
            TransactionExternalId = Guid.Empty,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var isValid = GetTransactionRequestValidator.IsValid(request, out var errors);

        // Assert
        isValid.Should().BeFalse();
        errors.Should().Contain("Transaction external ID cannot be empty");
    }

    [Fact]
    public void Validate_ValidRequest_ShouldReturnEmptyValidationResults()
    {
        // Arrange
        var request = new GetTransactionRequest
        {
            TransactionExternalId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var validationResults = GetTransactionRequestValidator.Validate(request);

        // Assert
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void Validate_InvalidRequest_ShouldReturnValidationResults()
    {
        // Arrange
        var request = new GetTransactionRequest
        {
            TransactionExternalId = Guid.Empty,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var validationResults = GetTransactionRequestValidator.Validate(request);

        // Assert
        validationResults.Should().NotBeEmpty();
        validationResults.Should().HaveCount(1);
    }

    [Fact]
    public void IsValid_FutureCreatedAt_ShouldStillBeValid()
    {
        // Arrange
        var request = new GetTransactionRequest
        {
            TransactionExternalId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow.AddDays(1) // Future date
        };

        // Act
        var isValid = GetTransactionRequestValidator.IsValid(request, out var errors);

        // Assert
        isValid.Should().BeTrue(); // No validation for future dates in this validator
        errors.Should().BeEmpty();
    }

    [Fact]
    public void IsValid_PastCreatedAt_ShouldBeValid()
    {
        // Arrange
        var request = new GetTransactionRequest
        {
            TransactionExternalId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow.AddDays(-30) // Past date
        };

        // Act
        var isValid = GetTransactionRequestValidator.IsValid(request, out var errors);

        // Assert
        isValid.Should().BeTrue();
        errors.Should().BeEmpty();
    }
}