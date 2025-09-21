// LERD.Application/Services/ResponseChartService.cs
using LERD.Application.Interfaces;
using LERD.Domain.Models;
using LERD.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using Microsoft.Extensions.Logging;

namespace LERD.Application.Services;

public class ResponseChartService : BaseChartService, IResponseChartService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ResponseChartService> _logger;
    private readonly Dictionary<string, string> _facilityMapping = new()
    {
        {"3001", "Bull Creek"},
        {"3002", "Coolbellup"},
        {"3003", "Mosman Park"},
        {"3004", "RoleyStone"},
        {"3005", "South Perth"},
        {"3008", "Duncraig"}
    };

    public ResponseChartService(ApplicationDbContext context, ILogger<ResponseChartService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ResponseChartData> GetResponseChartDataAsync(Guid surveyId, ChartFilters filters)
    {
        var baseCTE = BuildBaseResponseCTE(filters);
        
        var sql = $@"
            {baseCTE}
            SELECT 
                (SELECT COUNT(*) FROM response_records) as total_participants,
                facility_code,
                COUNT(*) as participant_count
            FROM response_records
            WHERE facility_code IS NOT NULL
            GROUP BY facility_code
            ORDER BY facility_code;";

        using var connection = new NpgsqlConnection(_context.Database.GetConnectionString());
        await connection.OpenAsync();

        using var command = new NpgsqlCommand(sql, connection);
        AddSurveyIdParameter(command, surveyId);
        AddFilterParameters(command, filters);

        var regions = new List<RegionData>();
        var totalParticipants = 0;

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            if (totalParticipants == 0)
            {
                totalParticipants = reader.GetInt32(reader.GetOrdinal("total_participants"));
            }
            
            var facilityCode = reader.GetString(reader.GetOrdinal("facility_code"));
            var participantCount = reader.GetInt32(reader.GetOrdinal("participant_count"));

            _logger.LogInformation("Found facility: {FacilityCode}, participants: {ParticipantCount}", facilityCode, participantCount);

            regions.Add(new RegionData
            {
                VillageName = _facilityMapping.GetValueOrDefault(facilityCode, facilityCode),
                ParticipantCount = participantCount
            });
        }

        // 应用文档中的特殊逻辑 - 始终显示地区数据当有数据时
        var shouldShowRegions = regions.Count > 0;
        // var shouldShowRegions = totalParticipants > 1 && totalParticipants < 5; // 原始逻辑

        _logger.LogInformation("Total regions found: {RegionCount}, shouldShowRegions: {ShouldShowRegions}", regions.Count, shouldShowRegions);

        return new ResponseChartData
        {
            TotalParticipants = totalParticipants,
            ResponseRate = "23%", // Stage 1固定值
            ShowRegions = shouldShowRegions,
            Regions = shouldShowRegions ? regions : new List<RegionData>()
        };
    }
}