// LERD.Application/Services/CustomerSatisfactionTrendService.cs
using LERD.Application.Interfaces;
using LERD.Domain.Models;
using LERD.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace LERD.Application.Services;

public class CustomerSatisfactionTrendService : BaseChartService, ICustomerSatisfactionTrendService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CustomerSatisfactionTrendService> _logger;

    public CustomerSatisfactionTrendService(ApplicationDbContext context, ILogger<CustomerSatisfactionTrendService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CustomerSatisfactionTrendData> GetTrendDataAsync(Guid surveyId, ChartFilters filters)
    {
        _logger.LogInformation("Getting trend data for survey {SurveyId} with filters: Gender={Gender}, ParticipantType={ParticipantType}, Period={Period}", 
            surveyId, filters.Gender, filters.ParticipantType, filters.Period);

        // Get real data from database (mainly 2025 and any other years in DB)
        var realData = await GetRealTrendDataAsync(surveyId, filters);
        
        // Combine static historical data with real data
        var result = new CustomerSatisfactionTrendData();
        
        // If period filter is specified, only include data for that period
        if (!string.IsNullOrEmpty(filters.Period))
        {
            // Try to parse the period as a year
            if (int.TryParse(filters.Period, out int requestedYear))
            {
                // Add static historical data only if it matches the requested year
                if (requestedYear == 2023)
                {
                    result.Years.Add(new YearlyTrendData
                    {
                        Year = 2023,
                        VerySatisfiedPercentage = 13,
                        SatisfiedPercentage = 32,
                        SomewhatSatisfiedPercentage = 38,
                        TotalSatisfiedPercentage = 83
                    });
                }
                else if (requestedYear == 2024)
                {
                    result.Years.Add(new YearlyTrendData
                    {
                        Year = 2024,
                        VerySatisfiedPercentage = 36,
                        SatisfiedPercentage = 40,
                        SomewhatSatisfiedPercentage = 22,
                        TotalSatisfiedPercentage = 98
                    });
                }
                
                // Add real data only for the requested year
                result.Years.AddRange(realData.Where(y => y.Year == requestedYear));
            }
            else
            {
                // If period is not a year (e.g., "2025-07"), only add real data from database
                result.Years.AddRange(realData);
            }
        }
        else
        {
            // No period filter - return all data (original behavior)
            result.Years.Add(new YearlyTrendData
            {
                Year = 2023,
                VerySatisfiedPercentage = 13,
                SatisfiedPercentage = 32,
                SomewhatSatisfiedPercentage = 38,
                TotalSatisfiedPercentage = 83
            });
            
            result.Years.Add(new YearlyTrendData
            {
                Year = 2024,
                VerySatisfiedPercentage = 36,
                SatisfiedPercentage = 40,
                SomewhatSatisfiedPercentage = 22,
                TotalSatisfiedPercentage = 98
            });
            
            result.Years.AddRange(realData);
        }
        
        // Sort by year to ensure proper chronological order
        result.Years = result.Years.OrderBy(y => y.Year).ToList();
        
        _logger.LogInformation("Retrieved trend data for {YearCount} years", result.Years.Count);
        
        return result;
    }

    private async Task<List<YearlyTrendData>> GetRealTrendDataAsync(Guid surveyId, ChartFilters filters)
    {
        var filterConditions = BuildFilterConditions(filters);
        
        var sql = $@"
            WITH yearly_satisfaction AS (
              SELECT 
                EXTRACT(YEAR FROM (response_element->>'EndDate')::timestamp) as year,
                response_element->>'Satisfaction' as satisfaction_code
              FROM survey_responses sr,
                   jsonb_array_elements(sr.response_data) as response_element
              WHERE sr.survey_id = @surveyId
                AND response_element->>'EndDate' IS NOT NULL
                AND {filterConditions}
            ),
            year_totals AS (
              SELECT 
                year,
                COUNT(*) as total_responses
              FROM yearly_satisfaction
              WHERE year IS NOT NULL
              GROUP BY year
            )
            SELECT 
              ys.year,
              COALESCE(
                ROUND(
                  COUNT(CASE WHEN ys.satisfaction_code = '6' THEN 1 END) * 100.0 / NULLIF(yt.total_responses, 0), 1
                ), 0
              ) as very_satisfied_percentage,
              COALESCE(
                ROUND(
                  COUNT(CASE WHEN ys.satisfaction_code = '5' THEN 1 END) * 100.0 / NULLIF(yt.total_responses, 0), 1
                ), 0
              ) as satisfied_percentage,
              COALESCE(
                ROUND(
                  COUNT(CASE WHEN ys.satisfaction_code = '4' THEN 1 END) * 100.0 / NULLIF(yt.total_responses, 0), 1
                ), 0
              ) as somewhat_satisfied_percentage,
              yt.total_responses
            FROM yearly_satisfaction ys
            JOIN year_totals yt ON ys.year = yt.year
            WHERE ys.year IS NOT NULL
            GROUP BY ys.year, yt.total_responses
            ORDER BY ys.year;";

        var result = new List<YearlyTrendData>();
        
        using var connection = new NpgsqlConnection(_context.Database.GetConnectionString());
        await connection.OpenAsync();

        using var command = new NpgsqlCommand(sql, connection);
        AddSurveyIdParameter(command, surveyId);
        AddFilterParameters(command, filters);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var year = Convert.ToInt32(reader.GetDecimal(reader.GetOrdinal("year")));
            var verySatisfied = reader.GetDecimal(reader.GetOrdinal("very_satisfied_percentage"));
            var satisfied = reader.GetDecimal(reader.GetOrdinal("satisfied_percentage"));
            var somewhatSatisfied = reader.GetDecimal(reader.GetOrdinal("somewhat_satisfied_percentage"));
            var totalResponses = reader.GetInt32(reader.GetOrdinal("total_responses"));

            var yearData = new YearlyTrendData
            {
                Year = year,
                VerySatisfiedPercentage = verySatisfied,
                SatisfiedPercentage = satisfied,
                SomewhatSatisfiedPercentage = somewhatSatisfied,
                TotalSatisfiedPercentage = verySatisfied + satisfied + somewhatSatisfied
            };
            
            _logger.LogInformation("Found data for year {Year}: VerySatisfied={VerySatisfied}%, Satisfied={Satisfied}%, SomewhatSatisfied={SomewhatSatisfied}%, Total responses={TotalResponses}", 
                year, verySatisfied, satisfied, somewhatSatisfied, totalResponses);
            
            result.Add(yearData);
        }

        return result;
    }
}
