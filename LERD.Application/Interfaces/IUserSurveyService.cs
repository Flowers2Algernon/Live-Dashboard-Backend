// LERD.Application/Interfaces/IUserSurveyService.cs
using LERD.Shared.DTOs;

namespace LERD.Application.Interfaces
{
    public interface IUserSurveyService
    {
        Task<UserSurveysResponse> GetUserSurveysAsync(Guid userId);
        Task<UserSurveyDto?> GetDefaultSurveyForUserAsync(Guid userId);
    }
}
