// LERD.Application/Services/BaseChartService.cs
using LERD.Domain.Models;
using Npgsql;
using NpgsqlTypes;

namespace LERD.Application.Services;

public abstract class BaseChartService
{
    /// <summary>
    /// Builds common filter conditions for survey response data
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
            conditions.Add("(@gender IS NULL OR sr.response_data->>'Gender' = @gender)");

        if (!string.IsNullOrEmpty(filters.ParticipantType))
            conditions.Add("(@participantType IS NULL OR sr.response_data->>'ParticipantType' = @participantType)");

        if (!string.IsNullOrEmpty(filters.Period))
        {
            // Support period formats like "2025-07" or "2025"
            conditions.Add("(@period IS NULL OR sr.response_data->>'EndDate' LIKE @period)");
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
    /// </summary>
    /// <param name="command">The command to add parameters to</param>
    /// <param name="filters">The filters containing parameter values</param>
    protected void AddFilterParameters(NpgsqlCommand command, ChartFilters filters)
    {
        command.Parameters.Add(new NpgsqlParameter("gender", NpgsqlDbType.Text) 
            { Value = (object?)filters.Gender ?? DBNull.Value });
        command.Parameters.Add(new NpgsqlParameter("participantType", NpgsqlDbType.Text) 
            { Value = (object?)filters.ParticipantType ?? DBNull.Value });
        
        if (!string.IsNullOrEmpty(filters.Period))
        {
            command.Parameters.Add(new NpgsqlParameter("period", NpgsqlDbType.Text) 
                { Value = $"{filters.Period}%" });
        }
        else
        {
            command.Parameters.Add(new NpgsqlParameter("period", NpgsqlDbType.Text) 
                { Value = DBNull.Value });
        }
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
