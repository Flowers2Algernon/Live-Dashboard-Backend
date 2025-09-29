using System.Text.Json;

namespace LERD.Domain.Entities;

public class UserSavedFilter
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid SurveyId { get; set; }
    public string FilterName { get; set; } = "default";
    public JsonDocument FilterConfiguration { get; set; } = null!;
    public bool IsDefault { get; set; } = true;
    public DateTime LastUsedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
    public Survey Survey { get; set; } = null!;
}
