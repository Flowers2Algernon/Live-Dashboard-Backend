using Microsoft.AspNetCore.Mvc;
using LERD.Application.Interfaces;
using LERD.Shared.DTOs;

namespace LERD_Backend.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserSurveysController : ControllerBase
    {
        private readonly IUserSurveyService _userSurveyService;
        private readonly ILogger<UserSurveysController> _logger;

        public UserSurveysController(IUserSurveyService userSurveyService, ILogger<UserSurveysController> logger)
        {
            _userSurveyService = userSurveyService;
            _logger = logger;
        }

        /// <summary>
        /// 获取用户可访问的所有调查
        /// </summary>
        [HttpGet("{userId}/surveys")]
        public async Task<IActionResult> GetUserSurveys(Guid userId)
        {
            try
            {
                if (userId == Guid.Empty)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Valid user ID is required"
                    });
                }

                var surveys = await _userSurveyService.GetUserSurveysAsync(userId);
                
                return Ok(new ApiResponse<UserSurveysResponse>
                {
                    Success = true,
                    Data = surveys,
                    Message = surveys.Surveys.Any() 
                        ? $"Found {surveys.Surveys.Count} surveys" 
                        : "No surveys found for user"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting surveys for user {UserId}", userId);
                
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving user surveys"
                });
            }
        }

        /// <summary>
        /// 获取用户的默认调查（用于仪表板初始化）
        /// </summary>
        [HttpGet("{userId}/surveys/default")]
        public async Task<IActionResult> GetDefaultSurvey(Guid userId)
        {
            try
            {
                if (userId == Guid.Empty)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Valid user ID is required"
                    });
                }

                var defaultSurvey = await _userSurveyService.GetDefaultSurveyForUserAsync(userId);
                
                if (defaultSurvey == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "No default survey found for user"
                    });
                }

                return Ok(new ApiResponse<UserSurveyDto>
                {
                    Success = true,
                    Data = defaultSurvey,
                    Message = "Default survey retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting default survey for user {UserId}", userId);
                
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving default survey"
                });
            }
        }
    }
}
