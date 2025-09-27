using Microsoft.AspNetCore.Mvc;
using LERD.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LERD_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DebugController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DebugController> _logger;

        public DebugController(ApplicationDbContext context, ILogger<DebugController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("test-db")]
        public async Task<IActionResult> TestDatabase()
        {
            try
            {
                // 测试基本连接
                var canConnect = await _context.Database.CanConnectAsync();
                _logger.LogInformation("Database connection test: {CanConnect}", canConnect);

                if (!canConnect)
                {
                    return Ok(new { success = false, message = "Cannot connect to database" });
                }

                // 测试survey_responses表是否存在
                var sql = "SELECT COUNT(*) FROM survey_responses WHERE survey_id = @surveyId";
                var surveyId = Guid.Parse("8dff523d-2a46-4ee3-8017-614af3813b32");
                
                var responseCount = await _context.Database
                    .ExecuteSqlRawAsync("SELECT 1"); // 简单测试SQL执行

                return Ok(new 
                { 
                    success = true, 
                    canConnect = true,
                    message = "Database connection successful",
                    testSurveyId = surveyId.ToString()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database test failed");
                return Ok(new 
                { 
                    success = false, 
                    message = "Database test failed", 
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        [HttpGet("test-survey-responses")]
        public async Task<IActionResult> TestSurveyResponses()
        {
            try
            {
                var surveyId = Guid.Parse("8dff523d-2a46-4ee3-8017-614af3813b32");
                
                // 直接执行原始SQL查询
                var connectionString = _context.Database.GetConnectionString();
                _logger.LogInformation("Using connection string (masked): {ConnectionString}", 
                    connectionString != null ? connectionString.Substring(0, Math.Min(50, connectionString.Length)) + "..." : "null");

                using var connection = new Npgsql.NpgsqlConnection(connectionString);
                await connection.OpenAsync();

                var sql = "SELECT COUNT(*) FROM survey_responses WHERE survey_id = @surveyId";
                using var command = new Npgsql.NpgsqlCommand(sql, connection);
                command.Parameters.AddWithValue("surveyId", surveyId);

                var count = await command.ExecuteScalarAsync();
                
                return Ok(new 
                { 
                    success = true, 
                    surveyResponseCount = count,
                    surveyId = surveyId.ToString(),
                    message = "Survey responses query successful"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Survey responses test failed");
                return Ok(new 
                { 
                    success = false, 
                    message = "Survey responses test failed", 
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        [HttpGet("test-chart-sql")]
        public async Task<IActionResult> TestChartSql()
        {
            try
            {
                var surveyId = Guid.Parse("8dff523d-2a46-4ee3-8017-614af3813b32");
                var connectionString = _context.Database.GetConnectionString();

                using var connection = new Npgsql.NpgsqlConnection(connectionString);
                await connection.OpenAsync();

                // 测试与CustomerSatisfactionService相同的SQL查询
                var sql = @"
                    WITH response_records AS (
                        SELECT 
                            response_element->>'Facility' as facility_code,
                            response_element->>'Gender' as gender,
                            response_element->>'ParticipantType' as participant_type,
                            response_element->>'EndDate' as end_date,
                            response_element->>'Satisfaction' as satisfaction_code
                        FROM survey_responses sr,
                             jsonb_array_elements(sr.response_data) as response_element
                        WHERE sr.survey_id = @surveyId
                          AND response_element->>'Satisfaction' IS NOT NULL
                          AND response_element->>'NPS_NPS_GROUP' IS NOT NULL
                          AND (@gender IS NULL OR response_element->>'Gender' = @gender)
                          AND (@participantType IS NULL OR response_element->>'ParticipantType' = @participantType)
                          AND (@period IS NULL OR response_element->>'EndDate' LIKE @period)
                    ),
                    total_count AS (
                      SELECT COUNT(*) as total FROM response_records
                    )
                    SELECT 
                      COALESCE(
                        ROUND(
                          COUNT(CASE WHEN satisfaction_code = '6' THEN 1 END) * 100.0 / NULLIF(tc.total, 0), 1
                        ), 0
                      ) as very_satisfied_percentage,
                      
                      tc.total as total_responses

                    FROM response_records, total_count tc
                    GROUP BY tc.total;";

                using var command = new Npgsql.NpgsqlCommand(sql, connection);
                command.Parameters.AddWithValue("surveyId", surveyId);
                command.Parameters.AddWithValue("gender", DBNull.Value);
                command.Parameters.AddWithValue("participantType", DBNull.Value);
                command.Parameters.AddWithValue("period", DBNull.Value);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var verySatisfied = reader.GetDecimal(0);
                    var totalResponses = reader.GetInt32(1);
                    
                    return Ok(new 
                    { 
                        success = true, 
                        verySatisfiedPercentage = verySatisfied,
                        totalResponses = totalResponses,
                        message = "Chart SQL query successful"
                    });
                }
                else
                {
                    return Ok(new 
                    { 
                        success = false, 
                        message = "No data returned from chart query"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chart SQL test failed");
                return Ok(new 
                { 
                    success = false, 
                    message = "Chart SQL test failed", 
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }
    }
}
