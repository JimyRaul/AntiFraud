using Xunit;
using FluentAssertions;
using Antifraud.Application.DTOs;

namespace Antifraud.Application.Tests.DTOs;

public class ResultTests
{
    [Fact]
    public void Success_WithValue_ShouldCreateSuccessResult()
    {
        // Arrange
        var value = "test value";

        // Act
        var result = Result<string>.Success(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(value);
        result.Error.Should().BeEmpty();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Failure_WithSingleError_ShouldCreateFailureResult()
    {
        // Arrange
        var error = "Something went wrong";

        // Act
        var result = Result<string>.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.Error.Should().Be(error);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Failure_WithMultipleErrors_ShouldCreateFailureResult()
    {
        // Arrange
        var errors = new[] { "Error 1", "Error 2", "Error 3" };

        // Act
        var result = Result<string>.Failure(errors);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.Error.Should().Be("Error 1; Error 2; Error 3");
        result.Errors.Should().BeEquivalentTo(errors);
    }

    [Fact]
    public void Match_SuccessResult_ShouldCallOnSuccessFunction()
    {
        // Arrange
        var value = 42;
        var result = Result<int>.Success(value);

        // Act
        var matchResult = result.Match(
            onSuccess: v => $"Success: {v}",
            onFailure: e => $"Failure: {e}");

        // Assert
        matchResult.Should().Be("Success: 42");
    }

    [Fact]
    public void Match_FailureResult_ShouldCallOnFailureFunction()
    {
        // Arrange
        var error = "Something went wrong";
        var result = Result<int>.Failure(error);

        // Act
        var matchResult = result.Match(
            onSuccess: v => $"Success: {v}",
            onFailure: e => $"Failure: {e}");

        // Assert
        matchResult.Should().Be("Failure: Something went wrong");
    }

    [Fact]
    public void StaticSuccess_ShouldCreateSuccessResult()
    {
        // Arrange
        var value = "test";

        // Act
        var result = Result.Success(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [Fact]
    public void StaticFailure_WithSingleError_ShouldCreateFailureResult()
    {
        // Arrange
        var error = "Error message";

        // Act
        var result = Result.Failure<string>(error);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void StaticFailure_WithMultipleErrors_ShouldCreateFailureResult()
    {
        // Arrange
        var errors = new[] { "Error 1", "Error 2" };

        // Act
        var result = Result.Failure<string>(errors);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().BeEquivalentTo(errors);
    }

    [Fact]
    public void Failure_WithEmptyErrorList_ShouldCreateFailureWithEmptyError()
    {
        // Arrange
        var errors = Array.Empty<string>();

        // Act
        var result = Result<string>.Failure(errors);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeEmpty();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Success_WithNullValue_ShouldStillCreateSuccessResult()
    {
        // Act
        var result = Result<string?>.Success(null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Failure_WithNullOrEmptyError_ShouldCreateFailureResult(string error)
    {
        // Act
        var result = Result<string>.Failure(error);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error ?? string.Empty);
    }
}
