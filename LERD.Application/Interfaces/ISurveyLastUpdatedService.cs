// LERD.Application/Interfaces/ISurveyLastUpdatedService.cs
using LERD.Shared.DTOs;

namespace LERD.Application.Interfaces;

/// <summary>
/// Service for retrieving survey data freshness information
/// Based on extraction_log - the authoritative source for "when did we last get new data"
/// </summary>
public interface ISurveyLastUpdatedService
{
    /// <summary>
    /// Get the last update time for a specific survey
    /// Uses extraction_log as the primary source (system-level data refresh time)
    /// </summary>
    /// <param name="surveyId">Survey GUID</param>
    /// <returns>Last updated information</returns>
    Task<SurveyLastUpdatedResponse> GetLastUpdatedAsync(Guid surveyId);
    
    /// <summary>
    /// Get last update times for multiple surveys
    /// Efficient batch operation for dashboard initialization
    /// </summary>
    /// <param name="surveyIds">List of survey GUIDs</param>
    /// <returns>Dictionary of survey ID to last updated info</returns>
    Task<Dictionary<string, SurveyLastUpdatedData>> GetLastUpdatedBatchAsync(List<Guid> surveyIds);
}
