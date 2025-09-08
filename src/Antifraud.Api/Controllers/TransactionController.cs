using Microsoft.AspNetCore.Mvc;
using Antifraud.Application.DTOs;
using Antifraud.Application.UseCases;
using Antifraud.Application.Validators;

namespace Antifraud.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TransactionController : ControllerBase
{
    private readonly CreateTransactionUseCase _createTransactionUseCase;
    private readonly GetTransactionUseCase _getTransactionUseCase;
    private readonly ILogger<TransactionController> _logger;

    public TransactionController(
        CreateTransactionUseCase createTransactionUseCase,
        GetTransactionUseCase getTransactionUseCase,
        ILogger<TransactionController> logger)
    {
        _createTransactionUseCase = createTransactionUseCase ?? throw new ArgumentNullException(nameof(createTransactionUseCase));
        _getTransactionUseCase = getTransactionUseCase ?? throw new ArgumentNullException(nameof(getTransactionUseCase));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new financial transaction
    /// </summary>
    /// <param name="request">Transaction creation request</param>
    /// <returns>Created transaction details</returns>
    /// <response code="201">Transaction created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Account not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest request)
    {
        _logger.LogInformation("Creating transaction from account {SourceAccountId} to {TargetAccountId} with value {Value}",
            request.SourceAccountId, request.TargetAccountId, request.Value);

        // Validar el request
        if (!CreateTransactionRequestValidator.IsValid(request, out var validationErrors))
        {
            var problemDetails = new ValidationProblemDetails
            {
                Title = "Validation failed",
                Status = StatusCodes.Status400BadRequest,
                Detail = "One or more validation errors occurred."
            };

            foreach (var error in validationErrors)
            {
                problemDetails.Errors.Add("ValidationError", new[] { error });
            }

            return BadRequest(problemDetails);
        }

        try
        {
            var result = await _createTransactionUseCase.ExecuteAsync(request);

            if (result.IsFailure)
            {
                _logger.LogWarning("Failed to create transaction: {Error}", result.Error);

                if (result.Error.Contains("does not exist"))
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Account not found",
                        Status = StatusCodes.Status404NotFound,
                        Detail = result.Error
                    });
                }

                return BadRequest(new ProblemDetails
                {
                    Title = "Transaction creation failed",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = result.Error
                });
            }

            _logger.LogInformation("Transaction created successfully with ID {TransactionId}", result.Value!.TransactionExternalId);

            return CreatedAtAction(
                nameof(GetTransaction),
                new { transactionExternalId = result.Value.TransactionExternalId },
                result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating transaction");

            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal server error",
                Status = StatusCodes.Status500InternalServerError,
                Detail = "An unexpected error occurred while processing the request."
            });
        }
    }

    /// <summary>
    /// Retrieves a transaction by its external ID
    /// </summary>
    /// <param name="transactionExternalId">The external ID of the transaction</param>
    /// <param name="createdAt">Optional creation date filter</param>
    /// <returns>Transaction details</returns>
    /// <response code="200">Transaction found</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="404">Transaction not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{transactionExternalId:guid}")]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTransaction(
        [FromRoute] Guid transactionExternalId,
        [FromQuery] DateTime? createdAt = null)
    {
        _logger.LogInformation("Retrieving transaction {TransactionId}", transactionExternalId);

        var request = new GetTransactionRequest
        {
            TransactionExternalId = transactionExternalId,
            CreatedAt = createdAt
        };

        // Validar el request
        if (!GetTransactionRequestValidator.IsValid(request, out var validationErrors))
        {
            var problemDetails = new ValidationProblemDetails
            {
                Title = "Validation failed",
                Status = StatusCodes.Status400BadRequest,
                Detail = "One or more validation errors occurred."
            };

            foreach (var error in validationErrors)
            {
                problemDetails.Errors.Add("ValidationError", new[] { error });
            }

            return BadRequest(problemDetails);
        }

        try
        {
            var result = await _getTransactionUseCase.ExecuteAsync(request);

            if (result.IsFailure)
            {
                _logger.LogWarning("Transaction {TransactionId} not found: {Error}", transactionExternalId, result.Error);

                return NotFound(new ProblemDetails
                {
                    Title = "Transaction not found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = result.Error
                });
            }

            _logger.LogInformation("Transaction {TransactionId} retrieved successfully", transactionExternalId);

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving transaction {TransactionId}", transactionExternalId);

            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal server error",
                Status = StatusCodes.Status500InternalServerError,
                Detail = "An unexpected error occurred while processing the request."
            });
        }
    }

    /// <summary>
    /// Health check endpoint for the transaction service
    /// </summary>
    /// <returns>Service health status</returns>
    [HttpGet("health")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult HealthCheck()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}