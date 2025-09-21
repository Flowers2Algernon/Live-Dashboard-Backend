using Microsoft.AspNetCore.Mvc;
using LERD_Backend.Models;
using LERD_Backend.Services;

namespace LERD_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly ILoginService _loginService;

        public LoginController(ILoginService loginService)
        {
            _loginService = loginService;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequest1 request1)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new LoginResponse1
                {
                    Success = false,
                    Message = "invalid request1 data"
                });
            }

            var result = await _loginService.ValidateLoginAsync(request1);
            
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return Unauthorized(result);
            }
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { message = "login API is working", timestamp = DateTime.Now });
        }
    }
}
