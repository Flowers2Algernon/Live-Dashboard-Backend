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
    public class AuthController(IAuthService authService) : ControllerBase
    {
        private readonly JwtHelper _jwtHelper;
        private readonly IConfiguration _config;
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var response = await authService.ValidateLoginAsync(request);
            if (!response.Success)
                return Unauthorized(response);

            return Ok(response);
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            var username = User.Identity?.Name;
            var orgId = User.Claims.FirstOrDefault(c => c.Type == "organisation_id")?.Value;

            return Ok(new
            {
                Username = username,
                OrganisationId = orgId,
                Message = "Token 校验成功"
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
                    return Unauthorized(new { Message = "不是有效的 Refresh Token" });

                var userId = principal.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
                var orgId = principal.Claims.First(c => c.Type == "organisation_id").Value;

                var newAccessToken = _jwtHelper.GenerateAccessToken(new User
                {
                    Id = Guid.Parse(userId),
                    OrganisationId = Guid.Parse(orgId),
                    Username = principal.Identity?.Name ?? "unknown"
                });

                return Ok(new { AccessToken = newAccessToken });
            }
            catch
            {
                return Unauthorized(new { Message = "无效或过期的 Refresh Token" });
            }
        }

    }
    
    
}