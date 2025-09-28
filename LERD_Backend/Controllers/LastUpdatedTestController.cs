using LERD.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LERD_Backend.Controllers;

/// <summary>
/// Temporary test controller for debugging Last Updated API
/// </summary>
[ApiController]
[Route("api/test")]
public class LastUpdatedTestController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<LastUpdatedTestController> _logger;

    public LastUpdatedTestController(
        ApplicationDbContext context,
        ILogger<LastUpdatedTestController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Test basic database connection and survey mapping
    /// </summary>
    [HttpGet("survey-mapping/{surveyId}")]
    public async Task<ActionResult> TestSurveyMapping(Guid surveyId)
    {
        try
        {
            _logger.LogInformation("Testing survey mapping for {SurveyId}", surveyId);

            // Test 1: Check if survey exists
            var surveyExists = await _context.Database
                .SqlQueryRaw<int>("SELECT COUNT(*) as value FROM surveys WHERE id = {0}", surveyId)
                .FirstOrDefaultAsync();

            _logger.LogInformation("Survey exists count: {Count}", surveyExists);

            // Test 2: Get survey details
            var survey = await _context.Database
                .SqlQueryRaw<SurveyTestResult>(@"
                    SELECT 
                        id,
                        qualtrics_survey_id,
                        name
                    FROM surveys 
                    WHERE id = {0}
                ", surveyId)
                .FirstOrDefaultAsync();

            if (survey == null)
            {
                return NotFound(new { message = "Survey not found", surveyId });
            }

            // Test 3: Check extraction log for this qualtrics_survey_id
            var extractionCount = await _context.Database
                .SqlQueryRaw<int>("SELECT COUNT(*) as value FROM survey_responses_extraction_log WHERE survey_id = {0}", 
                survey.QualtricsySurveyId)
                .FirstOrDefaultAsync();

            // Test 4: Get latest extraction record
            var latestExtraction = await _context.Database
                .SqlQueryRaw<ExtractionTestResult>(@"
                    SELECT 
                        survey_id,
                        extracted_at,
                        file_name
                    FROM survey_responses_extraction_log 
                    WHERE survey_id = {0}
                    ORDER BY extracted_at DESC
                    LIMIT 1
                ", survey.QualtricsySurveyId)
                .FirstOrDefaultAsync();

            return Ok(new
            {
                surveyId,
                survey = new
                {
                    id = survey.Id,
                    qualtricsSurveyId = survey.QualtricsySurveyId,
                    name = survey.Name
                },
                extractionLogCount = extractionCount,
                latestExtraction = latestExtraction == null ? null : new
                {
                    surveyId = latestExtraction.SurveyId,
                    extractedAt = latestExtraction.ExtractedAt,
                    fileName = latestExtraction.FileName
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in test survey mapping for {SurveyId}", surveyId);
            return StatusCode(500, new { error = ex.Message, surveyId });
        }
    }

    /// <summary>
    /// Test extraction log table directly
    /// </summary>
    [HttpGet("extraction-log")]
    public async Task<ActionResult> TestExtractionLog()
    {
        try
        {
            var allRecords = await _context.Database
                .SqlQueryRaw<ExtractionTestResult>(@"
                    SELECT 
                        survey_id,
                        extracted_at,
                        file_name
                    FROM survey_responses_extraction_log 
                    ORDER BY extracted_at DESC
                ")
                .ToListAsync();

            return Ok(new
            {
                totalRecords = allRecords.Count,
                records = allRecords
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing extraction log");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    public class SurveyTestResult
    {
        public Guid Id { get; set; }
        public string QualtricsySurveyId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class ExtractionTestResult
    {
        public string SurveyId { get; set; } = string.Empty;
        public DateTime ExtractedAt { get; set; }
        public string FileName { get; set; } = string.Empty;
    }
}
