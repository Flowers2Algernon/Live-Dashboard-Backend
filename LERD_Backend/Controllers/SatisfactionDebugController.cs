using Microsoft.AspNetCore.Mvc;
using LERD.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace LERD_Backend.Controllers
{
    [ApiController]
    [Route("api/debug")]
    public class SatisfactionDebugController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SatisfactionDebugController> _logger;

        public SatisfactionDebugController(ApplicationDbContext context, ILogger<SatisfactionDebugController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 检查satisfaction字段的实际数据分布 - Linus式：直接看数据，别瞎猜
        /// </summary>
        [HttpGet("satisfaction-data/{surveyId}")]
        public async Task<IActionResult> CheckSatisfactionData(Guid surveyId)
        {
            try
            {
                var connectionString = _context.Database.GetConnectionString();
                using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();

                // 1. 检查satisfaction字段的数据分布
                var distributionSql = @"
                    SELECT 
                        response_element->>'Satisfaction' as satisfaction_value,
                        COUNT(*) as count
                    FROM survey_responses sr,
                         jsonb_array_elements(sr.response_data) as response_element
                    WHERE sr.survey_id = @surveyId
                    GROUP BY response_element->>'Satisfaction'
                    ORDER BY satisfaction_value;";

                var distribution = new List<object>();
                using (var cmd = new NpgsqlCommand(distributionSql, connection))
                {
                    cmd.Parameters.AddWithValue("surveyId", surveyId);
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        distribution.Add(new
                        {
                            satisfaction = reader.IsDBNull(0) ? "NULL" : reader.GetString(0),
                            count = reader.GetInt32(1)
                        });
                    }
                }

                // 2. 检查NULL值情况
                var nullCheckSql = @"
                    SELECT 
                        COUNT(*) as total_responses,
                        COUNT(CASE WHEN response_element->>'Satisfaction' IS NOT NULL THEN 1 END) as non_null_satisfaction,
                        COUNT(CASE WHEN response_element->>'Satisfaction' IS NULL THEN 1 END) as null_satisfaction
                    FROM survey_responses sr,
                         jsonb_array_elements(sr.response_data) as response_element
                    WHERE sr.survey_id = @surveyId;";

                object nullStats = null;
                using (var cmd = new NpgsqlCommand(nullCheckSql, connection))
                {
                    cmd.Parameters.AddWithValue("surveyId", surveyId);
                    using var reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        nullStats = new
                        {
                            totalResponses = reader.GetInt32(0),
                            nonNullSatisfaction = reader.GetInt32(1),
                            nullSatisfaction = reader.GetInt32(2)
                        };
                    }
                }

                // 3. 测试实际的satisfaction计算
                var calculationSql = @"
                    WITH response_records AS (
                        SELECT response_element->>'Satisfaction' as satisfaction_code
                        FROM survey_responses sr,
                             jsonb_array_elements(sr.response_data) as response_element
                        WHERE sr.survey_id = @surveyId
                          AND response_element->>'Satisfaction' IS NOT NULL
                    ),
                    total_count AS (
                        SELECT COUNT(*) as total FROM response_records
                    )
                    SELECT 
                        tc.total,
                        COUNT(CASE WHEN satisfaction_code = '6' THEN 1 END) as very_satisfied,
                        COUNT(CASE WHEN satisfaction_code = '5' THEN 1 END) as satisfied,
                        COUNT(CASE WHEN satisfaction_code = '4' THEN 1 END) as somewhat_satisfied,
                        COUNT(CASE WHEN satisfaction_code IN ('4','5','6') THEN 1 END) as total_satisfied
                    FROM response_records, total_count tc
                    GROUP BY tc.total;";

                object calculationResult = null;
                using (var cmd = new NpgsqlCommand(calculationSql, connection))
                {
                    cmd.Parameters.AddWithValue("surveyId", surveyId);
                    using var reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        var total = reader.GetInt32(0);
                        calculationResult = new
                        {
                            totalRecords = total,
                            verySatisfied = reader.GetInt32(1),
                            satisfied = reader.GetInt32(2),
                            somewhatSatisfied = reader.GetInt32(3),
                            totalSatisfied = reader.GetInt32(4),
                            percentages = total > 0 ? new
                            {
                                verySatisfiedPct = Math.Round((double)reader.GetInt32(1) / total * 100, 1),
                                satisfiedPct = Math.Round((double)reader.GetInt32(2) / total * 100, 1),
                                somewhatSatisfiedPct = Math.Round((double)reader.GetInt32(3) / total * 100, 1),
                                totalSatisfiedPct = Math.Round((double)reader.GetInt32(4) / total * 100, 1)
                            } : null
                        };
                    }
                }

                return Ok(new
                {
                    success = true,
                    surveyId = surveyId,
                    satisfactionDistribution = distribution,
                    nullValueStats = nullStats,
                    calculationResult = calculationResult,
                    message = "Linus式调试：数据就在这里，看看哪里搞砸了"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in satisfaction data debug for survey {SurveyId}", surveyId);
                return StatusCode(500, new
                {
                    success = false,
                    message = $"调试失败: {ex.Message}",
                    exception = ex.GetType().Name
                });
            }
        }

        /// <summary>
        /// 测试BaseChartService的过滤逻辑是否搞砸了
        /// </summary>
        [HttpGet("test-filter-logic/{surveyId}")]
        public async Task<IActionResult> TestFilterLogic(Guid surveyId, string? gender = null, string? participantType = null)
        {
            try
            {
                var connectionString = _context.Database.GetConnectionString();
                using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();

                // 测试BaseChartService的过滤条件
                var filterTestSql = @"
                    WITH response_records AS (
                        SELECT 
                            response_element->>'Facility' as facility_code,
                            response_element->>'Gender' as gender,
                            response_element->>'ParticipantType' as participant_type,
                            response_element->>'EndDate' as end_date,
                            response_element->>'Satisfaction' as satisfaction_code
                        FROM survey_responses sr,
                             jsonb_array_elements(sr.response_data) as response_element
                        WHERE sr.survey_id = @surveyId
                          AND response_element->>'Satisfaction' IS NOT NULL
                          AND response_element->>'NPS_NPS_GROUP' IS NOT NULL
                    )
                    SELECT 
                        COUNT(*) as total_after_base_filter,
                        COUNT(CASE WHEN gender = @gender OR @gender IS NULL THEN 1 END) as after_gender_filter,
                        COUNT(CASE WHEN participant_type = @participantType OR @participantType IS NULL THEN 1 END) as after_participant_filter
                    FROM response_records;";

                using var cmd = new NpgsqlCommand(filterTestSql, connection);
                cmd.Parameters.AddWithValue("surveyId", surveyId);
                cmd.Parameters.AddWithValue("gender", (object?)gender ?? DBNull.Value);
                cmd.Parameters.AddWithValue("participantType", (object?)participantType ?? DBNull.Value);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return Ok(new
                    {
                        success = true,
                        surveyId = surveyId,
                        appliedFilters = new { gender, participantType },
                        results = new
                        {
                            totalAfterBaseFilter = reader.GetInt32(0),
                            afterGenderFilter = reader.GetInt32(1),
                            afterParticipantFilter = reader.GetInt32(2)
                        },
                        message = "检查过滤逻辑是否把所有记录都过滤掉了"
                    });
                }

                return Ok(new { success = false, message = "没有数据返回" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing filter logic");
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}
