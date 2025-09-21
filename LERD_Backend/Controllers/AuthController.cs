using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using LERD_Backend.Services;
using LERD.Application.Interfaces;
using LERD.Domain.Entities;
using LERD.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.IdentityModel.Tokens;
using LoginRequest = LERD.Shared.DTOs.LoginRequest;

namespace LERD_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(
        IAuthService authService,
        JwtHelper _jwtHelper,
        IConfiguration _config) : ControllerBase
    {
        private readonly IAuthService authService=authService;
        private readonly JwtHelper _jwtHelper=_jwtHelper;
        private readonly IConfiguration _config=_config;

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var response = await authService.ValidateLoginAsync(request);
            if (!response.Success)
                return Unauthorized(response);

            return Ok(response);
        }

        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // 无状态 JWT 无法在服务端强制失效 Access Token
            // 做法：通知前端删除保存的 token (localStorage / cookies)
            return Ok(new { Message = "Logout successful. Please remove tokens on client side." });
        }
        
        [Authorize]
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var username = User.Identity?.Name;
            var tokenType = User.Claims.FirstOrDefault(c => c.Type == "token_type")?.Value;
            // var orgId = User.Claims.FirstOrDefault(c => c.Type == "organisation_id")?.Value;

            return Ok(new
            {
                UserId = userId,
                Username = username,
                TokenType = tokenType,
                // OrganisationId = orgId,
                Message = "Token Verification successful"
            });
        }

        [HttpPost("refresh")]
        public IActionResult Refresh([FromBody] RefreshRequest request)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]!);

            try
            {
                var principal = tokenHandler.ValidateToken(request.RefreshToken, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidIssuer = _config["Jwt:Issuer"],
                    ValidAudience = _config["Jwt:Audience"],
                    ClockSkew = TimeSpan.Zero
                }, out var validatedToken);

                // 确认是 Refresh Token
                var typeClaim = principal.Claims.FirstOrDefault(c => c.Type == "token_type")?.Value;
                if (typeClaim != "refresh")
                    return Unauthorized(new { Message = "Not Valid Refresh Token" });

                var userId = principal.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
                // var orgId = principal.Claims.First(c => c.Type == "organisation_id").Value;

                var newAccessToken = _jwtHelper.GenerateAccessToken(new User
                {
                    Id = Guid.Parse(userId),
                    // OrganisationId = Guid.Parse(orgId),
                    Username = principal.Identity?.Name ?? "unknown"
                });

                return Ok(new { AccessToken = newAccessToken });
            }
            catch
            {
                return Unauthorized(new { Message = "Invalid or expired Refresh Token" });
            }
        }
    }
}