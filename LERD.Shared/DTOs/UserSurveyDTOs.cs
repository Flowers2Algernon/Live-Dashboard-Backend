// LERD.Shared/DTOs/UserSurveyDTOs.cs
namespace LERD.Shared.DTOs
{
    public class UserSurveyDto
    {
        public Guid SurveyId { get; set; }
        public string SurveyName { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsDefault { get; set; }  // 如果有多个调查，标记默认的
    }

    public class UserSurveysResponse
    {
        public List<UserSurveyDto> Surveys { get; set; } = new();
        public UserSurveyDto? DefaultSurvey { get; set; }  // 便于前端直接使用
    }
}
