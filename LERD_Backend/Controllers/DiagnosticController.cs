using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LERD.Infrastructure.Data;

namespace LERD_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DiagnosticController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public DiagnosticController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet("connection")]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                // 检查环境变量
                var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Not Set";
                var port = Environment.GetEnvironmentVariable("PORT") ?? "Not Set";
                var defaultConnection = _configuration.GetConnectionString("DefaultConnection") ?? "Not Set";
                var supabaseConnection = Environment.GetEnvironmentVariable("SUPABASE_CONNECTION_STRING") ?? "Not Set";
                var supabaseUrl = Environment.GetEnvironmentVariable("SUPABASE_URL") ?? "Not Set";
                var supabasePassword = Environment.GetEnvironmentVariable("SUPABASE_PASSWORD") ?? "Not Set";
                var dbHost = Environment.GetEnvironmentVariable("SUPABASE_DB_HOST") ?? "Not Set";
                var dbPort = Environment.GetEnvironmentVariable("SUPABASE_DB_PORT") ?? "Not Set";

                // 尝试数据库连接
                var canConnect = await _context.Database.CanConnectAsync();
                
                return Ok(new
                {
                    success = true,
                    message = "Diagnostic completed",
                    data = new
                    {
                        environment = new
                        {
                            ASPNETCORE_ENVIRONMENT = environment,
                            PORT = port
                        },
                        connectionStrings = new
                        {
                            DefaultConnection = defaultConnection?.Length > 20 ? $"{defaultConnection.Substring(0, 20)}..." : defaultConnection,
                            SUPABASE_CONNECTION_STRING = supabaseConnection?.Length > 20 ? $"{supabaseConnection.Substring(0, 20)}..." : supabaseConnection
                        },
                        supabaseVariables = new
                        {
                            SUPABASE_URL = supabaseUrl,
                            SUPABASE_DB_HOST = dbHost,
                            SUPABASE_DB_PORT = dbPort,
                            SUPABASE_PASSWORD = supabasePassword?.Length > 0 ? "***SET***" : "Not Set"
                        },
                        databaseConnection = new
                        {
                            canConnect = canConnect,
                            connectionState = _context.Database.GetConnectionString()?.Length > 20 ? 
                                $"{_context.Database.GetConnectionString().Substring(0, 20)}..." : 
                                _context.Database.GetConnectionString()
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Diagnostic failed",
                    error = ex.Message,
                    innerException = ex.InnerException?.Message
                });
            }
        }

        [HttpGet("simple-query")]
        public async Task<IActionResult> TestSimpleQuery()
        {
            try
            {
                // 尝试执行一个简单的查询
                var result = await _context.Database.ExecuteSqlRawAsync("SELECT 1");
                
                return Ok(new
                {
                    success = true,
                    message = "Simple query executed successfully",
                    result = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Simple query failed",
                    error = ex.Message,
                    innerException = ex.InnerException?.Message
                });
            }
        }
    }
}
