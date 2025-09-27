using Microsoft.AspNetCore.Mvc;
using LERD.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LERD_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HealthController(ApplicationDbContext context)
        {
            _context = context;
        }



        [HttpGet("database-simple")]
        public async Task<IActionResult> CheckSimpleDatabaseQuery()
        {
            try
            {
                // 尝试简单的查询
                var canConnect = await _context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    return StatusCode(500, new { Status = "Failed", Message = "Cannot connect to database" });
                }

                // 尝试查询第一个组织
                var firstOrg = await _context.Organisations.FirstOrDefaultAsync();
                
                return Ok(new
                {
                    Status = "Success",
                    Message = "简单数据库查询成功",
                    HasData = firstOrg != null,
                    FirstOrgId = firstOrg?.Id,
                    FirstOrgName = firstOrg?.Name,
                    Timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Status = "Error",
                    Message = "简单数据库查询失败",
                    Error = ex.Message,
                    InnerException = ex.InnerException?.Message,
                    Timestamp = DateTime.Now
                });
            }
        }

        [HttpGet("database")]
        public async Task<IActionResult> CheckDatabaseConnection()
        {
            try
            {
                // 尝试连接数据库
                var canConnect = await _context.Database.CanConnectAsync();
                
                if (canConnect)
                {
                    // 使用简单查询代替CountAsync避免超时
                    var firstOrg = await _context.Organisations.FirstOrDefaultAsync();
                    
                    return Ok(new
                    {
                        Status = "Success",
                        Message = "数据库连接成功",
                        HasData = firstOrg != null,
                        FirstOrgName = firstOrg?.Name,
                        ConnectionString = "aws-1-ap-southeast-2.pooler.supabase.com:6543",
                        Timestamp = DateTime.Now
                    });
                }
                else
                {
                    return StatusCode(500, new
                    {
                        Status = "Failed",
                        Message = "数据库可达但无法连接",
                        Details = "CanConnectAsync returned false",
                        Timestamp = DateTime.Now
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Status = "Error",
                    Message = "数据库连接测试失败",
                    Error = ex.Message,
                    InnerException = ex.InnerException?.Message,
                    StackTrace = ex.StackTrace?.Split('\n').Take(3).ToArray(),
                    Timestamp = DateTime.Now
                });
            }
        }

        [HttpGet]
        public IActionResult Health()
        {
            return Ok(new
            {
                Status = "Healthy",
                Message = "API 运行正常",
                Timestamp = DateTime.Now
            });
        }

        [HttpGet("organizations-test")]
        public async Task<IActionResult> TestOrganizationsQuery()
        {
            try
            {
                // 直接查询前5个组织，不使用COUNT
                var orgs = await _context.Organisations
                    .OrderBy(o => o.Name)
                    .Take(5)
                    .Select(o => new { o.Id, o.Name, o.ContactPerson })
                    .ToListAsync();
                
                return Ok(new
                {
                    Status = "Success",
                    Message = "组织查询测试成功",
                    Count = orgs.Count,
                    Organizations = orgs,
                    Timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Status = "Error",
                    Message = "组织查询测试失败",
                    Error = ex.Message,
                    InnerException = ex.InnerException?.Message,
                    Timestamp = DateTime.Now
                });
            }
        }
    }
}
