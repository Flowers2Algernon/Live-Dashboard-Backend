// LERD.Domain/Models/ResponseChartModels.cs
namespace LERD.Domain.Models;

public class ResponseChartData
{
    public int TotalParticipants { get; set; }
    public string ResponseRate { get; set; } = "23%";
    public bool ShowRegions { get; set; }
    public List<RegionData> Regions { get; set; } = new();
}

public class RegionData
{
    public string VillageName { get; set; } = string.Empty;
    public int ParticipantCount { get; set; }
}

public class ChartFilters
{
    public string? Gender { get; set; }
    public string? ParticipantType { get; set; }
    public string? Period { get; set; }
    
    /// <summary>
    /// Advanced period filter with support for multiple time formats
    /// </summary>
    public PeriodFilter PeriodFilter => new PeriodFilter { Period = Period };
}