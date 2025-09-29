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

    // Facility Code to Region Name mapping
    private readonly Dictionary<string, string> _facilityMapping = new()
    {
        { "1", "Mosman Park" },
        { "2", "Bull Creek" },
        { "3", "Coolbellup/Salter Point" },
        { "4", "Roley Stone/Mandurah" },
        { "5", "Duncraig" },
        { "6", "South Perth/Karrinyup" }
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
            _logger.LogInformation("Getting available services for user {UserId}", userId);

            // 1. 获取用户的organisation（如果用户有组织限制）
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found", userId);
                return new List<ServiceOption>();
            }

            _logger.LogInformation("User found: {UserId}", userId);

            // 2. 获取所有active surveys（使用原始SQL查询）
            var serviceResults = await _context.Database
                .SqlQueryRaw<ServiceQueryResult>(@"
                    SELECT 
                        id as SurveyId,
                        service_type as ServiceType,
                        name as ServiceName,
                        description as Description
                    FROM surveys 
                    WHERE status = 'active'
                    ORDER BY name
                ")
                .ToListAsync();

            var services = serviceResults.Select(s => new ServiceOption
            {
                SurveyId = s.SurveyId,
                ServiceType = s.ServiceType ?? "",
                ServiceName = s.ServiceName ?? "",
                Description = s.Description,
                IsSelected = false // Will be set below
            }).ToList();

            // 3. 获取用户当前选择的service
            var currentFilter = await _context.UserSavedFilters
                .Where(f => f.UserId == userId && f.IsDefault)
                .FirstOrDefaultAsync();

            Guid? selectedSurveyId = null;
            if (currentFilter != null)
            {
                selectedSurveyId = currentFilter.SurveyId;
            }

            // 4. 标记当前选择并获取响应数量
            foreach (var service in services)
            {
                service.IsSelected = service.SurveyId == selectedSurveyId;
                
                // 获取该service的响应数量（使用简单的SQL查询）
                var countResult = await _context.Database
                    .SqlQueryRaw<CountResult>("SELECT COUNT(*) as Count FROM survey_responses WHERE survey_id = {0}", service.SurveyId)
                    .FirstOrDefaultAsync();
                service.TotalResponses = countResult?.Count ?? 0;
            }

            _logger.LogInformation("Found {Count} available services for user {UserId}", services.Count, userId);
            return services;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available services for user {UserId}", userId);
            return new List<ServiceOption>();
        }
    }

    public async Task<List<RegionOption>> GetAvailableRegionsAsync(Guid surveyId)
    {
        try
        {
            _logger.LogInformation("Getting available regions for survey {SurveyId}", surveyId);

            // 从survey_responses中提取所有唯一的facility codes
            var facilityData = await _context.Database
                .SqlQueryRaw<FacilityData>(@"
                    SELECT 
                        response_data->>'$.participant_info.facility' as FacilityCode,
                        COUNT(*) as ParticipantCount
                    FROM survey_responses 
                    WHERE survey_id = {0}
                      AND response_data->>'$.participant_info.facility' IS NOT NULL
                    GROUP BY response_data->>'$.participant_info.facility'
                ", surveyId)
                .ToListAsync();

            // 转换为RegionOption并应用facility mapping
            var regions = facilityData
                .Where(f => !string.IsNullOrEmpty(f.FacilityCode))
                .Select(f => new RegionOption
                {
                    FacilityCode = f.FacilityCode,
                    RegionName = _facilityMapping.GetValueOrDefault(f.FacilityCode, f.FacilityCode),
                    ParticipantCount = f.ParticipantCount,
                    IsSelected = false // Will be set by GetUserFiltersAsync
                })
                .OrderBy(r => r.RegionName)
                .ToList();

            _logger.LogInformation("Found {Count} available regions for survey {SurveyId}", regions.Count, surveyId);
            return regions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available regions for survey {SurveyId}", surveyId);
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
                    SELECT DISTINCT CONCAT(period_year, '-', LPAD(period_month::text, 2, '0'))
                    FROM survey_responses 
                    WHERE survey_id = {0}
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
