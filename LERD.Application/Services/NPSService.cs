// LERD.Application/Services/NPSService.cs
using LERD.Application.Interfaces;
using LERD.Domain.Models;
using LERD.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace LERD.Application.Services;

public class NPSService : BaseChartService, INPSService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<NPSService> _logger;

    public NPSService(ApplicationDbContext context, ILogger<NPSService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<NPSData> GetNPSDataAsync(Guid surveyId, ChartFilters filters)
    {
        var filterConditions = BuildFilterConditions(filters);
        
        var sql = $@"
            WITH nps_data AS (
              SELECT 
                response_element->>'NPS_NPS_GROUP' as nps_group
              FROM survey_responses sr,
                   jsonb_array_elements(sr.response_data) as response_element
              WHERE sr.survey_id = @surveyId
                AND response_element->>'NPS_NPS_GROUP' IS NOT NULL
                AND {filterConditions}
            ),
            distribution AS (
              SELECT 
                COUNT(CASE WHEN nps_group = '3' THEN 1 END) as promoter_count,
                COUNT(CASE WHEN nps_group = '2' THEN 1 END) as passive_count,
                COUNT(CASE WHEN nps_group = '1' THEN 1 END) as detractor_count,
                COUNT(*) as total_count
              FROM nps_data
            )
            SELECT 
              promoter_count,
              passive_count,
              detractor_count,
              total_count,
              COALESCE(
                ROUND(
                  (promoter_count - detractor_count) * 100.0 / NULLIF(total_count, 0), 0
                ), 0
              ) as nps_score,
              COALESCE(
                ROUND(promoter_count * 100.0 / NULLIF(total_count, 0), 1), 0
              ) as promoter_percentage,
              COALESCE(
                ROUND(passive_count * 100.0 / NULLIF(total_count, 0), 1), 0
              ) as passive_percentage,
              COALESCE(
                ROUND(detractor_count * 100.0 / NULLIF(total_count, 0), 1), 0
              ) as detractor_percentage
            FROM distribution;";

        using var connection = new NpgsqlConnection(_context.Database.GetConnectionString());
        await connection.OpenAsync();

        using var command = new NpgsqlCommand(sql, connection);
        AddSurveyIdParameter(command, surveyId);
        AddFilterParameters(command, filters);

        _logger.LogInformation("Executing NPS query for survey {SurveyId} with filters: Gender={Gender}, ParticipantType={ParticipantType}", 
            surveyId, filters.Gender, filters.ParticipantType);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var result = new NPSData
            {
                NPSScore = reader.GetInt32(reader.GetOrdinal("nps_score")),
                Distribution = new NPSDistribution
                {
                    PromoterCount = reader.GetInt32(reader.GetOrdinal("promoter_count")),
                    PassiveCount = reader.GetInt32(reader.GetOrdinal("passive_count")),
                    DetractorCount = reader.GetInt32(reader.GetOrdinal("detractor_count")),
                    TotalCount = reader.GetInt32(reader.GetOrdinal("total_count")),
                    PromoterPercentage = reader.GetDecimal(reader.GetOrdinal("promoter_percentage")),
                    PassivePercentage = reader.GetDecimal(reader.GetOrdinal("passive_percentage")),
                    DetractorPercentage = reader.GetDecimal(reader.GetOrdinal("detractor_percentage"))
                }
            };

            _logger.LogInformation("NPS data retrieved: Score={NPSScore}, Promoters={PromoterCount}({PromoterPercentage}%), Passive={PassiveCount}({PassivePercentage}%), Detractors={DetractorCount}({DetractorPercentage}%), Total={TotalCount}", 
                result.NPSScore, result.Distribution.PromoterCount, result.Distribution.PromoterPercentage,
                result.Distribution.PassiveCount, result.Distribution.PassivePercentage,
                result.Distribution.DetractorCount, result.Distribution.DetractorPercentage,
                result.Distribution.TotalCount);

            return result;
        }

        _logger.LogWarning("No NPS data found for survey {SurveyId}", surveyId);
        return new NPSData();
    }
}
