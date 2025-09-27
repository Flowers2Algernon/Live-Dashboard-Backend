// LERD_Backend/Controllers/PeriodFilterTestController.cs
using LERD.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace LERD_Backend.Controllers;

[ApiController]
[Route("api/test")]
public class PeriodFilterTestController : ControllerBase
{
    [HttpGet("period-filter")]
    public IActionResult TestPeriodFilter([FromQuery] string? period = null)
    {
        var periodFilter = new PeriodFilter { Period = period };
        var parseResult = periodFilter.Parse();
        
        return Ok(new
        {
            Input = period,
            ParseSuccess = parseResult,
            Type = periodFilter.Type.ToString(),
            Year = periodFilter.Year,
            Months = periodFilter.Months,
            StartDate = periodFilter.StartDate?.ToString("yyyy-MM-dd"),
            EndDate = periodFilter.EndDate?.ToString("yyyy-MM-dd"),
            Description = periodFilter.GetDescription(),
            SqlWhereClause = periodFilter.BuildWhereClause(),
            TestCases = new
            {
                Message = "Test these URLs:",
                Examples = new[]
                {
                    "/api/test/period-filter?period=2025",
                    "/api/test/period-filter?period=2025-07",
                    "/api/test/period-filter?period=2025-07,2025-08",
                    "/api/test/period-filter?period=2025-01,2025-12",
                    "/api/test/period-filter",
                    "/api/test/period-filter?period=invalid"
                }
            }
        });
    }
}
