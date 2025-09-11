using LERD_Backend.Models;

namespace LERD_Backend.Services
{
    public interface ILoginService
    {
        Task<LoginResponse1> ValidateLoginAsync(LoginRequest1 request1);
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

        public async Task<LoginResponse1> ValidateLoginAsync(LoginRequest1 request1)
        {
            // 模拟异步操作
            await Task.Delay(100);

            if (string.IsNullOrEmpty(request1.Username) || string.IsNullOrEmpty(request1.Password))
            {
                return new LoginResponse1
                {
                    Success = false,
                    Message = "user name or password cannot be empty"
                };
            }

            if (_users.TryGetValue(request1.Username, out var storedPassword) && 
                storedPassword == request1.Password)
            {
                return new LoginResponse1
                {
                    Success = true,
                    Message = "login successful",
                    Username = request1.Username
                };
            }

            return new LoginResponse1
            {
                Success = false,
                Message = "user name or password is incorrect"
            };
        }
    }
}
