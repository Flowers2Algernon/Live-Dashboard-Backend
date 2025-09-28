// LERD_Backend/Controllers/NewDebugController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LERD.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace LERD_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NewDebugController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<NewDebugController> _logger;

    public NewDebugController(ApplicationDbContext context, ILogger<NewDebugController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("last-updated-raw/{surveyId}")]
    public async Task<IActionResult> TestLastUpdatedRaw(Guid surveyId)
    {
        try
        {
            var connectionString = _context.Database.GetConnectionString();
            
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            var result = new
            {
                Step1_SurveyLookup = await TestStep1(connection, surveyId),
                Step2_ExtractionLogQuery = await TestStep2(connection),
                Step3_DirectQuery = await TestStep3(connection, surveyId)
            };

            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Debug test failed for survey {SurveyId}", surveyId);
            return Ok(new 
            { 
                success = false, 
                error = ex.Message,
                innerError = ex.InnerException?.Message,
                stackTrace = ex.StackTrace
            });
        }
    }

    private async Task<object> TestStep1(NpgsqlConnection connection, Guid surveyId)
    {
        var sql = @"
            SELECT 
                id as survey_guid,
                qualtrics_survey_id,
                name,
                status
            FROM surveys 
            WHERE id = @surveyId";

        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("surveyId", surveyId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new
            {
                Found = true,
                SurveyGuid = reader.GetGuid(0),
                QualtricsId = reader.GetString(1),
                Name = reader.IsDBNull(2) ? null : reader.GetString(2),
                Status = reader.IsDBNull(3) ? null : reader.GetString(3)
            };
        }
        
        return new { Found = false };
    }

    private async Task<object> TestStep2(NpgsqlConnection connection)
    {
        var sql = @"
            SELECT 
                survey_id,
                extracted_at,
                file_name,
                file_size
            FROM survey_responses_extraction_log 
            ORDER BY extracted_at DESC 
            LIMIT 5";

        using var command = new NpgsqlCommand(sql, connection);
        using var reader = await command.ExecuteReaderAsync();

        var records = new List<object>();
        while (await reader.ReadAsync())
        {
            records.Add(new
            {
                SurveyId = reader.GetString(0),
                ExtractedAt = reader.GetDateTime(1),
                FileName = reader.GetString(2),
                FileSize = reader.GetInt64(3)
            });
        }

        return new { TotalRecords = records.Count, Records = records };
    }

    private async Task<object> TestStep3(NpgsqlConnection connection, Guid surveyId)
    {
        // 首先获取 qualtrics_survey_id
        var surveyLookupSql = "SELECT qualtrics_survey_id FROM surveys WHERE id = @surveyId";
        using var surveyCommand = new NpgsqlCommand(surveyLookupSql, connection);
        surveyCommand.Parameters.AddWithValue("surveyId", surveyId);
        
        var qualtricsId = await surveyCommand.ExecuteScalarAsync() as string;
        
        if (qualtricsId == null)
        {
            return new { Found = false, Message = "Survey not found" };
        }

        // 然后查询 extraction_log
        var extractionSql = @"
            SELECT 
                survey_id,
                MAX(extracted_at) as last_updated_at,
                COUNT(*) as record_count
            FROM survey_responses_extraction_log 
            WHERE survey_id = @qualtricsId
            GROUP BY survey_id";

        using var extractionCommand = new NpgsqlCommand(extractionSql, connection);
        extractionCommand.Parameters.AddWithValue("qualtricsId", qualtricsId);

        using var reader = await extractionCommand.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new
            {
                Found = true,
                QualtricsId = qualtricsId,
                SurveyId = reader.GetString(0),
                LastUpdatedAt = reader.GetDateTime(1),
                RecordCount = reader.GetInt32(2)
            };
        }

        return new 
        { 
            Found = false, 
            QualtricsId = qualtricsId,
            Message = "No extraction log found for this qualtrics_survey_id" 
        };
    }

    [HttpGet("test-ef-core/{surveyId}")]
    public async Task<IActionResult> TestEfCore(Guid surveyId)
    {
        try
        {
            // 测试 Entity Framework 查询
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
                return Ok(new { success = false, message = "Survey not found via EF Core" });
            }

            // 测试 extraction_log 查询
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

            return Ok(new 
            { 
                success = true, 
                survey = new { survey.SurveyGuid, survey.QualtricsySurveyId },
                lastUpdated = lastUpdated != null ? new { lastUpdated.SurveyId, lastUpdated.LastUpdatedAt } : null
            });
        }
        catch (Exception ex)
        {
            return Ok(new 
            { 
                success = false, 
                error = ex.Message,
                innerError = ex.InnerException?.Message,
                type = ex.GetType().Name
            });
        }
    }

    public class SurveyMappingResult
    {
        public Guid SurveyGuid { get; set; }
        public string QualtricsySurveyId { get; set; } = string.Empty;
    }

    public class LastUpdatedResult
    {
        public string SurveyId { get; set; } = string.Empty;
        public DateTime LastUpdatedAt { get; set; }
    }
}
