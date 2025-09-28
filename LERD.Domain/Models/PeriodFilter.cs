// LERD.Domain/Models/PeriodFilter.cs
using System.Globalization;

namespace LERD.Domain.Models;

/// <summary>
/// Advanced period filter that supports multiple time formats with backward compatibility
/// </summary>
public class PeriodFilter
{
    public string? Period { get; set; }
    
    // Parsed structured data
    public int? Year { get; private set; }
    public List<int> Months { get; private set; } = new List<int>();
    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public PeriodFilterType Type { get; private set; }
    
    public enum PeriodFilterType
    {
        None,           // No filter
        Year,           // "2025" - entire year
        Months,         // "2025-07" or "2025-07,2025-08" - specific months
        DateRange       // Future: "2025-07-01:2025-09-30" - date range
    }
    
    /// <summary>
    /// Parse the period string into structured filter data
    /// Supports backward compatibility with existing formats + new date range format
    /// </summary>
    /// <returns>True if parsing was successful or no period specified</returns>
    public bool Parse()
    {
        if (string.IsNullOrEmpty(Period))
        {
            Type = PeriodFilterType.None;
            return true;
        }
            
        try
        {
            // Format 1: Year only "2025" (backward compatible)
            if (int.TryParse(Period.Trim(), out int year) && year >= 2020 && year <= 2030)
            {
                Year = year;
                Type = PeriodFilterType.Year;
                return true;
            }
            
            // Format 2: Date range "2024-05:2025-08" (NEW - supports cross-year ranges)
            if (Period.Contains(':'))
            {
                var rangeParts = Period.Split(':', StringSplitOptions.RemoveEmptyEntries);
                if (rangeParts.Length == 2)
                {
                    var startPart = rangeParts[0].Trim();
                    var endPart = rangeParts[1].Trim();
                    
                    // Try to parse start and end as YYYY-MM format
                    if (DateTime.TryParseExact(startPart + "-01", "yyyy-MM-dd", 
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate) &&
                        DateTime.TryParseExact(endPart + "-01", "yyyy-MM-dd", 
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDate))
                    {
                        // Validate that start is before or equal to end
                        if (startDate <= endDate)
                        {
                            StartDate = new DateTime(startDate.Year, startDate.Month, 1);
                            EndDate = new DateTime(endDate.Year, endDate.Month, DateTime.DaysInMonth(endDate.Year, endDate.Month));
                            Type = PeriodFilterType.DateRange;
                            return true;
                        }
                    }
                }
                return false; // Invalid range format
            }
            
            // Format 3: Month list "2025-07,2025-08" or single month "2025-07" (same year only)
            var monthParts = Period.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var months = new List<int>();
            int? commonYear = null;
            
            foreach (var monthPart in monthParts)
            {
                var trimmedPart = monthPart.Trim();
                
                // Try to parse as YYYY-MM format
                if (DateTime.TryParseExact(trimmedPart + "-01", "yyyy-MM-dd", 
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                {
                    if (commonYear == null) 
                        commonYear = date.Year;
                    else if (date.Year != commonYear)
                        return false; // Don't allow cross-year selection in comma format
                    
                    if (!months.Contains(date.Month))
                        months.Add(date.Month);
                }
                else
                {
                    return false; // Invalid format
                }
            }
            
            if (months.Any() && commonYear.HasValue)
            {
                Year = commonYear.Value;
                Months = months.OrderBy(m => m).ToList();
                Type = PeriodFilterType.Months;
                
                // Calculate date range for the selected months
                var firstMonth = months.Min();
                var lastMonth = months.Max();
                
                StartDate = new DateTime(Year.Value, firstMonth, 1);
                EndDate = new DateTime(Year.Value, lastMonth, DateTime.DaysInMonth(Year.Value, lastMonth));
                
                return true;
            }
            
            // If we get here, the format is not recognized
            Type = PeriodFilterType.None;
            return true; // Gracefully degrade to "no filter"
        }
        catch
        {
            Type = PeriodFilterType.None;
            return true; // Gracefully degrade to "no filter"
        }
    }
    
    /// <summary>
    /// Build SQL WHERE clause for period filtering based on period_year and period_month database fields
    /// Fixed to use actual database schema: period_year (INTEGER) + period_month (SMALLINT 1-12)
    /// </summary>
    /// <returns>SQL condition string</returns>
    public string BuildWhereClause()
    {
        if (!Parse() || Type == PeriodFilterType.None)
            return "1=1"; // No filtering
            
        return Type switch
        {
            // 年份筛选：使用period_year字段
            PeriodFilterType.Year => 
                $"sr.period_year = {Year}",
                
            // 单月筛选：同时使用period_year和period_month字段
            PeriodFilterType.Months when Months.Count == 1 => 
                $"sr.period_year = {Year} AND sr.period_month = {Months[0]}",
                
            // 多月筛选：同年多个月份
            PeriodFilterType.Months when Months.Count > 1 =>
                $"sr.period_year = {Year} AND sr.period_month IN ({string.Join(",", Months)})",
                
            // 月份范围筛选：跨年支持
            PeriodFilterType.DateRange =>
                BuildDateRangeWhereClause(),
                
            _ => "1=1"
        };
    }

    /// <summary>
    /// Helper method to build WHERE clause for date ranges (supports cross-year ranges)
    /// </summary>
    private string BuildDateRangeWhereClause()
    {
        if (StartDate == null || EndDate == null)
            return "1=1";
            
        var startYear = StartDate.Value.Year;
        var startMonth = StartDate.Value.Month;
        var endYear = EndDate.Value.Year;
        var endMonth = EndDate.Value.Month;
        
        if (startYear == endYear)
        {
            // 同年范围：简单的month范围查询
            return $"sr.period_year = {startYear} AND sr.period_month BETWEEN {startMonth} AND {endMonth}";
        }
        else
        {
            // 跨年范围：需要更复杂的查询
            return $@"(
            (sr.period_year = {startYear} AND sr.period_month >= {startMonth}) OR
            (sr.period_year > {startYear} AND sr.period_year < {endYear}) OR
            (sr.period_year = {endYear} AND sr.period_month <= {endMonth})
        )";
        }
    }
    
    /// <summary>
    /// Get a human-readable description of the filter
    /// </summary>
    public string GetDescription()
    {
        if (!Parse() || Type == PeriodFilterType.None)
            return "All periods";
            
        return Type switch
        {
            PeriodFilterType.Year => $"Year {Year}",
            PeriodFilterType.Months when Months.Count == 1 => 
                $"{new DateTime(Year!.Value, Months[0], 1):MMMM yyyy}",
            PeriodFilterType.Months when Months.Count > 1 =>
                $"Months: {string.Join(", ", Months.Select(m => new DateTime(Year!.Value, m, 1).ToString("MMM")))} {Year}",
            PeriodFilterType.DateRange =>
                $"Range: {StartDate:MMM yyyy} to {EndDate:MMM yyyy}",
            _ => "All periods"
        };
    }
}
