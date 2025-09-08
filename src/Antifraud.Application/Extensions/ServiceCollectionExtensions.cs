using Microsoft.Extensions.DependencyInjection;
using Antifraud.Application.Interfaces;
using Antifraud.Application.Services;
using Antifraud.Application.UseCases;

namespace Antifraud.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        // Servicios de aplicación
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IAntifraudService, AntifraudService>();

        // Casos de uso
        services.AddScoped<CreateTransactionUseCase>();
        services.AddScoped<GetTransactionUseCase>();
        services.AddScoped<ProcessTransactionValidationUseCase>();

        return services;
    }
}