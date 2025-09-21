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
    private readonly ICustomerSatisfactionService _customerSatisfactionService;
    private readonly ICustomerSatisfactionTrendService _customerSatisfactionTrendService;
    private readonly INPSService _npsService;
    private readonly IServiceAttributeService _serviceAttributeService;
    private readonly ILogger<ChartsController> _logger;

    public ChartsController(
        IResponseChartService responseChartService,
        ICustomerSatisfactionService customerSatisfactionService,
        ICustomerSatisfactionTrendService customerSatisfactionTrendService,
        INPSService npsService,
        IServiceAttributeService serviceAttributeService,
        ILogger<ChartsController> logger)
    {
        _responseChartService = responseChartService;
        _customerSatisfactionService = customerSatisfactionService;
        _customerSatisfactionTrendService = customerSatisfactionTrendService;
        _npsService = npsService;
        _serviceAttributeService = serviceAttributeService;
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

    [HttpGet("customer-satisfaction")]
    public async Task<ActionResult<ApiResponse<CustomerSatisfactionData>>> GetCustomerSatisfaction(
        [FromQuery] Guid surveyId,
        [FromQuery] string? gender = null,
        [FromQuery] string? participantType = null,
        [FromQuery] string? period = null)
    {
        try
        {
            if (surveyId == Guid.Empty)
            {
                return BadRequest(new ApiResponse<CustomerSatisfactionData>
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

            var data = await _customerSatisfactionService.GetSatisfactionDataAsync(surveyId, filters);

            return Ok(new ApiResponse<CustomerSatisfactionData>
            {
                Success = true,
                Data = data
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer satisfaction data for survey {SurveyId}", surveyId);
            
            return StatusCode(500, new ApiResponse<CustomerSatisfactionData>
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpGet("customer-satisfaction-trend")]
    public async Task<ActionResult<ApiResponse<CustomerSatisfactionTrendData>>> GetCustomerSatisfactionTrend(
        [FromQuery] Guid surveyId,
        [FromQuery] string? gender = null,
        [FromQuery] string? participantType = null,
        [FromQuery] string? period = null)
    {
        try
        {
            if (surveyId == Guid.Empty)
            {
                return BadRequest(new ApiResponse<CustomerSatisfactionTrendData>
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

            var data = await _customerSatisfactionTrendService.GetTrendDataAsync(surveyId, filters);

            return Ok(new ApiResponse<CustomerSatisfactionTrendData>
            {
                Success = true,
                Data = data
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer satisfaction trend data for survey {SurveyId}", surveyId);
            
            return StatusCode(500, new ApiResponse<CustomerSatisfactionTrendData>
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpGet("nps")]
    public async Task<ActionResult<ApiResponse<NPSData>>> GetNPS(
        [FromQuery] Guid surveyId,
        [FromQuery] string? gender = null,
        [FromQuery] string? participantType = null,
        [FromQuery] string? period = null)
    {
        try
        {
            if (surveyId == Guid.Empty)
            {
                return BadRequest(new ApiResponse<NPSData>
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

            var data = await _npsService.GetNPSDataAsync(surveyId, filters);

            return Ok(new ApiResponse<NPSData>
            {
                Success = true,
                Data = data
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting NPS data for survey {SurveyId}", surveyId);
            
            return StatusCode(500, new ApiResponse<NPSData>
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpGet("service-attributes")]
    public async Task<ActionResult<ApiResponse<ServiceAttributeData>>> GetServiceAttributes(
        [FromQuery] Guid surveyId,
        [FromQuery] string? gender = null,
        [FromQuery] string? participantType = null,
        [FromQuery] string? period = null,
        [FromQuery] string[]? selectedAttributes = null) // 图表级别过滤
    {
        try
        {
            if (surveyId == Guid.Empty)
            {
                _logger.LogWarning("GetServiceAttributes called with empty survey ID");
                return BadRequest(new ApiResponse<ServiceAttributeData>
                {
                    Success = false,
                    Message = "Valid survey_id is required"
                });
            }

            var filters = new ServiceAttributeFilters
            {
                Gender = gender,
                ParticipantType = participantType,
                Period = period,
                SelectedAttributes = selectedAttributes?.ToList()
            };

            _logger.LogInformation("Getting service attributes for survey {SurveyId} with filters: Gender={Gender}, ParticipantType={ParticipantType}, SelectedAttributes={SelectedAttributes}", 
                surveyId, filters.Gender, filters.ParticipantType, filters.SelectedAttributes != null ? string.Join(",", filters.SelectedAttributes) : "None");

            var data = await _serviceAttributeService.GetServiceAttributeDataAsync(surveyId, filters);

            return Ok(new ApiResponse<ServiceAttributeData>
            {
                Success = true,
                Message = "Service attribute data retrieved successfully",
                Data = data
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service attribute data for survey {SurveyId}", surveyId);

            return StatusCode(500, new ApiResponse<ServiceAttributeData>
            {
                Success = false,
                Message = "An error occurred while getting service attribute data"
            });
        }
    }
}