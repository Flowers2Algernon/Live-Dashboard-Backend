using Microsoft.AspNetCore.Mvc;
using LERD.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace LERD_Backend.Controllers
{
    [ApiController]
    [Route("api/debug")]
    public class DataStructureDebugController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DataStructureDebugController> _logger;

        public DataStructureDebugController(ApplicationDbContext context, ILogger<DataStructureDebugController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Linus式调试：先看看response_data的实际结构，别瞎猜
        /// </summary>
        [HttpGet("check-response-data-structure/{surveyId}")]
        public async Task<IActionResult> CheckResponseDataStructure(Guid surveyId)
        {
            try
            {
                var connectionString = _context.Database.GetConnectionString();
                using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();

                // 1. 检查response_data的实际结构
                var structureSql = @"
                    SELECT 
                        jsonb_typeof(response_data) as data_type,
                        jsonb_array_length(CASE WHEN jsonb_typeof(response_data) = 'array' THEN response_data ELSE NULL END) as array_length,
                        jsonb_object_keys(CASE WHEN jsonb_typeof(response_data) = 'object' THEN response_data ELSE NULL END) as object_keys
                    FROM survey_responses 
                    WHERE survey_id = @surveyId 
                    LIMIT 1;";

                var dataStructure = new List<object>();
                using (var cmd = new NpgsqlCommand(structureSql, connection))
                {
                    cmd.Parameters.AddWithValue("surveyId", surveyId);
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        dataStructure.Add(new
                        {
                            dataType = reader.IsDBNull(0) ? null : reader.GetString(0),
                            arrayLength = reader.IsDBNull(1) ? (int?)null : reader.GetInt32(1),
                            objectKey = reader.IsDBNull(2) ? null : reader.GetString(2)
                        });
                    }
                }

                // 2. 如果是对象，获取所有顶级键
                var keysSql = @"
                    SELECT DISTINCT jsonb_object_keys(response_data) as keys
                    FROM survey_responses 
                    WHERE survey_id = @surveyId
                    ORDER BY keys;";

                var allKeys = new List<string>();
                using (var cmd = new NpgsqlCommand(keysSql, connection))
                {
                    cmd.Parameters.AddWithValue("surveyId", surveyId);
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        allKeys.Add(reader.GetString(0));
                    }
                }

                // 3. 检查是否有Satisfaction字段（直接在对象中）
                var satisfactionCheckSql = @"
                    SELECT 
                        COUNT(*) as total_records,
                        COUNT(CASE WHEN response_data->>'Satisfaction' IS NOT NULL THEN 1 END) as has_satisfaction,
                        COUNT(CASE WHEN response_data->>'Gender' IS NOT NULL THEN 1 END) as has_gender,
                        COUNT(CASE WHEN response_data->>'ParticipantType' IS NOT NULL THEN 1 END) as has_participant_type
                    FROM survey_responses 
                    WHERE survey_id = @surveyId;";

                object? fieldCheck = null;
                using (var cmd = new NpgsqlCommand(satisfactionCheckSql, connection))
                {
                    cmd.Parameters.AddWithValue("surveyId", surveyId);
                    using var reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        fieldCheck = new
                        {
                            totalRecords = reader.GetInt32(0),
                            hasSatisfaction = reader.GetInt32(1),
                            hasGender = reader.GetInt32(2),
                            hasParticipantType = reader.GetInt32(3)
                        };
                    }
                }

                // 4. 获取一个实际的response_data示例
                var sampleSql = @"
                    SELECT response_data
                    FROM survey_responses 
                    WHERE survey_id = @surveyId 
                    LIMIT 1;";

                string? sampleData = null;
                using (var cmd = new NpgsqlCommand(sampleSql, connection))
                {
                    cmd.Parameters.AddWithValue("surveyId", surveyId);
                    using var reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        sampleData = reader.GetString(0);
                    }
                }

                return Ok(new
                {
                    success = true,
                    surveyId = surveyId,
                    dataStructure = dataStructure,
                    allTopLevelKeys = allKeys,
                    fieldAvailability = fieldCheck,
                    sampleData = sampleData,
                    message = "Linus式调试：现在我们知道数据结构了"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking response data structure for survey {SurveyId}", surveyId);
                return StatusCode(500, new
                {
                    success = false,
                    message = $"调试失败: {ex.Message}",
                    exception = ex.GetType().Name
                });
            }
        }

        /// <summary>
        /// 修正后的satisfaction数据检查 - 基于实际数据结构
        /// </summary>
        [HttpGet("check-satisfaction-corrected/{surveyId}")]
        public async Task<IActionResult> CheckSatisfactionCorrected(Guid surveyId)
        {
            try
            {
                var connectionString = _context.Database.GetConnectionString();
                using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();

                // 假设response_data是对象而不是数组，直接访问字段
                var satisfactionSql = @"
                    SELECT 
                        response_data->>'Satisfaction' as satisfaction_value,
                        COUNT(*) as count
                    FROM survey_responses sr
                    WHERE sr.survey_id = @surveyId
                      AND response_data->>'Satisfaction' IS NOT NULL
                    GROUP BY response_data->>'Satisfaction'
                    ORDER BY satisfaction_value;";

                var distribution = new List<object>();
                using (var cmd = new NpgsqlCommand(satisfactionSql, connection))
                {
                    cmd.Parameters.AddWithValue("surveyId", surveyId);
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        distribution.Add(new
                        {
                            satisfaction = reader.GetString(0),
                            count = reader.GetInt32(1)
                        });
                    }
                }

                // 计算百分比
                var calculationSql = @"
                    WITH satisfaction_data AS (
                        SELECT response_data->>'Satisfaction' as satisfaction_code
                        FROM survey_responses sr
                        WHERE sr.survey_id = @surveyId
                          AND response_data->>'Satisfaction' IS NOT NULL
                    )
                    SELECT 
                        COUNT(*) as total,
                        COUNT(CASE WHEN satisfaction_code = '6' THEN 1 END) as very_satisfied,
                        COUNT(CASE WHEN satisfaction_code = '5' THEN 1 END) as satisfied,
                        COUNT(CASE WHEN satisfaction_code = '4' THEN 1 END) as somewhat_satisfied,
                        COUNT(CASE WHEN satisfaction_code IN ('4','5','6') THEN 1 END) as total_satisfied
                    FROM satisfaction_data;";

                object? calculation = null;
                using (var cmd = new NpgsqlCommand(calculationSql, connection))
                {
                    cmd.Parameters.AddWithValue("surveyId", surveyId);
                    using var reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        var total = reader.GetInt32(0);
                        var verySat = reader.GetInt32(1);
                        var sat = reader.GetInt32(2);
                        var somewhatSat = reader.GetInt32(3);
                        var totalSat = reader.GetInt32(4);

                        calculation = new
                        {
                            totalRecords = total,
                            verySatisfied = verySat,
                            satisfied = sat,
                            somewhatSatisfied = somewhatSat,
                            totalSatisfied = totalSat,
                            percentages = total > 0 ? new
                            {
                                verySatisfiedPct = Math.Round((double)verySat / total * 100, 1),
                                satisfiedPct = Math.Round((double)sat / total * 100, 1),
                                somewhatSatisfiedPct = Math.Round((double)somewhatSat / total * 100, 1),
                                totalSatisfiedPct = Math.Round((double)totalSat / total * 100, 1)
                            } : null
                        };
                    }
                }

                return Ok(new
                {
                    success = true,
                    surveyId = surveyId,
                    satisfactionDistribution = distribution,
                    calculationResult = calculation,
                    message = "修正后的satisfaction计算 - response_data是对象，不是数组"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in corrected satisfaction check for survey {SurveyId}", surveyId);
                return StatusCode(500, new
                {
                    success = false,
                    message = $"修正后的调试也失败了: {ex.Message}",
                    exception = ex.GetType().Name
                });
            }
        }
    }
}
