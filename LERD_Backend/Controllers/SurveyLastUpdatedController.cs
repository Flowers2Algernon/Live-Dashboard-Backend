// LERD_Backend/Controllers/SurveyLastUpdatedController.cs
using LERD.Application.Interfaces;
using LERD.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace LERD_Backend.Controllers;

/// <summary>
/// Survey data freshness API
/// Simple endpoint to show users when dashboard data was last updated
/// 
/// Design principles (Linus-approved):
/// - One job: tell user how fresh the data is
/// - Fast: Single query to extraction_log
/// - Reliable: No complex fallback logic
/// </summary>
[ApiController]
[Route("api/surveys")]
public class SurveyLastUpdatedController : ControllerBase
{
    private readonly ISurveyLastUpdatedService _lastUpdatedService;
    private readonly ILogger<SurveyLastUpdatedController> _logger;

    public SurveyLastUpdatedController(
        ISurveyLastUpdatedService lastUpdatedService,
        ILogger<SurveyLastUpdatedController> logger)
    {
        _lastUpdatedService = lastUpdatedService;
        _logger = logger;
    }

    /// <summary>
    /// Get last update time for a specific survey
    /// Shows when the survey data was last refreshed from source
    /// </summary>
    /// <param name="surveyId">Survey GUID</param>
    /// <returns>Last updated timestamp and metadata</returns>
    [HttpGet("{surveyId}/last-updated")]
    public async Task<ActionResult<SurveyLastUpdatedResponse>> GetLastUpdated(Guid surveyId)
    {
        try
        {
            if (surveyId == Guid.Empty)
            {
                return BadRequest(new SurveyLastUpdatedResponse
                {
                    Success = false,
                    Message = "Invalid survey ID",
                    Data = null
                });
            }

            var result = await _lastUpdatedService.GetLastUpdatedAsync(surveyId);

            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetLastUpdated for survey {SurveyId}", surveyId);
            
            return StatusCode(500, new SurveyLastUpdatedResponse
            {
                Success = false,
                Message = "Internal server error",
                Data = null
            });
        }
    }

    /// <summary>
    /// Get last update times for multiple surveys (batch operation)
    /// Efficient for dashboard initialization when user has access to multiple surveys
    /// </summary>
    /// <param name="surveyIds">Comma-separated list of survey GUIDs</param>
    /// <returns>Dictionary of survey IDs to last updated information</returns>
    [HttpGet("batch/last-updated")]
    public async Task<ActionResult<object>> GetLastUpdatedBatch([FromQuery] string surveyIds)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(surveyIds))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Survey IDs parameter is required",
                    data = (object?)null
                });
            }

            // Parse comma-separated GUIDs
            var guidList = new List<Guid>();
            var idStrings = surveyIds.Split(',', StringSplitOptions.RemoveEmptyEntries);

            foreach (var idString in idStrings)
            {
                if (Guid.TryParse(idString.Trim(), out var guid))
                {
                    guidList.Add(guid);
                }
            }

            if (!guidList.Any())
            {
                return BadRequest(new
                {
                    success = false,
                    message = "No valid survey IDs provided",
                    data = (object?)null
                });
            }

            var results = await _lastUpdatedService.GetLastUpdatedBatchAsync(guidList);

            return Ok(new
            {
                success = true,
                message = $"Retrieved last updated times for {results.Count} surveys",
                data = results
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetLastUpdatedBatch for surveyIds: {SurveyIds}", surveyIds);
            
            return StatusCode(500, new
            {
                success = false,
                message = "Internal server error",
                data = (object?)null
            });
        }
    }
}
