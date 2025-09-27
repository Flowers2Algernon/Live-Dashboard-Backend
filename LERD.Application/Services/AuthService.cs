using LERD.Application.Interfaces;
using LERD.Infrastructure.Data;
using LERD.Shared.DTOs;
using LERD.Utils;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;

namespace LERD.Application.Services;

public class AuthService(ApplicationDbContext context, JwtHelper jwtHelper) : IAuthService
{
    public async Task<LoginResponse> ValidateLoginAsync(LoginRequest request)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u =>
                u.Username == request.Username &&
                u.IsActive);

        if (user == null)
            return new LoginResponse { Success = false, Message = "The user does not exist or has been deactivated." };
        // No support password hash at Present
        // if (!VerifyPassword(request.Password, user.PasswordHash))
        if(user.PasswordHash != request.Password)
            return new LoginResponse { Success = false, Message = "Incorrect password" };

        user.LastLoginAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        var token = jwtHelper.GenerateAccessToken(user);

        return new LoginResponse
        {
            Success = true,
            Message = "Login Successful",
            AccessToken = jwtHelper.GenerateAccessToken(user),
            RefreshToken = jwtHelper.GenerateRefreshToken(user),
            FullName = user.FullName
        };
    }

    // 简单的 PBKDF2 验证函数
    private bool VerifyPassword(string password, string storedHash)
    {
        // 格式: {salt}:{hash}
        var parts = storedHash.Split(':');
        if (parts.Length != 2) return false;

        var salt = Convert.FromBase64String(parts[0]);
        var hash = parts[1];

        var derived = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 10000,
            numBytesRequested: 32));

        return hash == derived;
    }
}