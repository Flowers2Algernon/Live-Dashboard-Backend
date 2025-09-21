// LERD.Shared/DTOs/ResponseChartDTOs.cs
namespace LERD.Shared.DTOs;

public class ResponseChartDto
{
    public int TotalParticipants { get; set; }
    public string ResponseRate { get; set; }
    public bool ShowRegions { get; set; }
    public List<RegionDto> Regions { get; set; } = new();
}

public class RegionDto
{
    public string VillageName { get; set; }
    public int ParticipantCount { get; set; }
}