using LERD_Backend.Models;

namespace LERD_Backend.Services
{
    public interface ILoginService
    {
        Task<LoginResponse> ValidateLoginAsync(LoginRequest request);
    }

    public class LoginService : ILoginService
    {
        // hardcoded user data for demonstration purposes
        private readonly Dictionary<string, string> _users = new()
        {
            { "admin", "admin123" },
            { "user", "user123" },
            { "teacher", "teacher123" },
            { "student", "student123" }
        };

        public async Task<LoginResponse> ValidateLoginAsync(LoginRequest request)
        {
            // 模拟异步操作
            await Task.Delay(100);

            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "user name or password cannot be empty"
                };
            }

            if (_users.TryGetValue(request.Username, out var storedPassword) && 
                storedPassword == request.Password)
            {
                return new LoginResponse
                {
                    Success = true,
                    Message = "login successful",
                    Username = request.Username
                };
            }

            return new LoginResponse
            {
                Success = false,
                Message = "user name or password is incorrect"
            };
        }
    }
}
