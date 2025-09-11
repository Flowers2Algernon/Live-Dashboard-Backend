using System.ComponentModel.DataAnnotations;

namespace LERD_Backend.Models
{
    public class LoginRequest1
    {
        [Required]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse1
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Username { get; set; }
    }
}
