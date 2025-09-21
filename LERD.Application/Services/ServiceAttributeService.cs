// LERD.Application/Services/ServiceAttributeService.cs
using LERD.Application.Interfaces;
using LERD.Domain.Models;
using LERD.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace LERD.Application.Services;

public class ServiceAttributeService : BaseChartService, IServiceAttributeService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ServiceAttributeService> _logger;

    // 属性名称映射
    private readonly Dictionary<string, string> _attributeMapping = new()
    {
        {"Safety", "Safety & Security"},
        {"Location", "Village Location Access"},
        {"Activities", "Activity Availability"},
        {"Facilities", "Facilities"},
        {"Garden care", "Garden Care"},
        {"Staff service", "Staff Service"}
    };

    public ServiceAttributeService(ApplicationDbContext context, ILogger<ServiceAttributeService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ServiceAttributeData> GetServiceAttributeDataAsync(Guid surveyId, ServiceAttributeFilters filters)
    {
        _logger.LogInformation("Getting service attribute data for survey {SurveyId} with filters: Gender={Gender}, ParticipantType={ParticipantType}, SelectedAttributes={SelectedAttributes}", 
            surveyId, filters.Gender, filters.ParticipantType, filters.SelectedAttributes != null ? string.Join(",", filters.SelectedAttributes) : "None");

        // 1. 首先获取所有可用的属性
        var availableAttributes = await GetAvailableAttributes(surveyId);
        
        _logger.LogInformation("Found {Count} available attributes: {Attributes}", 
            availableAttributes.Count, string.Join(", ", availableAttributes));

        // 2. 应用属性过滤（如果有选择特定属性）
        var targetAttributes = filters.SelectedAttributes?.Any() == true 
            ? filters.SelectedAttributes.Intersect(availableAttributes).ToList()
            : availableAttributes;

        _logger.LogInformation("Processing {Count} target attributes: {Attributes}", 
            targetAttributes.Count, string.Join(", ", targetAttributes));

        // 3. 获取属性统计数据
        var attributeStats = await GetAttributeStats(surveyId, filters, targetAttributes);

        var result = new ServiceAttributeData
        {
            Attributes = attributeStats,
            AvailableAttributes = availableAttributes
        };

        _logger.LogInformation("Successfully retrieved service attribute data with {AttributeCount} attributes", attributeStats.Count);

        return result;
    }

    private async Task<List<string>> GetAvailableAttributes(Guid surveyId)
    {
        var sql = @"
            WITH response_keys AS (
              SELECT DISTINCT 
                jsonb_object_keys(response_element) as key_name
              FROM survey_responses sr,
                   jsonb_array_elements(sr.response_data) as response_element
              WHERE sr.survey_id = @surveyId
            )
            SELECT REPLACE(key_name, 'Ab_', '') as attribute_name
            FROM response_keys
            WHERE key_name LIKE 'Ab_%'
            ORDER BY attribute_name;";

        var attributes = new List<string>();
        using var connection = new NpgsqlConnection(_context.Database.GetConnectionString());
        await connection.OpenAsync();

        using var command = new NpgsqlCommand(sql, connection);
        AddSurveyIdParameter(command, surveyId);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            attributes.Add(reader.GetString(reader.GetOrdinal("attribute_name")));
        }

        return attributes;
    }

    private async Task<List<AttributeItem>> GetAttributeStats(Guid surveyId, ServiceAttributeFilters filters, List<string> targetAttributes)
    {
        if (!targetAttributes.Any())
        {
            _logger.LogWarning("No target attributes specified, returning empty result");
            return new List<AttributeItem>();
        }

        var filterConditions = BuildFilterConditions(filters);
        
        // 构建动态查询，只包含目标属性
        var attributeSelections = targetAttributes.Select(attr => 
            $"SELECT '{attr}' as attribute_name, 'Ab_{attr}' as field_name").ToList();
        
        var sql = $@"
            WITH attribute_data AS (
              SELECT 
                response_element
              FROM survey_responses sr,
                   jsonb_array_elements(sr.response_data) as response_element
              WHERE sr.survey_id = @surveyId
                AND response_element->>'Satisfaction' IS NOT NULL
                AND {filterConditions}
            ),
            target_attributes AS (
              {string.Join(" UNION ALL ", attributeSelections)}
            ),
            attribute_stats AS (
              SELECT 
                ta.attribute_name,
                COUNT(ad.response_element) as total_responses,
                COUNT(CASE WHEN ad.response_element->>ta.field_name = '4' THEN 1 END) as always_count,
                COUNT(CASE WHEN ad.response_element->>ta.field_name = '3' THEN 1 END) as most_count,
                COUNT(CASE WHEN ad.response_element->>ta.field_name IS NOT NULL 
                           AND ad.response_element->>ta.field_name != '' 
                           AND ad.response_element->>ta.field_name IN ('1', '2', '3', '4') THEN 1 END) as valid_responses
              FROM target_attributes ta
              CROSS JOIN attribute_data ad
              GROUP BY ta.attribute_name
            )
            SELECT 
              attribute_name,
              total_responses,
              valid_responses,
              always_count,
              most_count,
              COALESCE(ROUND(always_count * 100.0 / NULLIF(valid_responses, 0), 1), 0) as always_percentage,
              COALESCE(ROUND(most_count * 100.0 / NULLIF(valid_responses, 0), 1), 0) as most_percentage
            FROM attribute_stats
            WHERE valid_responses > 0
            ORDER BY attribute_name;";

        var result = new List<AttributeItem>();
        using var connection = new NpgsqlConnection(_context.Database.GetConnectionString());
        await connection.OpenAsync();

        using var command = new NpgsqlCommand(sql, connection);
        AddSurveyIdParameter(command, surveyId);
        AddFilterParameters(command, filters);

        _logger.LogInformation("Executing service attribute query for {AttributeCount} attributes", targetAttributes.Count);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var attributeName = reader.GetString(reader.GetOrdinal("attribute_name"));
            var displayName = _attributeMapping.GetValueOrDefault(attributeName, attributeName);
            
            var item = new AttributeItem
            {
                AttributeName = displayName,
                TotalResponses = reader.GetInt32(reader.GetOrdinal("total_responses")),
                ValidResponses = reader.GetInt32(reader.GetOrdinal("valid_responses")),
                AlwaysCount = reader.GetInt32(reader.GetOrdinal("always_count")),
                MostCount = reader.GetInt32(reader.GetOrdinal("most_count")),
                AlwaysPercentage = reader.GetDecimal(reader.GetOrdinal("always_percentage")),
                MostPercentage = reader.GetDecimal(reader.GetOrdinal("most_percentage"))
            };

            _logger.LogDebug("Attribute '{AttributeName}': Always={AlwaysCount}({AlwaysPercentage}%), Most={MostCount}({MostPercentage}%), Valid={ValidResponses}", 
                displayName, item.AlwaysCount, item.AlwaysPercentage, item.MostCount, item.MostPercentage, item.ValidResponses);

            result.Add(item);
        }

        return result;
    }
}
