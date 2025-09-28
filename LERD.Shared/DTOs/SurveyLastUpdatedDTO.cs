// LERD.Shared/DTOs/SurveyLastUpdatedDTO.cs
namespace LERD.Shared.DTOs;

/// <summary>
/// Survey last updated information response
/// Simple and direct - shows when data was last refreshed
/// </summary>
public class SurveyLastUpdatedResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public SurveyLastUpdatedData? Data { get; set; }
}

public class SurveyLastUpdatedData
{
    public string SurveyId { get; set; } = string.Empty;
    public DateTime LastUpdatedAt { get; set; }
    public string Source { get; set; } = "extraction_log"; // For debugging, shows data source
    public string FormattedTime { get; set; } = string.Empty; // Human-readable format
}
