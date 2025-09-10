namespace LERD.Shared.DTOs 
{
    public class LoginRequest
    {
        public Guid OrganisationId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public string? FullName { get; set; }
    }
}