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
    }
}
