// LERD.Application/Services/CustomerSatisfactionService.cs
using LERD.Application.Interfaces;
using LERD.Domain.Models;
using LERD.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace LERD.Application.Services;

public class CustomerSatisfactionService : BaseChartService, ICustomerSatisfactionService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CustomerSatisfactionService> _logger;

    public CustomerSatisfactionService(ApplicationDbContext context, ILogger<CustomerSatisfactionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CustomerSatisfactionData> GetSatisfactionDataAsync(Guid surveyId, ChartFilters filters)
    {
        var baseCTE = BuildBaseResponseCTE(filters, @"
                    sr.response_data->>'Satisfaction' as satisfaction_code");
        
        var sql = $@"
            {baseCTE},
            total_count AS (
              SELECT COUNT(*) as total FROM response_records
            )
            SELECT 
              COALESCE(
                ROUND(
                  COUNT(CASE WHEN satisfaction_code = '6' THEN 1 END) * 100.0 / NULLIF(tc.total, 0), 1
                ), 0
              ) as very_satisfied_percentage,
              
              COALESCE(
                ROUND(
                  COUNT(CASE WHEN satisfaction_code = '5' THEN 1 END) * 100.0 / NULLIF(tc.total, 0), 1
                ), 0
              ) as satisfied_percentage,
              
              COALESCE(
                ROUND(
                  COUNT(CASE WHEN satisfaction_code = '4' THEN 1 END) * 100.0 / NULLIF(tc.total, 0), 1
                ), 0
              ) as somewhat_satisfied_percentage,
              
              COALESCE(
                ROUND(
                  COUNT(CASE WHEN satisfaction_code IN ('4','5','6') THEN 1 END) * 100.0 / NULLIF(tc.total, 0), 1
                ), 0
              ) as total_satisfied_percentage

            FROM response_records, total_count tc
            GROUP BY tc.total;";

        using var connection = new NpgsqlConnection(_context.Database.GetConnectionString());
        await connection.OpenAsync();

        using var command = new NpgsqlCommand(sql, connection);
        AddSurveyIdParameter(command, surveyId);
        AddFilterParameters(command, filters);

        _logger.LogInformation("Executing customer satisfaction query for survey {SurveyId} with filters: Gender={Gender}, ParticipantType={ParticipantType}, Period={Period}", 
            surveyId, filters.Gender, filters.ParticipantType, filters.Period);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var result = new CustomerSatisfactionData
            {
                VerySatisfiedPercentage = reader.GetDecimal(reader.GetOrdinal("very_satisfied_percentage")),
                SatisfiedPercentage = reader.GetDecimal(reader.GetOrdinal("satisfied_percentage")),
                SomewhatSatisfiedPercentage = reader.GetDecimal(reader.GetOrdinal("somewhat_satisfied_percentage")),
                TotalSatisfiedPercentage = reader.GetDecimal(reader.GetOrdinal("total_satisfied_percentage"))
            };

            _logger.LogInformation("Customer satisfaction data retrieved: VerySatisfied={VerySatisfied}%, Satisfied={Satisfied}%, SomewhatSatisfied={SomewhatSatisfied}%, TotalSatisfied={TotalSatisfied}%", 
                result.VerySatisfiedPercentage, result.SatisfiedPercentage, result.SomewhatSatisfiedPercentage, result.TotalSatisfiedPercentage);

            return result;
        }

        _logger.LogWarning("No customer satisfaction data found for survey {SurveyId}", surveyId);
        return new CustomerSatisfactionData();
    }
}
