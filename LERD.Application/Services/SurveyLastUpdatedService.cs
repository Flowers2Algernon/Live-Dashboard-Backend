// LERD.Application/Services/SurveyLastUpdatedService.cs
using LERD.Application.Interfaces;
using LERD.Infrastructure.Data;
using LERD.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LERD.Application.Services;

/// <summary>
/// Survey last updated service - Linus式实现：简单、直接、高效
/// 
/// 核心原则：
/// 1. extraction_log是权威数据源（系统最后一次获取数据的时间）
/// 2. 一个SQL查询解决问题，不搞复杂的fallback
/// 3. 快速响应，稳定性能
/// </summary>
public class SurveyLastUpdatedService : ISurveyLastUpdatedService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SurveyLastUpdatedService> _logger;

    public SurveyLastUpdatedService(
        ApplicationDbContext context,
        ILogger<SurveyLastUpdatedService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<SurveyLastUpdatedResponse> GetLastUpdatedAsync(Guid surveyId)
    {
        try
        {
            // Linus式SQL：简单直接，一次查询解决问题
            var lastUpdated = await _context.Database
                .SqlQueryRaw<LastUpdatedResult>(@"
                    SELECT 
                        survey_id,
                        MAX(extracted_at) as last_updated_at
                    FROM survey_responses_extraction_log 
                    WHERE survey_id = {0}
                    GROUP BY survey_id
                ", surveyId)
                .FirstOrDefaultAsync();

            if (lastUpdated == null)
            {
                _logger.LogWarning("No extraction log found for survey {SurveyId}", surveyId);
                
                return new SurveyLastUpdatedResponse
                {
                    Success = false,
                    Message = "No data refresh history found for this survey",
                    Data = null
                };
            }

            return new SurveyLastUpdatedResponse
            {
                Success = true,
                Message = "Last updated time retrieved successfully",
                Data = new SurveyLastUpdatedData
                {
                    SurveyId = surveyId.ToString(),
                    LastUpdatedAt = lastUpdated.LastUpdatedAt,
                    Source = "extraction_log",
                    FormattedTime = lastUpdated.LastUpdatedAt.ToString("yyyy-MM-dd HH:mm:ss UTC")
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving last updated time for survey {SurveyId}", surveyId);
            
            return new SurveyLastUpdatedResponse
            {
                Success = false,
                Message = "Error retrieving last updated time",
                Data = null
            };
        }
    }

    public async Task<Dictionary<string, SurveyLastUpdatedData>> GetLastUpdatedBatchAsync(List<Guid> surveyIds)
    {
        try
        {
            if (!surveyIds.Any())
            {
                return new Dictionary<string, SurveyLastUpdatedData>();
            }

            // 批量查询：使用参数化查询防止SQL注入
            var results = new List<LastUpdatedResult>();
            
            foreach (var surveyId in surveyIds)
            {
                var result = await _context.Database
                    .SqlQueryRaw<LastUpdatedResult>(@"
                        SELECT 
                            survey_id,
                            MAX(extracted_at) as last_updated_at
                        FROM survey_responses_extraction_log 
                        WHERE survey_id = {0}
                        GROUP BY survey_id
                    ", surveyId)
                    .FirstOrDefaultAsync();
                    
                if (result != null)
                {
                    results.Add(result);
                }
            }

            var dictionary = new Dictionary<string, SurveyLastUpdatedData>();

            foreach (var result in results)
            {
                dictionary[result.SurveyId.ToString()] = new SurveyLastUpdatedData
                {
                    SurveyId = result.SurveyId.ToString(),
                    LastUpdatedAt = result.LastUpdatedAt,
                    Source = "extraction_log",
                    FormattedTime = result.LastUpdatedAt.ToString("yyyy-MM-dd HH:mm:ss UTC")
                };
            }

            _logger.LogDebug("Retrieved last updated times for {Count} surveys", results.Count);
            
            return dictionary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving batch last updated times for {Count} surveys", surveyIds.Count);
            return new Dictionary<string, SurveyLastUpdatedData>();
        }
    }

    /// <summary>
    /// Internal class for SQL query results
    /// Maps to the raw SQL output from extraction_log
    /// </summary>
    private class LastUpdatedResult
    {
        public Guid SurveyId { get; set; }
        public DateTime LastUpdatedAt { get; set; }
    }
}
