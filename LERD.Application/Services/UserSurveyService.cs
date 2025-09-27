using LERD.Application.Interfaces;
using LERD.Domain.Entities;
using LERD.Infrastructure.Data;
using LERD.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LERD.Application.Services
{
    public class UserSurveyService : IUserSurveyService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserSurveyService> _logger;

        public UserSurveyService(ApplicationDbContext context, ILogger<UserSurveyService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<UserSurveysResponse> GetUserSurveysAsync(Guid userId)
        {
            // 简单的JOIN查询 - Linus式：直接了当，无废话
            var user = await _context.User
                .Include(u => u.Organisation)
                .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found or inactive", userId);
                return new UserSurveysResponse();
            }

            // 获取用户组织的所有活跃调查
            var surveys = await _context.Surveys
                .Where(s => s.OrganisationId == user.OrganisationId && s.Status == "active")
                .OrderBy(s => s.Name)
                .Select(s => new UserSurveyDto
                {
                    SurveyId = s.Id,
                    SurveyName = s.Name,
                    ServiceType = s.ServiceType ?? "",
                    Status = s.Status,
                    IsDefault = false  // 先设为false，后面处理默认逻辑
                })
                .ToListAsync();

            var response = new UserSurveysResponse { Surveys = surveys };
            
            // 设置默认调查：第一个就是默认的（简单规则）
            if (surveys.Any())
            {
                surveys[0].IsDefault = true;
                response.DefaultSurvey = surveys[0];
            }

            _logger.LogInformation("Found {SurveyCount} surveys for user {UserId}", surveys.Count, userId);
            return response;
        }

        public async Task<UserSurveyDto?> GetDefaultSurveyForUserAsync(Guid userId)
        {
            var userSurveys = await GetUserSurveysAsync(userId);
            return userSurveys.DefaultSurvey;
        }
    }
}
