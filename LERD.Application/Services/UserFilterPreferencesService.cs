using System.Text.Json;
using LERD.Application.Interfaces;
using LERD.Domain.Entities;
using LERD.Infrastructure.Data;
using LERD.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LERD.Application.Services;

public class UserFilterPreferencesService : IUserFilterPreferencesService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserFilterPreferencesService> _logger;

    // Facility Code to Region Name mapping (匹配ResponseChartService)
    private readonly Dictionary<string, string> _facilityMapping = new()
    {
        { "3001", "Bull Creek" },
        { "3002", "Coolbellup" },
        { "3003", "Mosman Park" },
        { "3004", "RoleyStone" },
        { "3005", "South Perth" },
        { "3006", "Unknown Facility" }, // 从chart API看到的额外facility
        { "3008", "Duncraig" }
    };

    public UserFilterPreferencesService(
        ApplicationDbContext context,
        ILogger<UserFilterPreferencesService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region 读取APIs - 获取选项列表

    public async Task<List<ServiceOption>> GetAvailableServicesAsync(Guid userId)
    {
        try
        {
            _logger.LogInformation("🔍 Getting available services for user {UserId}", userId);

            // 1. 获取用户
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("❌ User {UserId} not found", userId);
                return new List<ServiceOption>();
            }

            _logger.LogInformation("✅ User found: {UserId}, OrgId: {OrgId}", userId, user.OrganisationId);

            // 2. 先查看数据库中有多少surveys总数
            var totalSurveys = await _context.Surveys.CountAsync();
            _logger.LogInformation("🔍 Total surveys in database: {Count}", totalSurveys);

            var activeSurveys = await _context.Surveys.Where(s => s.Status.ToLower() == "active").CountAsync();
            _logger.LogInformation("🔍 Active surveys in database: {Count}", activeSurveys);

            // 3. 修复：直接比较status，不使用ToLower()
            var surveys = await _context.Surveys
                .Where(s => s.OrganisationId == user.OrganisationId && s.Status == "active")
                .Select(s => new ServiceOption
                {
                    SurveyId = s.Id,
                    ServiceType = s.ServiceType ?? "",
                    ServiceName = s.Name ?? "",
                    Description = s.Description,
                    IsSelected = false,
                    TotalResponses = 0
                })
                .ToListAsync();

            _logger.LogInformation("✅ Found {Count} active surveys for organisation {OrgId}", 
                surveys.Count, user.OrganisationId);

            // 如果没有找到surveys，让我们检查原始数据
            if (surveys.Count == 0)
            {
                var allSurveysWithStatus = await _context.Surveys
                    .Select(s => new { s.Id, s.Name, s.Status, s.ServiceType })
                    .ToListAsync();
                
                _logger.LogInformation("🔍 All surveys in DB: {@Surveys}", allSurveysWithStatus);
            }

            // 3. 获取用户当前选择的service
            var currentFilter = await _context.UserSavedFilters
                .Where(f => f.UserId == userId && f.IsDefault)
                .FirstOrDefaultAsync();

            Guid? selectedSurveyId = currentFilter?.SurveyId;

            // 4. 标记当前选择（暂时跳过响应数量查询以测试基本功能）
            foreach (var service in surveys)
            {
                service.IsSelected = service.SurveyId == selectedSurveyId;
                service.TotalResponses = 0; // 暂时设为0，后续修复SQL查询
            }

            _logger.LogInformation("✅ Returning {Count} services", surveys.Count);
            return surveys;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting available services for user {UserId}", userId);
            return new List<ServiceOption>();
        }
    }

    public async Task<List<RegionOption>> GetAvailableRegionsAsync(Guid surveyId)
    {
        try
        {
            _logger.LogInformation("🔍 Getting available regions for survey {SurveyId}", surveyId);

            // ✅ 修复：使用实际的JSON结构 - Facility在根级别，不在participant_info里！
            var facilityData = await _context.Database
                .SqlQueryRaw<FacilityData>(@"
                    SELECT 
                        response_data->>'Facility' as FacilityCode,
                        COUNT(*)::integer as ParticipantCount
                    FROM survey_responses 
                    WHERE survey_id = {0}
                      AND response_data->>'Facility' IS NOT NULL
                    GROUP BY response_data->>'Facility'
                ", surveyId)
                .ToListAsync();

            _logger.LogInformation("📊 Found {Count} unique facilities", facilityData.Count);

            // 转换为RegionOption并应用facility mapping  
            var regions = facilityData
                .Where(f => !string.IsNullOrEmpty(f.FacilityCode))
                .Select(f => new RegionOption
                {
                    FacilityCode = f.FacilityCode,
                    RegionName = _facilityMapping.GetValueOrDefault(f.FacilityCode, f.FacilityCode),
                    ParticipantCount = f.ParticipantCount,
                    IsSelected = false
                })
                .OrderBy(r => r.FacilityCode)
                .ToList();

            _logger.LogInformation("✅ Returning {Count} regions", regions.Count);
            return regions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting available regions for survey {SurveyId}", surveyId);
            return new List<RegionOption>();
        }
    }

    public async Task<FilterOptions> GetFilterOptionsAsync(Guid surveyId)
    {
        try
        {
            _logger.LogInformation("Getting filter options for survey {SurveyId}", surveyId);

            // 获取Gender选项
            var genderOptions = new List<FilterOption>
            {
                new() { Value = "1", Label = "Male" },
                new() { Value = "2", Label = "Female" }
            };

            // 获取ParticipantType选项
            var participantTypeOptions = new List<FilterOption>
            {
                new() { Value = "1", Label = "Resident" },
                new() { Value = "2", Label = "Family/Friend" }
            };

            // 获取Period选项（从数据中提取）
            var periods = await _context.Database
                .SqlQueryRaw<string>(@"
                    SELECT DISTINCT CONCAT(period_year, '-', LPAD(period_month::text, 2, '0')) as period
                    FROM survey_responses 
                    WHERE survey_id = {0}
                      AND period_year IS NOT NULL
                    ORDER BY CONCAT(period_year, '-', LPAD(period_month::text, 2, '0')) DESC
                ", surveyId)
                .ToListAsync();

            return new FilterOptions
            {
                Gender = genderOptions,
                ParticipantType = participantTypeOptions,
                Period = periods
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting filter options for survey {SurveyId}", surveyId);
            return new FilterOptions();
        }
    }

    #endregion

    #region 读取APIs - 获取当前选择

    public async Task<FilterConfiguration> GetUserFiltersAsync(Guid userId, Guid surveyId)
    {
        try
        {
            _logger.LogInformation("Getting user filters for user {UserId}, survey {SurveyId}", userId, surveyId);

            var savedFilter = await _context.UserSavedFilters
                .FirstOrDefaultAsync(f => f.UserId == userId && f.SurveyId == surveyId);

            if (savedFilter == null)
            {
                _logger.LogInformation("No saved filters found, initializing defaults for user {UserId}, survey {SurveyId}", userId, surveyId);
                return await InitializeDefaultFiltersAsync(userId, surveyId);
            }

            return ParseFilterConfiguration(savedFilter.FilterConfiguration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user filters for user {UserId}, survey {SurveyId}", userId, surveyId);
            return new FilterConfiguration();
        }
    }

    #endregion

    #region 写入APIs - 保存用户选择

    public async Task UpdateServiceSelectionAsync(Guid userId, Guid surveyId, string serviceType)
    {
        try
        {
            _logger.LogInformation("Updating service selection for user {UserId}: {ServiceType}", userId, serviceType);

            // 1. 删除用户的旧default filter（如果存在）
            var existingFilters = await _context.UserSavedFilters
                .Where(f => f.UserId == userId && f.IsDefault)
                .ToListAsync();

            _context.UserSavedFilters.RemoveRange(existingFilters);

            // 2. 获取新service的所有regions作为默认选择
            var availableRegions = await GetAvailableRegionsAsync(surveyId);
            var allRegionCodes = availableRegions.Select(r => r.FacilityCode).ToList();

            // 3. 创建新的default filter configuration
            var config = new FilterConfiguration
            {
                ServiceType = new SingleSelectFilter 
                { 
                    Type = "single_select", 
                    Value = serviceType 
                },
                Region = new MultiSelectFilter 
                { 
                    Type = "multi_select", 
                    Values = allRegionCodes 
                }
            };

            // 4. 保存新的filter
            var newFilter = new UserSavedFilter
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                SurveyId = surveyId,
                FilterName = "default",
                FilterConfiguration = SerializeFilterConfiguration(config),
                IsDefault = true,
                LastUsedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.UserSavedFilters.Add(newFilter);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Service selection updated successfully for user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating service selection for user {UserId}", userId);
            throw;
        }
    }

    public async Task UpdateRegionSelectionAsync(Guid userId, Guid surveyId, List<string> regions)
    {
        try
        {
            _logger.LogInformation("Updating region selection for user {UserId}: {Regions}", userId, string.Join(",", regions));

            var filter = await GetOrCreateFilterAsync(userId, surveyId);
            var config = ParseFilterConfiguration(filter.FilterConfiguration);

            config.Region = new MultiSelectFilter
            {
                Type = "multi_select",
                Values = regions
            };

            filter.FilterConfiguration = SerializeFilterConfiguration(config);
            filter.UpdatedAt = DateTime.UtcNow;
            filter.LastUsedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Region selection updated successfully for user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating region selection for user {UserId}", userId);
            throw;
        }
    }

    #endregion

    #region 初始化

    public async Task<FilterConfiguration> InitializeDefaultFiltersAsync(Guid userId, Guid surveyId)
    {
        try
        {
            _logger.LogInformation("Initializing default filters for user {UserId}, survey {SurveyId}", userId, surveyId);

            // 获取survey信息
            var survey = await _context.Surveys.FindAsync(surveyId);
            if (survey == null)
            {
                throw new ArgumentException($"Survey {surveyId} not found");
            }

            // 获取该survey的所有regions作为默认选择
            var availableRegions = await GetAvailableRegionsAsync(surveyId);
            var allRegionCodes = availableRegions.Select(r => r.FacilityCode).ToList();

            var defaultConfig = new FilterConfiguration
            {
                ServiceType = new SingleSelectFilter 
                { 
                    Type = "single_select", 
                    Value = survey.ServiceType ?? "" 
                },
                Region = new MultiSelectFilter 
                { 
                    Type = "multi_select", 
                    Values = allRegionCodes 
                }
            };

            // 保存到数据库
            var newFilter = new UserSavedFilter
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                SurveyId = surveyId,
                FilterName = "default",
                FilterConfiguration = SerializeFilterConfiguration(defaultConfig),
                IsDefault = true,
                LastUsedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.UserSavedFilters.Add(newFilter);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Default filters initialized successfully for user {UserId}", userId);
            return defaultConfig;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing default filters for user {UserId}, survey {SurveyId}", userId, surveyId);
            throw;
        }
    }

    #endregion

    #region Helper Methods

    private async Task<UserSavedFilter> GetOrCreateFilterAsync(Guid userId, Guid surveyId)
    {
        var filter = await _context.UserSavedFilters
            .FirstOrDefaultAsync(f => f.UserId == userId && f.SurveyId == surveyId);

        if (filter == null)
        {
            await InitializeDefaultFiltersAsync(userId, surveyId);
            filter = await _context.UserSavedFilters
                .FirstOrDefaultAsync(f => f.UserId == userId && f.SurveyId == surveyId);
        }

        return filter!;
    }

    private FilterConfiguration ParseFilterConfiguration(JsonDocument jsonDoc)
    {
        try
        {
            var json = jsonDoc.RootElement.GetRawText();
            return JsonSerializer.Deserialize<FilterConfiguration>(json) ?? new FilterConfiguration();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing filter configuration");
            return new FilterConfiguration();
        }
    }

    private JsonDocument SerializeFilterConfiguration(FilterConfiguration config)
    {
        try
        {
            var json = JsonSerializer.Serialize(config);
            return JsonDocument.Parse(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error serializing filter configuration");
            return JsonDocument.Parse("{}");
        }
    }

    #endregion

    // Helper classes for SQL query results
    private class FacilityData
    {
        public string FacilityCode { get; set; } = string.Empty;
        public int ParticipantCount { get; set; }
    }

    private class ServiceQueryResult
    {
        public Guid SurveyId { get; set; }
        public string? ServiceType { get; set; }
        public string? ServiceName { get; set; }
        public string? Description { get; set; }
    }

    private class CountResult
    {
        public int Count { get; set; }
    }
}
