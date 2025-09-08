using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Antifraud.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class HealthController : ControllerBase
{
    private readonly HealthCheckService _healthCheckService;
    private readonly ILogger<HealthController> _logger;

    public HealthController(
        HealthCheckService healthCheckService,
        ILogger<HealthController> logger)
    {
        _healthCheckService = healthCheckService ?? throw new ArgumentNullException(nameof(healthCheckService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Comprehensive health check for all dependencies
    /// </summary>
    /// <returns>Health status of all components</returns>
    /// <response code="200">All services are healthy</response>
    /// <response code="503">One or more services are unhealthy</response>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetHealth()
    {
        try
        {
            var healthReport = await _healthCheckService.CheckHealthAsync();

            var response = new
            {
                status = healthReport.Status.ToString(),
                totalDuration = healthReport.TotalDuration.TotalMilliseconds,
                entries = healthReport.Entries.Select(entry => new
                {
                    name = entry.Key,
                    status = entry.Value.Status.ToString(),
                    duration = entry.Value.Duration.TotalMilliseconds,
                    description = entry.Value.Description,
                    data = entry.Value.Data
                })
            };

            return healthReport.Status == HealthStatus.Healthy
                ? Ok(response)
                : StatusCode(StatusCodes.Status503ServiceUnavailable, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing health check");

            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                status = "Unhealthy",
                error = "Health check failed"
            });
        }
    }

    /// <summary>
    /// Quick readiness check
    /// </summary>
    /// <returns>Simple ready status</returns>
    [HttpGet("ready")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult Ready()
    {
        return Ok(new { status = "ready", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Quick liveness check
    /// </summary>
    /// <returns>Simple alive status</returns>
    [HttpGet("live")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult Live()
    {
        return Ok(new { status = "alive", timestamp = DateTime.UtcNow });
    }
}