// LERD_Backend/Controllers/DateRangeTestController.cs
using LERD.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace LERD_Backend.Controllers;

[ApiController]
[Route("api/test")]
public class DateRangeTestController : ControllerBase
{
    [HttpGet("date-range-filter")]
    public IActionResult TestDateRangeFilter([FromQuery] string? period = null)
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
                Message = "Test these date range formats:",
                Examples = new[]
                {
                    "/api/test/date-range-filter?period=2024-05:2025-08",
                    "/api/test/date-range-filter?period=2025-01:2025-12", 
                    "/api/test/date-range-filter?period=2024-12:2025-01",
                    "/api/test/date-range-filter?period=2025-07",
                    "/api/test/date-range-filter?period=2025-07,2025-08",
                    "/api/test/date-range-filter?period=2025",
                    "/api/test/date-range-filter",
                    "/api/test/date-range-filter?period=invalid:format"
                }
            }
        });
    }
}
