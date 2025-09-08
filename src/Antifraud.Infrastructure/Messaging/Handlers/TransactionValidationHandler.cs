using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Antifraud.Application.DTOs;
using Antifraud.Application.UseCases;

namespace Antifraud.Infrastructure.Messaging.Handlers;

public class TransactionValidationHandler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TransactionValidationHandler> _logger;

    public TransactionValidationHandler(
        IServiceProvider serviceProvider,
        ILogger<TransactionValidationHandler> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task HandleAsync(string message)
    {
        try
        {
            var validationEvent = JsonSerializer.Deserialize<TransactionValidationEvent>(message, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (validationEvent == null)
            {
                _logger.LogWarning("Received null validation event");
                return;
            }

            using var scope = _serviceProvider.CreateScope();
            var useCase = scope.ServiceProvider.GetRequiredService<ProcessTransactionValidationUseCase>();

            var result = await useCase.ExecuteAsync(validationEvent);

            if (result.IsFailure)
            {
                _logger.LogError("Failed to process transaction validation: {Error}", result.Error);
            }
            else
            {
                _logger.LogInformation("Successfully processed validation for transaction {TransactionId}", 
                    validationEvent.TransactionExternalId);
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize validation event: {Message}", message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error handling validation event: {Message}", message);
        }
    }
}