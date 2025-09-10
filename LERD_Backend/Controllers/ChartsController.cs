// LERD_Backend/Controllers/ChartsController.cs
using LERD.Application.Interfaces;
using LERD.Domain.Models;
using LERD.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace LERD_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChartsController : ControllerBase
{
    private readonly IResponseChartService _responseChartService;
    private readonly ILogger<ChartsController> _logger;

    public ChartsController(
        IResponseChartService responseChartService,
        ILogger<ChartsController> logger)
    {
        _responseChartService = responseChartService;
        _logger = logger;
    }

    
    [HttpGet("response")]
    public async Task<ActionResult<ApiResponse<ResponseChartData>>> GetResponseChart(
        [FromQuery] Guid surveyId,
        [FromQuery] string? gender = null,
        [FromQuery] string? participantType = null,
        [FromQuery] string? period = null)
    {
        try
        {
            if (surveyId == Guid.Empty)
            {
                return BadRequest(new ApiResponse<ResponseChartData>
                {
                    Success = false,
                    Message = "Valid survey_id is required"
                });
            }

            var filters = new ChartFilters
            {
                Gender = gender,
                ParticipantType = participantType,
                Period = period
            };

            var data = await _responseChartService.GetResponseChartDataAsync(surveyId, filters);

            return Ok(new ApiResponse<ResponseChartData>
            {
                Success = true,
                Data = data
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting response chart data for survey {SurveyId}", surveyId);
            
            return StatusCode(500, new ApiResponse<ResponseChartData>
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }
}