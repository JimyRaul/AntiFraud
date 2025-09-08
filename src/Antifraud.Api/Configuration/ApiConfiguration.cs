namespace Antifraud.Api.Configuration;

public class ApiConfiguration
{
    public const string SectionName = "Api";

    public string Title { get; set; } = "Antifraud API";
    public string Version { get; set; } = "v1";
    public string Description { get; set; } = "Anti-fraud transaction validation microservice";
    public bool EnableSwagger { get; set; } = true;
    public bool EnableDetailedErrors { get; set; } = false;
    public int RequestTimeoutSeconds { get; set; } = 30;
    public bool EnableRequestLogging { get; set; } = true;
    public bool EnableCorrelationId { get; set; } = true;
}