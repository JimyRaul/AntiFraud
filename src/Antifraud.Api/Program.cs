using System.Reflection;
using Antifraud.Api.Configuration;
using Antifraud.Api.Middleware;
using Antifraud.Application.Extensions;
using Antifraud.Infrastructure.DependencyInjection;
using Antifraud.Infrastructure.Scripts;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // Customize model validation responses
        options.SuppressModelStateInvalidFilter = false;
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Antifraud API",
        Version = "v1",
        Description = "Anti-fraud transaction validation microservice",
        Contact = new OpenApiContact
        {
            Name = "Development Team",
            Email = "dev@antifraud.com"
        }
    });

    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Add security definition for future authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Application layers
builder.Services.AddApplicationLayer();
builder.Services.AddInfrastructureLayer(builder.Configuration);
builder.Services.AddDatabaseMigrations();

// Logging
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
});

// API Configuration
builder.Services.Configure<ApiConfiguration>(
    builder.Configuration.GetSection(ApiConfiguration.SectionName));

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Antifraud API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
}

// Middleware pipeline
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

// Health checks
app.MapHealthChecks("/health");

// Initialize database
if (args.Contains("--migrate"))
{
    await DatabaseInitializer.InitializeAsync(app.Services);
    return;
}

// Start the application
app.Logger.LogInformation("Starting Antifraud API...");

try
{
    // Auto-migrate in development
    if (app.Environment.IsDevelopment())
    {
        await DatabaseInitializer.InitializeAsync(app.Services);
    }

    await app.RunAsync();
}
catch (Exception ex)
{
    app.Logger.LogCritical(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    app.Logger.LogInformation("Antifraud API stopped");
}