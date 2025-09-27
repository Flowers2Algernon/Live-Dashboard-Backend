// LERD.Application/Services/BaseChartService.cs
using LERD.Domain.Models;
using Npgsql;
using NpgsqlTypes;

namespace LERD.Application.Services;

public abstract class BaseChartService
{
    /// <summary>
    /// Builds common filter conditions for survey response data with advanced period filtering
    /// </summary>
    /// <param name="filters">Chart filters to apply</param>
    /// <returns>SQL WHERE conditions string</returns>
    protected string BuildFilterConditions(ChartFilters filters)
    {
        var conditions = new List<string>
        {
            "sr.response_data->>'Satisfaction' IS NOT NULL",
            "sr.response_data->>'NPS_NPS_GROUP' IS NOT NULL"
        };

        if (!string.IsNullOrEmpty(filters.Gender))
            conditions.Add("sr.response_data->>'Gender' = @gender");

        if (!string.IsNullOrEmpty(filters.ParticipantType))
            conditions.Add("sr.response_data->>'ParticipantType' = @participantType");

        // Advanced period filtering with multiple format support
        var periodFilter = filters.PeriodFilter;
        var periodCondition = periodFilter.BuildWhereClause();
        if (periodCondition != "1=1") // Only add if there's actual filtering
        {
            conditions.Add(periodCondition);
        }

        return string.Join(" AND ", conditions);
    }

    /// <summary>
    /// Builds base CTE (Common Table Expression) for filtered survey responses
    /// </summary>
    /// <param name="filters">Chart filters to apply</param>
    /// <param name="additionalFields">Additional fields to select in the CTE</param>
    /// <returns>SQL CTE string</returns>
    protected string BuildBaseResponseCTE(ChartFilters filters, string additionalFields = "")
    {
        var filterConditions = BuildFilterConditions(filters);
        
        var baseFields = @"
                    sr.response_data->>'Facility' as facility_code,
                    sr.response_data->>'Gender' as gender,
                    sr.response_data->>'ParticipantType' as participant_type,
                    sr.response_data->>'EndDate' as end_date";

        var allFields = string.IsNullOrEmpty(additionalFields) 
            ? baseFields 
            : $"{baseFields},{additionalFields}";

        return $@"
            WITH response_records AS (
                SELECT {allFields}
                FROM survey_responses sr
                WHERE sr.survey_id = @surveyId
                  AND {filterConditions}
            )";
    }

    /// <summary>
    /// Adds standard filter parameters to a Npgsql command
    /// Enhanced to support advanced period filtering while maintaining backward compatibility
    /// </summary>
    /// <param name="command">The command to add parameters to</param>
    /// <param name="filters">The filters containing parameter values</param>
    protected void AddFilterParameters(NpgsqlCommand command, ChartFilters filters)
    {
        // Add gender parameter only if specified
        if (!string.IsNullOrEmpty(filters.Gender))
        {
            command.Parameters.Add(new NpgsqlParameter("gender", NpgsqlDbType.Text) 
                { Value = filters.Gender });
        }
        
        // Add participant type parameter only if specified
        if (!string.IsNullOrEmpty(filters.ParticipantType))
        {
            command.Parameters.Add(new NpgsqlParameter("participantType", NpgsqlDbType.Text) 
                { Value = filters.ParticipantType });
        }
        
        // Note: Period filtering is now handled directly in BuildFilterConditions()
        // using PeriodFilter.BuildWhereClause() for more advanced filtering logic
    }

    /// <summary>
    /// Adds survey ID parameter to a Npgsql command
    /// </summary>
    /// <param name="command">The command to add the parameter to</param>
    /// <param name="surveyId">The survey ID</param>
    protected void AddSurveyIdParameter(NpgsqlCommand command, Guid surveyId)
    {
        command.Parameters.Add(new NpgsqlParameter("surveyId", NpgsqlDbType.Uuid) 
            { Value = surveyId });
    }
}
