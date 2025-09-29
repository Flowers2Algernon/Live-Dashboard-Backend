using LERD.Application.Interfaces;
using LERD.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace LERD_Backend.Controllers;

[ApiController]
[Route("api")]
public class UserFiltersController : ControllerBase
{
    private readonly IUserFilterPreferencesService _filterService;
    private readonly ILogger<UserFiltersController> _logger;

    public UserFiltersController(
        IUserFilterPreferencesService filterService,
        ILogger<UserFiltersController> logger)
    {
        _filterService = filterService;
        _logger = logger;
    }

    #region 读取APIs - 获取选项列表

    /// <summary>
    /// 获取用户可访问的所有services
    /// GET /api/users/{userId}/services
    /// </summary>
    [HttpGet("users/{userId}/services")]
    public async Task<ActionResult<ServicesResponse>> GetAvailableServices(Guid userId)
    {
        try
        {
            _logger.LogInformation("Getting available services for user {UserId}", userId);

            var services = await _filterService.GetAvailableServicesAsync(userId);

            return Ok(new ServicesResponse
            {
                Success = true,
                Message = $"Found {services.Count} services",
                Data = services
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available services for user {UserId}", userId);
            return StatusCode(500, new ServicesResponse
            {
                Success = false,
                Message = $"Error retrieving services: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// 获取指定survey的所有regions
    /// GET /api/surveys/{surveyId}/regions
    /// </summary>
    [HttpGet("surveys/{surveyId}/regions")]
    public async Task<ActionResult<RegionsResponse>> GetAvailableRegions(Guid surveyId)
    {
        try
        {
            _logger.LogInformation("Getting available regions for survey {SurveyId}", surveyId);

            var regions = await _filterService.GetAvailableRegionsAsync(surveyId);

            return Ok(new RegionsResponse
            {
                Success = true,
                Message = $"Found {regions.Count} regions",
                Data = regions
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available regions for survey {SurveyId}", surveyId);
            return StatusCode(500, new RegionsResponse
            {
                Success = false,
                Message = $"Error retrieving regions: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// 获取过滤器选项
    /// GET /api/surveys/{surveyId}/filter-options
    /// </summary>
    [HttpGet("surveys/{surveyId}/filter-options")]
    public async Task<ActionResult<object>> GetFilterOptions(Guid surveyId)
    {
        try
        {
            _logger.LogInformation("Getting filter options for survey {SurveyId}", surveyId);

            var options = await _filterService.GetFilterOptionsAsync(surveyId);

            return Ok(new
            {
                success = true,
                message = "Filter options retrieved successfully",
                data = options
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting filter options for survey {SurveyId}", surveyId);
            return StatusCode(500, new
            {
                success = false,
                message = $"Error retrieving filter options: {ex.Message}"
            });
        }
    }

    #endregion

    #region 读取APIs - 获取当前选择

    /// <summary>
    /// 获取用户的过滤器配置
    /// GET /api/users/{userId}/filters?surveyId={surveyId}
    /// </summary>
    [HttpGet("users/{userId}/filters")]
    public async Task<ActionResult<UserFilterResponse>> GetUserFilters(
        Guid userId,
        [FromQuery] Guid surveyId)
    {
        try
        {
            _logger.LogInformation("Getting user filters for user {UserId}, survey {SurveyId}", userId, surveyId);

            var filters = await _filterService.GetUserFiltersAsync(userId, surveyId);

            return Ok(new UserFilterResponse
            {
                Success = true,
                Message = "User filters retrieved successfully",
                Data = filters
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user filters for user {UserId}, survey {SurveyId}", userId, surveyId);
            return StatusCode(500, new UserFilterResponse
            {
                Success = false,
                Message = $"Error retrieving user filters: {ex.Message}"
            });
        }
    }

    #endregion

    #region 写入APIs - 保存用户选择

    /// <summary>
    /// 更新Service选择
    /// PATCH /api/users/{userId}/filters/service
    /// </summary>
    [HttpPatch("users/{userId}/filters/service")]
    public async Task<ActionResult<object>> UpdateServiceSelection(
        Guid userId,
        [FromBody] UpdateServiceRequest request)
    {
        try
        {
            _logger.LogInformation("Updating service selection for user {UserId}: {ServiceType}", userId, request.ServiceType);

            await _filterService.UpdateServiceSelectionAsync(
                userId,
                request.SurveyId,
                request.ServiceType);

            return Ok(new
            {
                success = true,
                message = "Service selection updated successfully",
                data = new
                {
                    userId = userId,
                    surveyId = request.SurveyId,
                    serviceType = request.ServiceType
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating service selection for user {UserId}", userId);
            return StatusCode(500, new
            {
                success = false,
                message = $"Error updating service selection: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// 更新Region选择
    /// PATCH /api/users/{userId}/filters/regions
    /// </summary>
    [HttpPatch("users/{userId}/filters/regions")]
    public async Task<ActionResult<object>> UpdateRegionSelection(
        Guid userId,
        [FromBody] UpdateRegionsRequest request)
    {
        try
        {
            _logger.LogInformation("Updating region selection for user {UserId}: {Regions}", userId, string.Join(",", request.Regions));

            await _filterService.UpdateRegionSelectionAsync(
                userId,
                request.SurveyId,
                request.Regions);

            return Ok(new
            {
                success = true,
                message = "Region selection updated successfully",
                data = new
                {
                    userId = userId,
                    surveyId = request.SurveyId,
                    regions = request.Regions
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating region selection for user {UserId}", userId);
            return StatusCode(500, new
            {
                success = false,
                message = $"Error updating region selection: {ex.Message}"
            });
        }
    }

    #endregion
}
