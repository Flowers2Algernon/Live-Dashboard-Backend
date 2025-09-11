using LERD.Shared.DTOs;

namespace LERD.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> ValidateLoginAsync(LoginRequest request);
}