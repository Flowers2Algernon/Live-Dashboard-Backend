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
            "response_element->>'Satisfaction' IS NOT NULL",
            "response_element->>'NPS_NPS_GROUP' IS NOT NULL"
        };

        if (!string.IsNullOrEmpty(filters.Gender))
            conditions.Add("(@gender IS NULL OR response_element->>'Gender' = @gender)");

        if (!string.IsNullOrEmpty(filters.ParticipantType))
            conditions.Add("(@participantType IS NULL OR response_element->>'ParticipantType' = @participantType)");

        if (!string.IsNullOrEmpty(filters.Period))
        {
            // Support period formats like "2025-07" or "2025"
            conditions.Add("(@period IS NULL OR response_element->>'EndDate' LIKE @period)");
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
                    response_element->>'Facility' as facility_code,
                    response_element->>'Gender' as gender,
                    response_element->>'ParticipantType' as participant_type,
                    response_element->>'EndDate' as end_date";

        var allFields = string.IsNullOrEmpty(additionalFields) 
            ? baseFields 
            : $"{baseFields},{additionalFields}";

        return $@"
            WITH response_records AS (
                SELECT {allFields}
                FROM survey_responses sr,
                     jsonb_array_elements(sr.response_data) as response_element
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
