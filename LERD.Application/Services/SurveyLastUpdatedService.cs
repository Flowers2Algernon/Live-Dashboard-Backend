// LERD.Application/Services/SurveyLastUpdatedService.cs
using LERD.Application.Interfaces;
using LERD.Infrastructure.Data;
using LERD.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LERD.Application.Services;

/// <summary>
/// Survey last updated service - Linus式修复版本
/// 
/// 修复问题：
/// 1. SQL语法错误
/// 2. 更详细的错误日志
/// 3. 防御性编程
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
            _logger.LogInformation("Getting last updated time for survey {SurveyId}", surveyId);

            // Step 1: Get qualtrics_survey_id from surveys table
            var survey = await _context.Database
                .SqlQueryRaw<SurveyMappingResult>(@"
                    SELECT 
                        id as SurveyGuid,
                        qualtrics_survey_id as QualtricsySurveyId
                    FROM surveys 
                    WHERE id = {0}
                ", surveyId)
                .FirstOrDefaultAsync();

            if (survey == null)
            {
                _logger.LogWarning("Survey not found with ID {SurveyId}", surveyId);
                return new SurveyLastUpdatedResponse
                {
                    Success = false,
                    Message = "Survey not found",
                    Data = null
                };
            }

            _logger.LogInformation("Found survey with qualtrics_survey_id: {QualtricsId}", survey.QualtricsySurveyId);

            // Step 2: Use qualtrics_survey_id to query extraction log - 修复SQL语法
            var lastUpdated = await _context.Database
                .SqlQueryRaw<LastUpdatedResult>(@"
                    SELECT 
                        survey_id as SurveyId,
                        MAX(extracted_at) as LastUpdatedAt
                    FROM survey_responses_extraction_log 
                    WHERE survey_id = {0}
                    GROUP BY survey_id
                ", survey.QualtricsySurveyId)
                .FirstOrDefaultAsync();

            if (lastUpdated == null)
            {
                _logger.LogWarning("No extraction log found for survey {SurveyId} with qualtrics_id {QualtricsId}", 
                    surveyId, survey.QualtricsySurveyId);
                
                return new SurveyLastUpdatedResponse
                {
                    Success = false,
                    Message = $"No data refresh history found for this survey (qualtrics_id: {survey.QualtricsySurveyId})",
                    Data = null
                };
            }

            _logger.LogInformation("Found last updated time: {LastUpdated} for survey {SurveyId}", 
                lastUpdated.LastUpdatedAt, surveyId);

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
            _logger.LogError(ex, "Error retrieving last updated time for survey {SurveyId}. " +
                "Exception: {ExceptionType}, Message: {ExceptionMessage}, " +
                "StackTrace: {StackTrace}", 
                surveyId, ex.GetType().Name, ex.Message, ex.StackTrace);
            
            return new SurveyLastUpdatedResponse
            {
                Success = false,
                Message = $"Error retrieving last updated time: {ex.Message}",
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

            var results = new List<LastUpdatedResult>();
            
            foreach (var surveyId in surveyIds)
            {
                // Step 1: Get qualtrics_survey_id
                var survey = await _context.Database
                    .SqlQueryRaw<SurveyMappingResult>(@"
                        SELECT 
                            id as SurveyGuid,
                            qualtrics_survey_id as QualtricsySurveyId
                        FROM surveys 
                        WHERE id = {0}
                    ", surveyId)
                    .FirstOrDefaultAsync();

                if (survey == null) continue;

                // Step 2: Query extraction log with qualtrics_survey_id - 修复SQL语法
                var result = await _context.Database
                    .SqlQueryRaw<LastUpdatedResult>(@"
                        SELECT 
                            survey_id as SurveyId,
                            MAX(extracted_at) as LastUpdatedAt
                        FROM survey_responses_extraction_log 
                        WHERE survey_id = {0}
                        GROUP BY survey_id
                    ", survey.QualtricsySurveyId)
                    .FirstOrDefaultAsync();
                    
                if (result != null)
                {
                    results.Add(result);
                }
            }

            var dictionary = new Dictionary<string, SurveyLastUpdatedData>();

            foreach (var result in results)
            {
                dictionary[result.SurveyId] = new SurveyLastUpdatedData
                {
                    SurveyId = result.SurveyId,
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
            _logger.LogError(ex, "Error retrieving batch last updated times for {Count} surveys: {ExceptionMessage}", 
                surveyIds.Count, ex.Message);
            return new Dictionary<string, SurveyLastUpdatedData>();
        }
    }

    /// <summary>
    /// Internal class for SQL query results
    /// Maps to the raw SQL output from extraction_log
    /// </summary>
    private class LastUpdatedResult
    {
        public string SurveyId { get; set; } = string.Empty;
        public DateTime LastUpdatedAt { get; set; }
    }

    /// <summary>
    /// Internal class for survey mapping query results
    /// Maps GUID survey ID to Qualtrics survey ID
    /// </summary>
    private class SurveyMappingResult
    {
        public Guid SurveyGuid { get; set; }
        public string QualtricsySurveyId { get; set; } = string.Empty;
    }
}
