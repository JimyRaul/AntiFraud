using Xunit;
using FluentAssertions;
using Antifraud.Domain.ValueObjects;

namespace Antifraud.Domain.Tests.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void From_ValidAmount_ShouldCreateMoney()
    {
        // Arrange
        var amount = 100.50m;

        // Act
        var money = Money.From(amount);

        // Assert
        money.Amount.Should().Be(amount);
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void From_ValidAmountAndCurrency_ShouldCreateMoney()
    {
        // Arrange
        var amount = 100.50m;
        var currency = "EUR";

        // Act
        var money = Money.From(amount, currency);

        // Assert
        money.Amount.Should().Be(amount);
        money.Currency.Should().Be("EUR");
    }

    [Fact]
    public void From_NegativeAmount_ShouldThrowArgumentException()
    {
        // Arrange
        var amount = -100m;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Money.From(amount));
        exception.Message.Should().Contain("Amount cannot be negative");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void From_InvalidCurrency_ShouldThrowArgumentException(string invalidCurrency)
    {
        // Arrange
        var amount = 100m;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Money.From(amount, invalidCurrency));
        exception.Message.Should().Contain("Currency cannot be null or empty");
    }

    [Fact]
    public void Add_SameCurrency_ShouldReturnSum()
    {
        // Arrange
        var money1 = Money.From(100m, "USD");
        var money2 = Money.From(50m, "USD");

        // Act
        var result = money1.Add(money2);

        // Assert
        result.Amount.Should().Be(150m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Add_DifferentCurrency_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var money1 = Money.From(100m, "USD");
        var money2 = Money.From(50m, "EUR");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => money1.Add(money2));
        exception.Message.Should().Contain("Cannot add different currencies");
    }

    [Fact]
    public void IsGreaterThan_LargerAmount_ShouldReturnTrue()
    {
        // Arrange
        var money1 = Money.From(100m);
        var money2 = Money.From(50m);

        // Act
        var result = money1.IsGreaterThan(money2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsGreaterThan_SmallerAmount_ShouldReturnFalse()
    {
        // Arrange
        var money1 = Money.From(50m);
        var money2 = Money.From(100m);

        // Act
        var result = money1.IsGreaterThan(money2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsGreaterThan_DifferentCurrency_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var money1 = Money.From(100m, "USD");
        var money2 = Money.From(50m, "EUR");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => money1.IsGreaterThan(money2));
        exception.Message.Should().Contain("Cannot compare different currencies");
    }

    [Fact]
    public void Zero_ShouldReturnZeroAmount()
    {
        // Act
        var zero = Money.Zero;

        // Assert
        zero.Amount.Should().Be(0m);
        zero.Currency.Should().Be("USD");
    }

    [Fact]
    public void ImplicitConversion_FromDecimal_ShouldWork()
    {
        // Arrange
        decimal amount = 100m;

        // Act
        Money money = amount;

        // Assert
        money.Amount.Should().Be(amount);
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void ImplicitConversion_ToDecimal_ShouldWork()
    {
        // Arrange
        var money = Money.From(100m);

        // Act
        decimal amount = money;

        // Assert
        amount.Should().Be(100m);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var money = Money.From(100.50m, "USD");

        // Act
        var result = money.ToString();

        // Assert
        result.Should().Contain("100");
        result.Should().Contain("USD");
    }

    [Fact]
    public void Equality_SameAmountAndCurrency_ShouldBeEqual()
    {
        // Arrange
        var money1 = Money.From(100m, "USD");
        var money2 = Money.From(100m, "USD");

        // Act & Assert
        money1.Should().Be(money2);
        (money1 == money2).Should().BeTrue();
    }

    [Fact]
    public void Equality_DifferentAmount_ShouldNotBeEqual()
    {
        // Arrange
        var money1 = Money.From(100m, "USD");
        var money2 = Money.From(200m, "USD");

        // Act & Assert
        money1.Should().NotBe(money2);
        (money1 != money2).Should().BeTrue();
    }

    [Fact]
    public void Equality_DifferentCurrency_ShouldNotBeEqual()
    {
        // Arrange
        var money1 = Money.From(100m, "USD");
        var money2 = Money.From(100m, "EUR");

        // Act & Assert
        money1.Should().NotBe(money2);
        (money1 != money2).Should().BeTrue();
    }
}